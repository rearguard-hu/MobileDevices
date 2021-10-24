using System.Net;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Represents an iOS device which is connect to the host, either via WiFi or USB.
    /// </summary>
    public class MuxerDevice
    {
        /// <summary>
        /// Gets or sets a value indicating how the device is connected to the USB muxer.
        /// </summary>
        public MuxerConnectionType ConnectionType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ID assigned to this device by <c>usbmuxd</c>.
        /// </summary>
        public int DeviceID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the unique ID of this device.
        /// </summary>
        public string Udid
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the IP address of the device. This value is available only if the <see cref="ConnectionType"/>
        /// is <see cref="MuxerConnectionType.Network"/>.
        /// </summary>
        public IPAddress IPAddress
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.Udid;
        }
    }
}
