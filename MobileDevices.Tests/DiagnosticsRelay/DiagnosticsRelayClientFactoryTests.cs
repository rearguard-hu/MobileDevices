using Microsoft.Extensions.Logging.Abstractions;
using MobileDevices.iOS;
using MobileDevices.iOS.DiagnosticsRelay;
using MobileDevices.iOS.Lockdown;
using MobileDevices.iOS.Muxer;
using MobileDevices.iOS.PropertyLists;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace MobileDevices.Tests.DiagnosticsRelay
{
    /// <summary>
    /// Tests the <see cref="DiagnosticsRelayClientFactory"/> class.
    /// </summary>
    public class DiagnosticsRelayClientFactoryTests
    {
        /// <summary>
        /// The <see cref="DiagnosticsRelayClientFactory"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new DiagnosticsRelayClientFactory(null, new DeviceContext(), new PropertyListProtocolFactory(), Mock.Of<LockdownClientFactory>(), NullLogger<DiagnosticsRelayClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new DiagnosticsRelayClientFactory(Mock.Of<MuxerClient>(), null, new PropertyListProtocolFactory(), Mock.Of<LockdownClientFactory>(), NullLogger<DiagnosticsRelayClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new DiagnosticsRelayClientFactory(Mock.Of<MuxerClient>(), new DeviceContext(), null, Mock.Of<LockdownClientFactory>(), NullLogger<DiagnosticsRelayClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new DiagnosticsRelayClientFactory(Mock.Of<MuxerClient>(), new DeviceContext(), new PropertyListProtocolFactory(), null, NullLogger<DiagnosticsRelayClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new DiagnosticsRelayClientFactory(Mock.Of<MuxerClient>(), new DeviceContext(), new PropertyListProtocolFactory(), Mock.Of<LockdownClientFactory>(), null));
        }

        /// <summary>
        /// The <see cref="DiagnosticsRelayClientFactory.CreateAsync(CancellationToken)"/> method returns a properly
        /// configured <see cref="DiagnosticsRelayClient"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateAsync_Works_Async()
        {
            var pairingRecord = new PairingRecord();
            var sessionResponse = new StartSessionResponse() { SessionID = "1234" };
            var lockdownClientFactory = new Mock<LockdownClientFactory>(MockBehavior.Strict);
            var muxerClient = new Mock<MuxerClient>(MockBehavior.Strict);
            var context = new DeviceContext() { Device = new MuxerDevice(), PairingRecord = pairingRecord };

            var lockdownClient = new Mock<LockdownClient>(MockBehavior.Strict);
            lockdownClientFactory
                .Setup(l => l.CreateAsync(default))
                .ReturnsAsync(lockdownClient.Object)
                .Verifiable();

            lockdownClient
                .Setup(l => l.StartSessionAsync(pairingRecord, default))
                .ReturnsAsync(sessionResponse);

            lockdownClient
                .Setup(l => l.StartServiceAsync(DiagnosticsRelayClient.ServiceName, default))
                .ReturnsAsync(new ServiceDescriptor() { Port = 1234 })
                .Verifiable();

            lockdownClient
                .Setup(l => l.StopSessionAsync(sessionResponse.SessionID, default))
                .Returns(Task.CompletedTask);

            muxerClient
                .Setup(m => m.ConnectAsync(context.Device, 1234, default))
                .ReturnsAsync(Stream.Null)
                .Verifiable();

            var factory = new DiagnosticsRelayClientFactory(muxerClient.Object, context, new PropertyListProtocolFactory(), lockdownClientFactory.Object, NullLogger<DiagnosticsRelayClient>.Instance);

            await using (var client = await factory.CreateAsync(default).ConfigureAwait(false))
            {
            }

            lockdownClientFactory.Verify();
            lockdownClient.Verify();
            muxerClient.Verify();
        }
    }
}
