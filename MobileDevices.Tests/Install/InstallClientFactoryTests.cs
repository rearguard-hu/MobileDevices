using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using MobileDevices.iOS;
using MobileDevices.iOS.DiagnosticsRelay;
using MobileDevices.iOS.Install;
using MobileDevices.iOS.Lockdown;
using MobileDevices.iOS.Muxer;
using MobileDevices.iOS.PropertyLists;
using Moq;
using Xunit;

namespace MobileDevices.Tests.Install
{
    public class InstallClientFactoryTests
    {
        /// <summary>
        /// The <see cref="InstallClientFactory"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new InstallClientFactory(null, new DeviceContext(), new PropertyListProtocolFactory(), Mock.Of<LockdownClientFactory>(), NullLogger<InstallClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new InstallClientFactory(Mock.Of<MuxerClient>(), null, new PropertyListProtocolFactory(), Mock.Of<LockdownClientFactory>(), NullLogger<InstallClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new InstallClientFactory(Mock.Of<MuxerClient>(), new DeviceContext(), null, Mock.Of<LockdownClientFactory>(), NullLogger<InstallClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new InstallClientFactory(Mock.Of<MuxerClient>(), new DeviceContext(), new PropertyListProtocolFactory(), null, NullLogger<InstallClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new InstallClientFactory(Mock.Of<MuxerClient>(), new DeviceContext(), new PropertyListProtocolFactory(), Mock.Of<LockdownClientFactory>(), null));
        }

        /// <summary>
        /// The <see cref="InstallClientFactory.CreateAsync(CancellationToken)"/> method returns a properly
        /// configured <see cref="InstallClient"/>.
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
                .Setup(l => l.StartServiceAsync(InstallClient.ServiceName, default))
                .ReturnsAsync(new ServiceDescriptor() { Port = 1234 })
                .Verifiable();

            lockdownClient
                .Setup(l => l.StopSessionAsync(sessionResponse.SessionID, default))
                .Returns(Task.CompletedTask);

            muxerClient
                .Setup(m => m.ConnectAsync(context.Device, 1234, default))
                .ReturnsAsync(Stream.Null)
                .Verifiable();

            var factory = new InstallClientFactory(muxerClient.Object, context, new PropertyListProtocolFactory(), lockdownClientFactory.Object, NullLogger<InstallClient>.Instance);

            await using (var client = await factory.CreateAsync(default).ConfigureAwait(false))
            {
            }

            lockdownClientFactory.Verify();
            lockdownClient.Verify();
            muxerClient.Verify();
        }


    }
}
