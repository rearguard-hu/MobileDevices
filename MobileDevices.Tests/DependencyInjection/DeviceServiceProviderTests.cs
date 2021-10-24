using Microsoft.Extensions.DependencyInjection;
using MobileDevices.iOS;
using MobileDevices.iOS.DependencyInjection;
using MobileDevices.iOS.Lockdown;
using MobileDevices.iOS.Muxer;
using Moq;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xunit;

namespace MobileDevices.Tests.DependencyInjection
{
    /// <summary>
    /// Tests the <see cref="DeviceServiceProvider"/> class.
    /// </summary>
    public class DeviceServiceProviderTests
    {
        /// <summary>
        /// The <see cref="DeviceServiceProvider"/> construct validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new DeviceServiceProvider(null));
        }

        /// <summary>
        /// <see cref="DeviceServiceProvider.CreateDeviceScopeAsync(string, CancellationToken)"/>
        /// throws a <see cref="MuxerException"/> when no UDID is specified and a no devices are connected.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateDeviceScopeAsync_NoUdid_NoDevice_ThrowsAsync()
        {
            var muxer = new Mock<MuxerClient>(MockBehavior.Strict);
            muxer
                .Setup(m => m.ListDevicesAsync(default))
                .ReturnsAsync(new Collection<MuxerDevice>() { });

            var provider = new ServiceCollection()
                .AddSingleton(muxer.Object)
                .BuildServiceProvider();

            var deviceServiceProvider = new DeviceServiceProvider(provider);

            await Assert.ThrowsAsync<MuxerException>(() => deviceServiceProvider.CreateDeviceScopeAsync((string)null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="DeviceServiceProvider.CreateDeviceScopeAsync(string, CancellationToken)"/>
        /// throws a <see cref="MuxerException"/> when no UDID is specified and more than one device is connected.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateDeviceScopeAsync_NoUdid_TwoDevices_ThrowsAsync()
        {
            var muxer = new Mock<MuxerClient>(MockBehavior.Strict);
            muxer
                .Setup(m => m.ListDevicesAsync(default))
                .ReturnsAsync(new Collection<MuxerDevice>()
                {
                    new MuxerDevice(),
                    new MuxerDevice(),
                });

            var provider = new ServiceCollection()
                .AddSingleton(muxer.Object)
                .AddScoped<DeviceContext>()
                .BuildServiceProvider();

            var deviceServiceProvider = new DeviceServiceProvider(provider);

            await Assert.ThrowsAsync<MuxerException>(() => deviceServiceProvider.CreateDeviceScopeAsync((string)null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="DeviceServiceProvider.CreateDeviceScopeAsync(string, CancellationToken)"/>
        /// returns the connected device when no UDID is specified and only a single device is connected.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateDeviceScopeAsync_NoUdid_SingleDevice_WorksAsync()
        {
            var device = new MuxerDevice() { Udid = "udid" };
            var pairingRecord = new PairingRecord();

            var muxer = new Mock<MuxerClient>(MockBehavior.Strict);
            muxer
                .Setup(m => m.ListDevicesAsync(default))
                .ReturnsAsync(new Collection<MuxerDevice>()
                {
                    device,
                });
            muxer
                .Setup(m => m.ReadPairingRecordAsync("udid", default))
                .ReturnsAsync(pairingRecord);

            var provider = new ServiceCollection()
                .AddSingleton(muxer.Object)
                .AddScoped<DeviceContext>()
                .BuildServiceProvider();

            var deviceServiceProvider = new DeviceServiceProvider(provider);

            using (var scope = await deviceServiceProvider.CreateDeviceScopeAsync((string)null, default).ConfigureAwait(false))
            {
                var context = scope.ServiceProvider.GetRequiredService<DeviceContext>();
                Assert.Same(device, context.Device);
                Assert.Same(pairingRecord, context.PairingRecord);
            }
        }

        /// <summary>
        /// <see cref="DeviceServiceProvider.CreateDeviceScopeAsync(string, CancellationToken)"/>
        /// throws an exception when the requested device is not found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateDeviceScopeAsync_Udid_NoMatch_ThrowsAsync()
        {
            var muxer = new Mock<MuxerClient>(MockBehavior.Strict);
            muxer
                .Setup(m => m.ListDevicesAsync(default))
                .ReturnsAsync(new Collection<MuxerDevice>()
                {
                    new MuxerDevice() { Udid = "1" },
                    new MuxerDevice() { Udid = "2" },
                });

            var provider = new ServiceCollection()
                .AddSingleton(muxer.Object)
                .AddScoped<DeviceContext>()
                .BuildServiceProvider();

            var deviceServiceProvider = new DeviceServiceProvider(provider);

            await Assert.ThrowsAsync<MuxerException>(() => deviceServiceProvider.CreateDeviceScopeAsync("3", default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="DeviceServiceProvider.CreateDeviceScopeAsync(string, CancellationToken)"/>
        /// returns the requested device when available.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateDeviceScopeAsync_Udid_Match_ReturnsDeviceAsync()
        {
            var device = new MuxerDevice() { Udid = "2" };
            var pairingRecord = new PairingRecord();

            var muxer = new Mock<MuxerClient>(MockBehavior.Strict);
            muxer
                .Setup(m => m.ListDevicesAsync(default))
                .ReturnsAsync(new Collection<MuxerDevice>()
                {
                    new MuxerDevice() { Udid = "1" },
                    device,
                });
            muxer
                .Setup(m => m.ReadPairingRecordAsync("2", default))
                .ReturnsAsync(pairingRecord);

            var provider = new ServiceCollection()
                .AddSingleton(muxer.Object)
                .AddScoped<DeviceContext>()
                .BuildServiceProvider();

            var deviceServiceProvider = new DeviceServiceProvider(provider);

            using (var scope = await deviceServiceProvider.CreateDeviceScopeAsync("2", default).ConfigureAwait(false))
            {
                var context = scope.ServiceProvider.GetRequiredService<DeviceContext>();
                Assert.Same(device, context.Device);
                Assert.Same(pairingRecord, context.PairingRecord);
            }
        }
    }
}
