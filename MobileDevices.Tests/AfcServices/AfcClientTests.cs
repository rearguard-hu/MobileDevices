using System;
using System.Buffers.Binary;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using MobileDevices.iOS.AfcServices;
using Moq;
using Xunit;

namespace MobileDevices.Tests.AfcServices
{
    public class AfcClientTests
    {
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new AfcClient(Stream.Null, null));
            Assert.Throws<ArgumentNullException>(() => new AfcClient(null, NullLogger<AfcClient>.Instance));

            Assert.Throws<ArgumentNullException>(() => new AfcClient(null));
        }

        /// <summary>
        /// Tests the <see cref="AfcClient.ReadDirectoryAsync(string,CancellationToken)"/> method.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ReadDirectoryAsync_Works_Async()
        {

            var result = await File.ReadAllBytesAsync("AfcServices/afcData.bin");

            var protocol = new Mock<AfcProtocol>();
            await using var client = new AfcClient(protocol.Object);

            protocol
                .Setup(c => c.WriteDataAsync(It.IsAny<AfcRequest>(), default))
                .Callback((AfcRequest pl, CancellationToken ct) =>
                {
                    Assert.Equal("testPath", pl.FilePath);
                    Assert.Equal(AfcOperations.ReadDir, pl.AfcOperation);
                })
                .Returns(Task.FromResult(true));

            protocol
                .Setup(c => c.ReceiveDataAsync(default))
                .ReturnsAsync(result);

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
        public async Task FileOpenAsync_Works_Async()
        {

            var result = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };

            var protocol = new Mock<AfcProtocol>();
            await using var client = new AfcClient(protocol.Object);

            protocol
                .Setup(c => c.WriteDataAsync(It.IsAny<AfcRequest>(), default))
                .Callback((AfcRequest pl, CancellationToken ct) =>
                {
                    Assert.Equal("testPath", pl.FilePath);
                    Assert.Equal(AfcFileMode.FopenRw, pl.AfcFileMode);
                    Assert.Equal(AfcOperations.FileRefOpen, pl.AfcOperation);
                })
                .Returns(Task.FromResult(true));

            protocol
                .Setup(c => c.ReceiveDataAsync(default))
                .ReturnsAsync(result);

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
        public async Task FileCloseAsync_Works_Async()
        {

            var result = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            ulong handle = 1;

            var protocol = new Mock<AfcProtocol>();
            await using var client = new AfcClient(protocol.Object);

            protocol
                .Setup(c => c.WriteDataAsync(It.IsAny<AfcRequest>(), default))
                .Callback((AfcRequest pl, CancellationToken ct) =>
                {
                    Assert.Equal(handle, pl.FileHandle);
                    Assert.Equal(AfcOperations.FileRefClose, pl.AfcOperation);
                })
                .Returns(Task.FromResult(true));

            protocol
                .Setup(c => c.ReceiveDataAsync(default))
                .ReturnsAsync(result);

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
        public async Task FileWriteAsync_Works_Async()
        {

            var result = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            var request = new byte[] { 15, 26, 36 };
            ulong handle = 1;

            var protocol = new Mock<AfcProtocol>();
            await using var client = new AfcClient(protocol.Object);

            protocol
                .Setup(c => c.WriteDataAsync(It.IsAny<AfcRequest>(), default))
                .Callback((AfcRequest pl, CancellationToken ct) =>
                {
                    Assert.Equal(handle, pl.FileHandle);
                    Assert.Equal(AfcOperations.FileRefWrite, pl.AfcOperation);
                })
                .Returns(Task.FromResult(true));

            protocol
                .Setup(c => c.ReceiveDataAsync(default))
                .ReturnsAsync(result);

            var rt = await client.FileWriteAsync(handle, request, request.Length, default).ConfigureAwait(false);

            protocol.Verify();

            Assert.True(rt);

        }

    }
}
