using Microsoft.Extensions.Logging.Abstractions;
using MobileDevices.iOS;
using MobileDevices.iOS.Lockdown;
using MobileDevices.iOS.Muxer;
using MobileDevices.Tests;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MobileDevices.Tests.Lockdown
{
    /// <summary>
    /// Tests the <see cref="LockdownClientFactory"/> class.
    /// </summary>
    public class LockdownClientFactoryTests
    {
        /// <summary>
        /// <see cref="LockdownClientFactory"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new LockdownClientFactory(null, Mock.Of<DeviceContext>(), NullLogger<LockdownClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new LockdownClientFactory(Mock.Of<MuxerClient>(), null, NullLogger<LockdownClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new LockdownClientFactory(Mock.Of<MuxerClient>(), Mock.Of<DeviceContext>(), null));
        }

        /// <summary>
        /// The <see cref="LockdownClientFactory.CreateAsync(CancellationToken)"/> method works.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Connect_Works_Async()
        {
            // Sample traffic from https://www.theiphonewiki.com/wiki/Usbmux ("lockdownd protocol")
            var muxer = new Mock<MuxerClient>();
            var device = new MuxerDevice();

            using (var traceStream = new TraceStream("Lockdown/connect-device.bin", "Lockdown/connect-host.bin"))
            {
                muxer
                    .Setup(m => m.ConnectAsync(device, 0xF27E, default))
                    .ReturnsAsync(traceStream);

                var factory = new LockdownClientFactory(muxer.Object, new DeviceContext() { Device = device }, NullLogger<LockdownClient>.Instance);

                await using (await factory.CreateAsync(default))
                {
                    // The trace stream will assert the correct data is exchanged.
                }
            }
        }

        /// <summary>
        /// The <see cref="LockdownClientFactory.CreateAsync(CancellationToken)"/> method works.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Connect_InvalidResponse_Throws_Async()
        {
            // Sample traffic from https://www.theiphonewiki.com/wiki/Usbmux ("lockdownd protocol")
            var muxer = new Mock<MuxerClient>();
            var device = new MuxerDevice();

            using (var traceStream = new TraceStream("Lockdown/connect-device-invalid.bin", "Lockdown/connect-host.bin"))
            {
                muxer
                    .Setup(m => m.ConnectAsync(device, 0xF27E, default))
                    .ReturnsAsync(traceStream);

                var factory = new LockdownClientFactory(muxer.Object, new DeviceContext() { Device = device }, NullLogger<LockdownClient>.Instance);

                await Assert.ThrowsAsync<LockdownException>(() => factory.CreateAsync(default)).ConfigureAwait(false);
            }
        }
    }
}
