using MobileDevices.iOS.Lockdown;
using MobileDevices.iOS.Muxer;

namespace MobileDevices.iOS
{
    /// <summary>
    /// The <see cref="DeviceContext"/> represents a single connection to an iOS device.
    /// It can be used in session scopes to inject information about the current iOS device
    /// to via the constructor.
    /// </summary>
    public class DeviceContext
    {
        /// <summary>
        /// Gets or sets the device to which the session is scoped.
        /// </summary>
        public MuxerDevice Device { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="PairingRecord"/> for this device, if available.
        /// </summary>
        public PairingRecord PairingRecord { get; set; }
    }
}
