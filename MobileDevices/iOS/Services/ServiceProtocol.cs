using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.Extensions.Logging;
using MobileDevices.Buffers;
using MobileDevices.iOS.AfcServices;

namespace MobileDevices.iOS.Services
{

    public partial class ServiceProtocol : IAsyncDisposable, IDisposableObservable
    {

        private readonly Stream _rawStream;

        private readonly bool _ownsStream;

        protected readonly MemoryPool<byte> MemoryPool = MemoryPool<byte>.Shared;

        protected readonly ILogger Logger;

        private Stream _stream;

        public virtual Stream Stream {
            get => _stream;
            set {
                _stream = value;

                _pipeReader = null;
                _pipeWriter = null;
            }
        }

        private PipeWriter _pipeWriter;

        private PipeReader _pipeReader;

        /// <summary>
        /// A PipeWriter adapted over the given stream.
        /// </summary>
        public PipeWriter Output {
            get {
                if (_pipeWriter is null)
                    _pipeWriter = PipeWriter.Create(Stream, new StreamPipeWriterOptions(leaveOpen: true));

                return _pipeWriter;
            }
        }

        public PipeReader Input {
            get {
                if (_pipeReader is null)
                    _pipeReader = PipeReader.Create(Stream, new StreamPipeReaderOptions(leaveOpen: true));

                return _pipeReader;
            }
        }

        /// <inheritdoc/>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProtocol"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents the connection to the muxer.
        /// </param>
        /// <param name="ownsStream">
        /// A value indicating whether this <see cref="ServiceProtocol"/> instance owns the <paramref name="stream"/> or not.
        /// </param>
        /// <param name="logger">
        /// A <see cref="iOS"/> which can be used when logging.
        /// </param>
        public ServiceProtocol(Stream stream, bool ownsStream, ILogger logger)
        {
            _rawStream = stream ?? throw new ArgumentNullException(nameof(stream));
            _stream = _rawStream;
            _ownsStream = ownsStream;
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ServiceProtocol()
        {

        }

        /// <summary>
        /// Asynchronously sends a message to the remote client.
        /// </summary>
        /// <param name="message">
        /// The message to send.
        /// </param>
        /// <param name="token">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public virtual async Task WriteMessageAsync(string message, CancellationToken token)
        {
            Verify.NotDisposed(this);

            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message));
            }

            var messageLength = Encoding.UTF8.GetByteCount(message);
            var packetLength = 4 + messageLength;

            var memory = Output.GetMemory(packetLength);

