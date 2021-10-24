using MobileDevices.iOS.Muxer;
using Microsoft;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.DependencyInjection
{
    /// <summary>
    /// A <see cref="DeviceServiceProvider"/> is a specialized <see cref="IServiceProvider"/>, which allows creating device scopes,
    /// and connecting to services running on devices.
    /// </summary>
    public class DeviceServiceProvider
    {
        private readonly IServiceProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceServiceProvider"/> class.
        /// </summary>
        /// <param name="provider">
        /// The underlying <see cref="IServiceProvider"/> which provides iOS-related services.
        /// </param>
        public DeviceServiceProvider(IServiceProvider provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceServiceProvider"/> class.
        /// </summary>
        /// <remarks>
        /// Intended for mocking purposes only.
        /// </remarks>
        protected DeviceServiceProvider()
        {
        }

        /// <summary>
        /// Asynchronously create a device scope, which can be used to interact with services running on the iOS device.
        /// </summary>
        /// <param name="udid">
        /// The UDID of the device to which to connect.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and returns a <see cref="IServiceScope"/>
        /// from which device services (such as lockdown service clients) can be sourced.
        /// </returns>
        public virtual async Task<DeviceServiceScope> CreateDeviceScopeAsync(string udid, CancellationToken cancellationToken)
        {
            var muxer = this.provider.GetRequiredService<MuxerClient>();
            var allDevices = await muxer.ListDevicesAsync(cancellationToken).ConfigureAwait(false);

            // The UDID can be null, in which case we select the first device.
            var devices = allDevices.Where(
                d => udid == null
                || string.Equals(d.Udid, udid, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (devices.Length != 1)
            {
                throw new MuxerException($"Could not find the device with udid '{udid}'.");
            }

            return await this.CreateDeviceScopeAsync(devices[0], cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously create a device scope, which can be used to interact with services running on the iOS device.
        /// </summary>
        /// <param name="device">
        /// The device to which to connect.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and returns a <see cref="IServiceScope"/>
        /// from which device services (such as lockdown service clients) can be sourced.
        /// </returns>
        public virtual async Task<DeviceServiceScope> CreateDeviceScopeAsync(MuxerDevice device, CancellationToken cancellationToken)
        {
            Requires.NotNull(device, nameof(device));

            var muxer = this.provider.GetRequiredService<MuxerClient>();
            var scope = this.provider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<DeviceContext>();
            context.Device = device;
            context.PairingRecord = await muxer.ReadPairingRecordAsync(context.Device.Udid, cancellationToken).ConfigureAwait(false);

            return new DeviceServiceScope(scope);
        }
    }
}
