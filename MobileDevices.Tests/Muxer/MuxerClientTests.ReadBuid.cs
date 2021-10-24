using MobileDevices.iOS.Muxer;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MobileDevices.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="MuxerClient.ReadBuidAsync(CancellationToken)"/> method.
    /// </summary>
    public partial class MuxerClientTests
    {
        /// <summary>
        /// <see cref="MuxerClient.ReadBuidAsync(CancellationToken)"/> returns <see langword="null"/> when a connection
        /// with the muxer could not be established.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReadBuid_NoConnection_ReturnsNull_Async()
        {
            var client = new Mock<MuxerClient>();
            client.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync((MuxerProtocol)null);

            Assert.Null(await client.Object.ReadBuidAsync(default).ConfigureAwait(false));
        }

        /// <summary>
        /// <see cref="MuxerClient.ReadBuidAsync(CancellationToken)"/> returns the BUID.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReadBuid_ReturnsBuid_Async()
        {
            var protocol = new Mock<MuxerProtocol>();
            var client = new Mock<MuxerClient>();
            client.Setup(c => c.ReadBuidAsync(default)).CallBase();
            client.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync(protocol.Object);

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<MuxerMessage>(), default))
                .Callback<MuxerMessage, CancellationToken>(
                (message, ct) =>
                {
                    Assert.Equal(MuxerMessageType.ReadBUID, message.MessageType);
                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(new BuidMessage()
                {
                    BUID = "1234",
                });

            Assert.Equal("1234", await client.Object.ReadBuidAsync(default).ConfigureAwait(false));
        }
    }
}
