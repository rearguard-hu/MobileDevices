using System;
using System.Net;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Contains properties of a device which is connected to the host over USB.
    /// </summary>
    public partial class DeviceProperties
    {
        /// <summary>
        /// Gets or sets the speed of the USB connection.
        /// </summary>
        public int ConnectionSpeed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of connection. By default, this value is <c>USB</c>; for WiFi-connected devices
        /// this can be <c>Network</c>.
        /// </summary>
        public MuxerConnectionType ConnectionType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ID of the device, as assigned by <c>usbmuxd</c>. This is not the UDID.
        /// </summary>
        public int DeviceID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the serial number, or UDID, of the device.
        /// </summary>
        public string SerialNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the UDID of the device. This property is available starting with macOS Catalina 10.15.1.
        /// </summary>
        public string UDID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the USB serial number of the device. This is often the same
        /// as the <see cref="SerialNumber"/>. However, for devices with special
        /// characters in their UDID (such as dashes), the USB serial number may
        /// differ from the UDID. This property is available starting with macOS Catalina 10.15.
        /// </summary>
        public string USBSerialNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the location ID of the device. Used for USB-connected devices.
        /// </summary>
        public int LocationID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the USB ID of the product. This typically relates to a iPhone or iPad model. Used for USB-connected devices.
        /// </summary>
        public int ProductID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Bonjour service name for this device. Used for WiFi-connected devices.
        /// </summary>
        public string EscapedFullServiceName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the index of the network interface used to communicate with this device. Used for
        /// WiFi-connected devices.
        /// </summary>
        public int InterfaceIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the network address of the device. Used for WiFi-connected devices.
        /// </summary>
        public byte[] NetworkAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the IP address of the device. Used for WiFi-connected devices.
        /// </summary>
        public IPAddress IPAddress
        {
            get
            {
                const int AF_INET = 0x02;
                const int AF_INET6 = 0x1E;

                if (this.NetworkAddress == null || this.NetworkAddress.Length == 0)
                {
                    return null;
                }

                switch (this.NetworkAddress[0])
                {
                    case AF_INET:
                        return new IPAddress(this.NetworkAddress.AsSpan(4, 4));

                    case AF_INET6:
                        return new IPAddress(this.NetworkAddress.AsSpan(4, 16));

                    default:
                        return null;
                }
            }
        }
    }
}