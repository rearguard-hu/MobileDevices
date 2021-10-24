using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MobileDevices.iOS.Services;

namespace MobileDevices.iOS.AfcServices
{
    public class AfcProtocol:ServiceProtocol
    {
        private ulong _packetNum = 1;

        /// <summary>
        /// PacketHeader CFA6LPAA
        /// </summary>
        private const ulong PacketHeaderValue = 4702127774209492547;

        private const int ExtraLength = 8;

        private static readonly int HeaderLen = Marshal.SizeOf(typeof(AfcPacketHeard));


        public AfcProtocol()
        {

        }

        public AfcProtocol(Stream stream, bool ownsStream, ILogger logger)
            : base(stream, ownsStream, logger)
        {
        }

        private (bool, ReadOnlyMemory<byte>) ProcessAfcRequest(AfcRequest afcRequest)
        {
            if (string.IsNullOrEmpty(afcRequest.FilePath) && afcRequest.FileHandle == null)
            {
                return (false, ReadOnlyMemory<byte>.Empty);
            }

            var extraLength = Encoding.UTF8.GetByteCount(afcRequest.FilePath ?? "");
            extraLength += afcRequest.FileHandle == null ? 0 : ExtraLength;
            extraLength += afcRequest.AfcFileMode == null ? 0 : ExtraLength;


            using var packet = this.MemoryPool.Rent(extraLength);

            var tempPacket = packet.Memory.Span;
            if (afcRequest.AfcFileMode != null)
            {
                BinaryPrimitives.WriteUInt64LittleEndian(tempPacket, (ulong)afcRequest.AfcFileMode);
                tempPacket = tempPacket.Slice(8);
            }

            if (afcRequest.FileHandle != null)
                BinaryPrimitives.WriteUInt64LittleEndian(tempPacket, (ulong)afcRequest.FileHandle);

            if (!string.IsNullOrEmpty(afcRequest.FilePath))
                Encoding.UTF8.GetBytes(afcRequest.FilePath).AsSpan().CopyTo(tempPacket);

            return (true, packet.Memory);
        }

        public virtual async Task<bool> WriteDataAsync(AfcRequest afcRequest, CancellationToken token)
        {

            var (isSuccess, extraData) = ProcessAfcRequest(afcRequest);
            if (!isSuccess) return false;

            return !await DispatchPacketAsync(afcRequest.AfcOperation, extraData, afcRequest.FileData, token);
        }

        public async Task<bool> DispatchPacketAsync(AfcOperations operation, ReadOnlyMemory<byte> extraData,
            ReadOnlyMemory<byte> payload, CancellationToken token)
        {
            var dataLength = extraData.Length;

            var payloadLength = payload.Length;

            var packetHeard = new AfcPacketHeard()
            {
                Magic = PacketHeaderValue,
                EntireLength = (ulong)(HeaderLen + dataLength + payloadLength),
                PacketNum = _packetNum,
                ThisLength = (ulong)(HeaderLen + dataLength),
                Operation = operation
            };
            Interlocked.Increment(ref _packetNum);


            var len = HeaderLen + dataLength + payloadLength;
            var packet = Output.GetMemory(len);


            MemoryMarshal.Write(packet.Span, ref packetHeard);
            if (dataLength > 0)
            {
                extraData.CopyTo(packet.Slice(HeaderLen));
            }

            if (payloadLength != 0)
            {
                payload.CopyTo(packet.Slice(HeaderLen + dataLength));
            }


            Output.Advance(len);
            var rt = await Output.FlushAsync(token);
            return rt.IsCompleted;
        }


        public virtual async Task<bool> WriteDataAsync(Memory<byte> data, CancellationToken token)
        {
            var len = data.Length;

            var memory = Output.GetMemory(len);

            data.CopyTo(memory);
            Output.Advance(len);
            var rt = await Output.FlushAsync(token);

            return rt.IsCompleted;
        }

        /// <summary>
        /// 获取原始数据
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual async Task<ReadOnlyMemory<byte>> ReceiveRawDataAsync(CancellationToken token)
        {
            var result = await Input.ReadAsync(token);
            var buffer = result.Buffer;

            var len = (int)buffer.Length;
            using var memory = MemoryPool.Rent(len);
            buffer.CopyTo(memory.Memory.Span);


            Input.AdvanceTo(buffer.Start, buffer.End);

            return memory.Memory[..len];
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual async Task<ReadOnlyMemory<byte>> ReceiveDataAsync(CancellationToken token)
        {
            var memory = await ReadPipeDataAsync(token);

            var packetHeader = MemoryMarshal.Read<AfcPacketHeard>(memory.Span);

            //当前长度
            var currentLength = (ulong)memory.Length - (ulong)HeaderLen;
            //总长度
            var entireLength = packetHeader.EntireLength - (ulong)HeaderLen;


            using var packet = this.MemoryPool.Rent((int)entireLength);

            memory.Slice(HeaderLen).CopyTo(packet.Memory);

            while ((currentLength < entireLength) && !token.IsCancellationRequested)
            {
                var tempBuffer = await ReadPipeDataAsync(token);

                tempBuffer.Slice(HeaderLen).CopyTo(packet.Memory.Slice((int)currentLength));

                currentLength += (ulong)(tempBuffer.Length - HeaderLen);
            }

            if (currentLength < entireLength)
            {
                this.Logger.LogWarning($"could not receive full packet (read {currentLength}, size {entireLength})");
            }

            return packet.Memory.Slice(0, (int)currentLength);

        }

        /// <summary>
        /// 单个afc包
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        protected override bool TryParsePacket(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
        {
            line = ReadOnlySequence<byte>.Empty;
            if (buffer.Length < HeaderLen)
            {
                line = ReadOnlySequence<byte>.Empty;
                return false;
            }

            //afc 包总长
            var entireLengthSlice = buffer.Slice(8, 8);
            var entireLength = ReadUInt64LittleEndian(entireLengthSlice);

            //当前包是否够
            if (entireLength > (ulong)buffer.Length)
            {
                line = ReadOnlySequence<byte>.Empty;
                return false;
            }

            line = buffer.Slice(buffer.Start, (int)entireLength);
            buffer = buffer.Slice(line.End);

            return true;
        }


    }

}
