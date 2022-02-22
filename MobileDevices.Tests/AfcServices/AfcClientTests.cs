using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using MobileDevices.Buffers;
using MobileDevices.iOS.AfcServices;
using Moq;
using Xunit;

namespace MobileDevices.Tests.AfcServices
{
    public class AfcClientTests
    {
        [Fact]
        public void ConstructorArgumentsTest()
        {
            Assert.Throws<ArgumentNullException>(() => new AfcClient(Stream.Null, null));
            Assert.Throws<ArgumentNullException>(() => new AfcClient(null, NullLogger<AfcClient>.Instance));

            Assert.Throws<ArgumentNullException>(() => new AfcClient(null));
        }

        private (MemoryOwner, AfcPacketHeard) GetBytes(Span<byte> bytes)
        {
            var owner = MemoryPool<byte>.Shared.Rent(bytes.Length);
            bytes.CopyTo(owner.Memory.Span);
            var afcPacket = new AfcPacketHeard
            {
                EntireLength = (ulong)(bytes.Length + 40)
            };

            return (new MemoryOwner(owner, bytes.Length), afcPacket);
        }

        /// <summary>
        /// Tests the <see cref="AfcClient.ReadDirectoryAsync(string,CancellationToken)"/> method.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ReadDirectoryAsyncTest_Async()
        {

            var result = await File.ReadAllBytesAsync("iOS/AfcServices/afcData.bin");

            var (packet, packetHeader) = GetBytes(result);

            var protocol = new Mock<AfcProtocol>();
            await using var client = new AfcClient(protocol.Object);

            protocol
                .Setup(c => c.WriteMessageAsync(It.IsAny<AfcRequest>(), default))
                .Callback((AfcRequest pl, CancellationToken ct) =>
                {
                    Assert.Equal("testPath", pl.FilePath);
                    Assert.Equal(AfcOperations.ReadDir, pl.AfcOperation);
                })
                .Returns(Task.FromResult(true));

            protocol
                .Setup(c => c.ReceiveDataAsync(default))
                .ReturnsAsync((packet, packetHeader));

            await client.ReadDirectoryAsync("testPath", default).ConfigureAwait(false);

            protocol.Verify();
        }

        /// <summary>
        /// Tests the <see cref="AfcClient.FileOpenAsync(string, AfcFileMode, CancellationToken)"/> method.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task FileOpenAsyncTest_Async()
        {

            var result = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };

            var (packet, packetHeader) = GetBytes(result);

            var protocol = new Mock<AfcProtocol>();
            await using var client = new AfcClient(protocol.Object, NullLogger<AfcClient>.Instance);

            protocol
                .Setup(c => c.WriteMessageAsync(It.IsAny<AfcRequest>(), default))
                .Callback((AfcRequest pl, CancellationToken ct) =>
                {
                    Assert.Equal("testPath", pl.FilePath);
                    Assert.Equal(AfcFileMode.FopenRw, pl.AfcFileMode);
                    Assert.Equal(AfcOperations.FileRefOpen, pl.AfcOperation);
                })
                .Returns(Task.FromResult(true));

            protocol
                .Setup(c => c.ReceiveDataAsync(default))
                .ReturnsAsync((packet, packetHeader));

            var rt = await client.FileOpenAsync("testPath", AfcFileMode.FopenRw, default).ConfigureAwait(false);
            protocol.Verify();

            Assert.Equal(BinaryPrimitives.ReadUInt64LittleEndian(result), rt);

        }

        /// <summary>
        /// Tests the <see cref="AfcClient.FileCloseAsync(ulong, CancellationToken)"/> method.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task FileCloseAsyncTest_Async()
        {

            var result = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            ulong handle = 1;
            var (packet, packetHeader) = GetBytes(result);
            var protocol = new Mock<AfcProtocol>();
            await using var client = new AfcClient(protocol.Object);

            protocol
                .Setup(c => c.WriteMessageAsync(It.IsAny<AfcRequest>(), default))
                .Callback((AfcRequest pl, CancellationToken ct) =>
                {
                    Assert.Equal(handle, pl.FileHandle);
                    Assert.Equal(AfcOperations.FileRefClose, pl.AfcOperation);
                })
                .Returns(Task.FromResult(true));

            protocol
                .Setup(c => c.ReceiveDataAsync(default))
                .ReturnsAsync((packet, packetHeader));

            var rt = await client.FileCloseAsync(handle, default).ConfigureAwait(false);
            protocol.Verify();

            Assert.Equal(BinaryPrimitives.ReadUInt64LittleEndian(result), rt);

        }

        /// <summary>
        /// Tests the <see cref="AfcClient.FileWriteAsync(ulong, ReadOnlyMemory{byte}, int, CancellationToken)"/> method.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task FileWriteAsyncTest_Async()
        {

            var result = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            var request = new byte[] { 15, 26, 36 };
            ulong handle = 1;
            var (packet, packetHeader) = GetBytes(result);

            var protocol = new Mock<AfcProtocol>();
            await using var client = new AfcClient(protocol.Object);

            protocol
                .Setup(c => c.WriteMessageAsync(It.IsAny<AfcRequest>(), default))
                .Callback((AfcRequest pl, CancellationToken ct) =>
                {
                    Assert.Equal(handle, pl.FileHandle);
                    Assert.Equal(AfcOperations.FileRefWrite, pl.AfcOperation);
                })
                .Returns(Task.FromResult(true));

            protocol
                .Setup(c => c.ReceiveDataAsync(default))
                .ReturnsAsync((packet, packetHeader));

            var rt = await client.FileWriteAsync(handle, request, request.Length, default).ConfigureAwait(false);

            protocol.Verify();

            Assert.True(rt);

        }

    }
}
