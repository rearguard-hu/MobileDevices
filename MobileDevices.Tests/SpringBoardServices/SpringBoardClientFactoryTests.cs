using Microsoft.Extensions.Logging.Abstractions;
using MobileDevices.iOS;
using MobileDevices.iOS.Lockdown;
using MobileDevices.iOS.Muxer;
using MobileDevices.iOS.PropertyLists;
using MobileDevices.iOS.SpingBoardServices;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MobileDevices.Tests.SpringBoardServices
{
    /// <summary>
    /// Tests the <see cref="SpringBoardClientFactory"/> class.
    /// </summary>
    public class SpringBoardClientFactoryTests
    {
        /// <summary>
        /// The <see cref="SpringBoardClientFactory"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new SpringBoardClientFactory(null, new DeviceContext(), new PropertyListProtocolFactory(), Mock.Of<LockdownClientFactory>(), NullLogger<SpringBoardClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new SpringBoardClientFactory(Mock.Of<MuxerClient>(), null, new PropertyListProtocolFactory(), Mock.Of<LockdownClientFactory>(), NullLogger<SpringBoardClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new SpringBoardClientFactory(Mock.Of<MuxerClient>(), new DeviceContext(), null, Mock.Of<LockdownClientFactory>(), NullLogger<SpringBoardClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new SpringBoardClientFactory(Mock.Of<MuxerClient>(), new DeviceContext(), new PropertyListProtocolFactory(), null, NullLogger<SpringBoardClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new SpringBoardClientFactory(Mock.Of<MuxerClient>(), new DeviceContext(), new PropertyListProtocolFactory(), Mock.Of<LockdownClientFactory>(), null));
        }

        /// <summary>
        /// The <see cref="SpringBoardClientFactory.CreateAsync(CancellationToken)"/> method returns a properly
        /// configured <see cref="SpringBoardClient"/>.
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
                .Setup(l => l.StartServiceAsync(SpringBoardClient.ServiceName, default))
                .ReturnsAsync(new ServiceDescriptor() { Port = 1234 })
                .Verifiable();

            lockdownClient
                .Setup(l => l.StopSessionAsync(sessionResponse.SessionID, default))
                .Returns(Task.CompletedTask);

            muxerClient
                .Setup(m => m.ConnectAsync(context.Device, 1234, default))
                .ReturnsAsync(Stream.Null)
                .Verifiable();

            var factory = new SpringBoardClientFactory(muxerClient.Object, context, new PropertyListProtocolFactory(), lockdownClientFactory.Object, NullLogger<SpringBoardClient>.Instance);

            await using (var client = await factory.CreateAsync(default).ConfigureAwait(false))
            {
            }

            lockdownClientFactory.Verify();
            lockdownClient.Verify();
            muxerClient.Verify();
        }
    }
}
