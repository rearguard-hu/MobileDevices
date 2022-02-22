using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MobileDevices.Buffers;
using MobileDevices.iOS.Services;

namespace MobileDevices.iOS.AfcServices
{
    /// <summary>
    /// AfcService data protocol
    /// </summary>
    public class AfcProtocol : ServiceProtocol
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

        private MemoryOwner ProcessAfcRequest(AfcRequest afcRequest)
        {
            if (string.IsNullOrEmpty(afcRequest.FilePath) && afcRequest.FileHandle == null)
            {
                return new MemoryOwner(MemoryPool.Rent(),0);
            }
            var extraLength = Encoding.UTF8.GetByteCount(afcRequest.FilePath ?? "");
            extraLength += afcRequest.FileHandle == null ? 0 : ExtraLength;
            extraLength += afcRequest.AfcFileMode == null ? 0 : ExtraLength;


            var packet = MemoryPool.Rent(extraLength);

            var tempPacket = packet.Memory.Span;
            if (afcRequest.AfcFileMode != null)
            {
                BinaryPrimitives.WriteUInt64LittleEndian(tempPacket, (ulong)afcRequest.AfcFileMode);
                tempPacket = tempPacket.Slice(8);
            }

            if (afcRequest.FileHandle != null)
            {
                BinaryPrimitives.WriteUInt64LittleEndian(tempPacket, (ulong)afcRequest.FileHandle);
                tempPacket = tempPacket.Slice(8);
            }

            if (!string.IsNullOrEmpty(afcRequest.FilePath))
            {
                Encoding.UTF8.GetBytes(afcRequest.FilePath, tempPacket);
            }

            return new MemoryOwner(packet, extraLength);
        }

        public virtual async Task<bool> WriteMessageAsync(AfcRequest afcRequest, CancellationToken token)
        {
            using var memoryOwner = ProcessAfcRequest(afcRequest);
            if (memoryOwner.ValidLength == 0) return false;

            var extraData = memoryOwner.Memory.Slice(0, memoryOwner.ValidLength);
            var payload = afcRequest.FileData;

            var dataLength = extraData.Length;
            var payloadLength = payload.Length;
            var packetHeard = new AfcPacketHeard()
            {
                Magic = PacketHeaderValue,
                EntireLength = (ulong)(HeaderLen + dataLength + payloadLength),
                PacketNum = _packetNum,
                ThisLength = (ulong)(HeaderLen + dataLength),
                Operation = afcRequest.AfcOperation
            };
            Interlocked.Increment(ref _packetNum);
            var len = HeaderLen + dataLength + payloadLength;
            var packet = Output.GetMemory(len);
            MemoryMarshal.Write(packet.Span, ref packetHeard);
            if (dataLength > 0)
            {
                extraData.Span.CopyTo(packet.Span.Slice(HeaderLen));
            }
            if (payloadLength != 0)
            {
                payload.Span.CopyTo(packet.Span.Slice(HeaderLen + dataLength));
            }

            Output.Advance(len);
            var rt = await Output.FlushAsync(token);
            return !rt.IsCompleted;
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

        public virtual async Task<MemoryOwner> ReceiveRawDataAsync(CancellationToken token)
        {
            try
            {
                var result = await Input.ReadAsync(token);
                var buffer = result.Buffer;
                var len = (int)buffer.Length;
                var packetOwner = MemoryPool.Rent(len);
                buffer.CopyTo(packetOwner.Memory.Span);
                Input.AdvanceTo(buffer.End, buffer.End);

                return new MemoryOwner(packetOwner, len);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex.Message, ex);
            }

            return new MemoryOwner(MemoryPool.Rent(), 0);
        }

        public virtual async Task<(MemoryOwner, AfcPacketHeard)> ReceiveDataAsync(CancellationToken token)
        {
            IMemoryOwner<byte> packetOwner;
            ulong thisLen, entireLength;
            AfcPacketHeard packetHeader;

            using (var owner = await ReadPipeDataAsync(token))
            {
                var memorySpan = owner.Memory;
                packetHeader = MemoryMarshal.Read<AfcPacketHeard>(memorySpan.Span);

                thisLen = (ulong)owner.ValidLength - (ulong)HeaderLen;
                entireLength = packetHeader.EntireLength - (ulong)HeaderLen;

                packetOwner = MemoryPool.Rent((int)entireLength);

                if (thisLen > 0)
                {
                    memorySpan.Span.Slice(HeaderLen, (int)thisLen).CopyTo(packetOwner.Memory.Span);
                }
            }

            var currentCount = thisLen;
            if (entireLength <= thisLen) return (new MemoryOwner(packetOwner, (int)currentCount), packetHeader);

            while ((currentCount < entireLength) && !token.IsCancellationRequested)
            {
                var result = await Input.ReadAsync(token);
                var buffer = result.Buffer;

                buffer.CopyTo(packetOwner.Memory.Span.Slice((int)currentCount));
                currentCount += (ulong)buffer.Length;
                Input.AdvanceTo(buffer.End, buffer.End);
            }

            if (currentCount < entireLength)
            {
                Logger.LogWarning($"could not receive full packet (read {currentCount}, size {entireLength})");
            }
            return (new MemoryOwner(packetOwner, (int)currentCount), packetHeader);
        }

        protected override bool TryParsePacket(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
        {
            line = ReadOnlySequence<byte>.Empty;
            if (buffer.Length < HeaderLen)
            {
                line = ReadOnlySequence<byte>.Empty;
                return false;
            }

            var entireLengthSlice = buffer.Slice(16, 8);
            var entireLength = ReadUInt64LittleEndian(entireLengthSlice);

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
