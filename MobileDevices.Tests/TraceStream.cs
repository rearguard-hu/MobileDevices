using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MobileDevices.Tests
{
    /// <summary>
    /// A <see cref="Stream"/> which reads input from trace files, and asserts that the output is
    /// equal to the output captured in trace files.
    /// </summary>
    internal class TraceStream : Stream
    {
        private readonly Stream input;
        private readonly Stream expectedOutput;
        private readonly MemoryPool<byte> memoryPool = MemoryPool<byte>.Shared;

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceStream"/> class.
        /// </summary>
        /// <param name="inputTrace">
        /// The path to the input trace file.
        /// </param>
        /// <param name="outputTrace">
        /// The path to the output trace file.
        /// </param>
        public TraceStream(string inputTrace, string outputTrace)
            : this(File.OpenRead(inputTrace), File.OpenRead(outputTrace))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceStream"/> class.
        /// </summary>
        /// <param name="input">
        /// A <see cref="Stream"/> which represents the input.
        /// </param>
        /// <param name="expectedOutput">
        /// A <see cref="Stream"/> which represents the output.
        /// </param>
        public TraceStream(Stream input, Stream expectedOutput)
        {
            this.input = input ?? throw new ArgumentNullException(nameof(input));
            this.expectedOutput = expectedOutput ?? throw new ArgumentNullException(nameof(expectedOutput));
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override long Length => throw new NotSupportedException();

        /// <inheritdoc/>
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Flush()
        {
        }

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return this.input.ReadAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc/>
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return this.input.ReadAsync(buffer, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return this.WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
        }

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            using (var readBuffer = this.memoryPool.Rent(buffer.Length))
            {
                var memory = readBuffer.Memory.Slice(0, buffer.Length);
                await this.expectedOutput.ReadAsync(memory).ConfigureAwait(false);
                Assert.Equal(memory.ToArray(), buffer.ToArray());
            }
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.input.Dispose();
            this.expectedOutput.Dispose();
        }
    }
}
