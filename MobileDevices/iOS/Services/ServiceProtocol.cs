using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.Extensions.Logging;

namespace MobileDevices.iOS.Services
{

    public partial class ServiceProtocol : IAsyncDisposable, IDisposableObservable
    {

        private readonly Stream rawStream;

        private readonly bool ownsStream;

        protected readonly MemoryPool<byte> MemoryPool = MemoryPool<byte>.Shared;

        protected readonly ILogger Logger;

        private Stream stream;

        public virtual Stream Stream
        {
            get => stream;
            set
            {
                stream = value;

                pipeReader = null;
                pipeWriter = null;
            }
        }

        private PipeWriter pipeWriter;

        private PipeReader pipeReader;

        /// <summary>
        /// A PipeWriter adapted over the given stream.
        /// </summary>
        public PipeWriter Output
        {
            get
            {
                if (pipeWriter == null)
                {
                    pipeWriter = PipeWriter.Create(Stream, new StreamPipeWriterOptions(leaveOpen: true));
                }
                return pipeWriter;
            }
        }

        public PipeReader Input
        {
            get
            {
                if (pipeReader == null)
                {
                    pipeReader = PipeReader.Create(Stream, new StreamPipeReaderOptions(leaveOpen: true));
                }
                return pipeReader;
            }
        }


        /// <inheritdoc/>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stream.ServiceProtocol"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents the connection to the muxer.
        /// </param>
        /// <param name="ownsStream">
        /// A value indicating whether this <see cref="iOS.Services.ServiceProtocol"/> instance owns the <paramref name="stream"/> or not.
        /// </param>
        /// <param name="logger">
        /// A <see cref="iOS"/> which can be used when logging.
        /// </param>
        public ServiceProtocol(Stream stream, bool ownsStream, ILogger logger)
        {
            this.rawStream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.stream = this.rawStream;
            this.ownsStream = ownsStream;
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ServiceProtocol()
        {
            
        }

        protected virtual bool TryParsePacket(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
        {
            throw new NotImplementedException();
        }

        protected virtual Task<bool> PacketProcessAsync(Memory<byte> writer, ReadOnlySequence<byte> packet, out int writerLength, CancellationToken token)
        {
            packet.CopyTo(writer.Span);
            writerLength = (int)packet.Length;
            return Task.FromResult(true);
        }

        protected virtual async Task<ReadOnlyMemory<byte>> ReadPipeDataAsync(CancellationToken token)
        {
            var onlyMemory = ReadOnlyMemory<byte>.Empty;
            while (!token.IsCancellationRequested)
            {
                var result = await Input.ReadAsync(token);
                var buffer = result.Buffer;

                var readLength = 0;
                var proc = false;
                using var memory = MemoryPool.Rent((int)buffer.Length);
                try
                {
                    while (TryParsePacket(ref buffer, out var packet))
                    {
                        var rt = await PacketProcessAsync(memory.Memory, packet, out readLength, token);

                        readLength = (int)packet.Length;
                        proc = true;

                        if (rt) break;
                    }

                    Input.AdvanceTo(buffer.Start, buffer.End);
                    if (result.IsCompleted)
                        break;

                    if (buffer.Length <= 0|| proc)
                        break;

                }
                finally
                {
                    onlyMemory = memory.Memory[..readLength];
                }
            }
            return onlyMemory;
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
            if (this.stream != this.rawStream)
            {
                await this.stream.DisposeAsync().ConfigureAwait(false);
            }

            if (this.rawStream != null)
            {
                await this.rawStream.DisposeAsync().ConfigureAwait(false);
            }

            this.IsDisposed = true;
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            if (this.stream != this.rawStream)
            {
                this.stream.Dispose();
            }

            if (this.stream != null)
            {
                this.rawStream.Dispose();
            }

            this.IsDisposed = true;
        }


    }
}
