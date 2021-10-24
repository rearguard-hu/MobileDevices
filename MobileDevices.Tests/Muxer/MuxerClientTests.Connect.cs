using MobileDevices.iOS.Muxer;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MobileDevices.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="MuxerClient.TryConnectAsync(MuxerDevice, int, CancellationToken)"/> method.
    /// </summary>
    public partial class MuxerClientTests
    {
        /// <summary>
        /// <see cref="MuxerClient.TryConnectAsync(MuxerDevice, int, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TryConnectAsync_ValidatesArguments_Async()
        {
            var client = new Mock<MuxerClient>();
            client.CallBase = true;

            await Assert.ThrowsAsync<ArgumentNullException>(() => client.Object.TryConnectAsync(null, 1, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="MuxerClient.TryConnectAsync(MuxerDevice, int, CancellationToken)"/> returns <see langword="null"/>
        /// if a connection to the muxer could not be established.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TryConnectAsync_NoMuxer_ReturnsNull_Async()
        {
            var client = new Mock<MuxerClient>();
            client.CallBase = true;
            var device = new MuxerDevice();
            client.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync((MuxerProtocol)null);

            (var error, var stream) = await client.Object.TryConnectAsync(device, 1, default).ConfigureAwait(false);

            Assert.Equal(MuxerError.MuxerError, error);
            Assert.Null(stream);
        }

        /// <summary>
        /// <see cref="MuxerClient.TryConnectAsync(MuxerDevice, int, CancellationToken)"/> returns <see langword="null"/> if the
        /// device prematurely closes the connection.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TryConnectAsync_NoResponse_ReturnsNull_Async()
        {
            var protocol = new Mock<MuxerProtocol>();
            protocol.Setup(p => p.WriteMessageAsync(It.IsAny<MuxerMessage>(), default)).Returns(Task.CompletedTask);
            protocol.Setup(p => p.ReadMessageAsync(default)).ReturnsAsync((MuxerMessage)null);

            var client = new Mock<MuxerClient>();
            client.CallBase = true;
            var device = new MuxerDevice();
            client.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync(protocol.Object);

            (var error, var stream) = await client.Object.TryConnectAsync(device, 1, default).ConfigureAwait(false);

            Assert.Equal(MuxerError.MuxerError, error);
            Assert.Null(stream);
        }

        /// <summary>
        /// <see cref="MuxerClient.TryConnectAsync(MuxerDevice, int, CancellationToken)"/> returns an error if the devie
        /// returns an error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TryConnectAsync_ErrorResponse_ReturnsNull_Async()
        {
            var protocol = new Mock<MuxerProtocol>();
            protocol.Setup(p => p.WriteMessageAsync(It.IsAny<MuxerMessage>(), default)).Returns(Task.CompletedTask);
            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(
                    new ResultMessage()
                    {
                        Number = MuxerError.BadCommand,
                    });

            var client = new Mock<MuxerClient>();
            client.CallBase = true;
            var device = new MuxerDevice();
            client.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync(protocol.Object);

            (var error, var stream) = await client.Object.TryConnectAsync(device, 1, default).ConfigureAwait(false);

            Assert.Equal(MuxerError.BadCommand, error);
            Assert.Null(stream);
        }

        /// <summary>
        /// <see cref="MuxerClient.TryConnectAsync(MuxerDevice, int, CancellationToken)"/> returns a valid <see cref="Stream"/>
        /// if the operation succeeds.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TryConnectAsync_Success_ReturnsStream_Async()
        {
            var protocolStream = new Mock<Stream>();
            var protocol = new Mock<MuxerProtocol>();
            protocol.Setup(p => p.Stream).Returns(protocolStream.Object);
            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<MuxerMessage>(), default))
                .Callback<MuxerMessage, CancellationToken>(
                (message, ct) =>
                {
                    var connectMessage = Assert.IsType<ConnectMessage>(message);

                    Assert.Equal(MuxerMessageType.Connect, connectMessage.MessageType);
                    Assert.Equal(42, connectMessage.DeviceID);

                    // Intential big endian to little endian encoding error.
                    Assert.Equal(256, connectMessage.PortNumber);
                })
                .Returns(Task.CompletedTask);
            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(
                    new ResultMessage()
                    {
                        Number = MuxerError.Success,
                    });

            var client = new Mock<MuxerClient>();
            client.CallBase = true;
            var device = new MuxerDevice() { DeviceID = 42 };
            client.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync(protocol.Object);

            (var error, var stream) = await client.Object.TryConnectAsync(device, 1, default).ConfigureAwait(false);

            Assert.Equal(MuxerError.Success, error);
            Assert.Same(protocolStream.Object, stream);
        }

        /// <summary>
        /// <see cref="MuxerClient.ConnectAsync(MuxerDevice, int, CancellationToken)"/> throws when the connect operation fails.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ConnectAsync_ThrowsOnError_Async()
        {
            var device = new MuxerDevice();

            var muxer = new Mock<MuxerClient>();
            muxer.CallBase = true;

            muxer
                .Setup(m => m.TryConnectAsync(device, 1, default))
                .ReturnsAsync((MuxerError.BadCommand, (Stream)null));

            await Assert.ThrowsAsync<MuxerException>(() => muxer.Object.ConnectAsync(device, 1, default));
        }

        /// <summary>
        /// <see cref="MuxerClient.ConnectAsync(MuxerDevice, int, CancellationToken)"/> returns the underlying <see cref="Stream"/>
        /// when the operation succeeds.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ConnectAsync_Success_ReturnsStream_Async()
        {
            var protocolStream = new Mock<Stream>();
            var device = new MuxerDevice();

            var muxer = new Mock<MuxerClient>();
            muxer.CallBase = true;

            muxer
                .Setup(m => m.TryConnectAsync(device, 1, default))
                .ReturnsAsync((MuxerError.Success, protocolStream.Object));

            var stream = await muxer.Object.ConnectAsync(device, 1, default).ConfigureAwait(false);

            Assert.Equal(protocolStream.Object, stream);
        }
    }
}