            // Construct the entire packet:
            // [length] (4 bytes)
            // [UTF-8 XML-encoded property list message] (N bytes)
            BinaryPrimitives.WriteInt32BigEndian(memory.Span, messageLength);
            Encoding.UTF8.GetBytes(message, memory.Span.Slice(4));
            Output.Advance(packetLength);
            await Output.FlushAsync(token);
        }

        /// <summary>
        /// Parsing packet
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        protected virtual bool TryParsePacket(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
        {
            if (buffer.Length < 4)
            {
                line = ReadOnlySequence<byte>.Empty;
                return false;
            }

            var lengthSlice = buffer.Slice(buffer.Start, 4);

            var length = ReadInt32BigEndian(lengthSlice);

            //判断 流的长度是不是够
            if (length > buffer.Length - 4)
            {
                line = ReadOnlySequence<byte>.Empty;
                return false;
            }

            line = buffer.Slice(lengthSlice.End, length);
            buffer = buffer.Slice(line.End);

            return true;
        }

        /// <summary>
        /// 数据包处理
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="packet"></param>
        /// <param name="writerLength"></param>
        /// <returns></returns>
        protected virtual bool PacketProcess(Memory<byte> writer, ReadOnlySequence<byte> packet, out int writerLength)
        {
            packet.CopyTo(writer.Span);
            writerLength = (int)packet.Length;
            return true;
        }

        protected virtual async Task<MemoryOwner> ReadPipeDataAsync(CancellationToken token)
        {
            IMemoryOwner<byte> owner = default;
            var readLength = 0;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var result = await Input.ReadAsync(token);
                    var buffer = result.Buffer;
                    while (TryParsePacket(ref buffer, out var packet))
                    {
                        owner = MemoryPool.Rent((int)packet.Length);
                        var rt = PacketProcess(owner.Memory, packet, out readLength);

                        if (rt) break;
                    }

                    Input.AdvanceTo(buffer.Start, buffer.End);
                    if (result.IsCompleted)
                        break;

                    if (buffer.Length <= 0 || readLength > 0)
                        break;

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, ex.Message);
                    break;
                }
            }

            return owner == default
                ? new MemoryOwner(MemoryPool.Rent(), readLength)
                : new MemoryOwner(owner, readLength);
        }

        /// <summary>
        /// Read the 32bit Big Endian length
        /// </summary>
        /// <param name="lengthSlice"></param>
        /// <returns></returns>
        protected static int ReadInt32BigEndian(ReadOnlySequence<byte> lengthSlice)
        {
            int length;
            if (lengthSlice.IsSingleSegment)
            {
                // Fast path since it's a single segment.
                length = BinaryPrimitives.ReadInt32BigEndian(lengthSlice.First.Span);
            }
            else
            {
                // There are 4 bytes split across multiple segments. Since it's so small, it
                // can be copied to a stack allocated buffer. This avoids a heap allocation.
                Span<byte> stackBuffer = stackalloc byte[4];
                lengthSlice.CopyTo(stackBuffer);

                length = BinaryPrimitives.ReadInt32BigEndian(stackBuffer);
            }

            return length;
        }

        /// <summary>
        /// Read the 64bit Big Endian length
        /// </summary>
        /// <param name="lengthSlice"></param>
        /// <returns></returns>
        protected static ulong ReadUInt64BigEndian(ReadOnlySequence<byte> lengthSlice)
        {
            ulong length;
            if (lengthSlice.IsSingleSegment)
            {
                // Fast path since it's a single segment.
                length = BinaryPrimitives.ReadUInt64BigEndian(lengthSlice.First.Span);
            }
            else
            {
                // There are 4 bytes split across multiple segments. Since it's so small, it
                // can be copied to a stack allocated buffer. This avoids a heap allocation.
                Span<byte> stackBuffer = stackalloc byte[8];
                lengthSlice.CopyTo(stackBuffer);

                length = BinaryPrimitives.ReadUInt64BigEndian(stackBuffer);
            }

            return length;
        }

        protected static ulong ReadUInt64LittleEndian(ReadOnlySequence<byte> lengthSlice)
        {
            ulong length;
            if (lengthSlice.IsSingleSegment)
            {
                // Fast path since it's a single segment.
                length = BinaryPrimitives.ReadUInt64LittleEndian(lengthSlice.First.Span);
            }
            else
            {
                // There are 4 bytes split across multiple segments. Since it's so small, it
                // can be copied to a stack allocated buffer. This avoids a heap allocation.
                Span<byte> stackBuffer = stackalloc byte[8];
                lengthSlice.CopyTo(stackBuffer);

                length = BinaryPrimitives.ReadUInt64LittleEndian(stackBuffer);
            }

            return length;
        }


        /// <inheritdoc/>
        public virtual async ValueTask DisposeAsync()
        {
            if (_stream != _rawStream)
            {
                await _stream.DisposeAsync().ConfigureAwait(false);
            }

            if (_rawStream != null)
            {
                await _rawStream.DisposeAsync().ConfigureAwait(false);
            }

            IsDisposed = true;
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            if (_stream != _rawStream)
            {
                _stream.Dispose();
            }

            if (_stream != null)
            {
                _rawStream.Dispose();
            }

            IsDisposed = true;
        }


    }
}
