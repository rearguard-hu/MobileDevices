using Claunia.PropertyList;
using System;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Contains the serialization methods for the <see cref="DeviceProperties"/> class.
    /// </summary>
    public partial class DeviceProperties
    {
        /// <summary>
        /// Reads a <see cref="DeviceProperties"/> from a <see cref="NSDictionary"/>.
        /// </summary>
        /// <param name="data">
        /// The message data.
        /// </param>
        /// <returns>
        /// A <see cref="DeviceProperties"/> object.
        /// </returns>
        public static DeviceProperties Read(NSDictionary data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return new DeviceProperties()
            {
                ConnectionSpeed = data.ContainsKey(nameof(ConnectionSpeed)) ? (int)data.Get(nameof(ConnectionSpeed)).ToObject() : 0,
                ConnectionType = Enum.Parse<MuxerConnectionType>((string)data.Get(nameof(ConnectionType)).ToObject()),
                DeviceID = (int)data.Get(nameof(DeviceID)).ToObject(),
                EscapedFullServiceName = data.ContainsKey(nameof(EscapedFullServiceName)) ? (string)data.Get(nameof(EscapedFullServiceName)).ToObject() : null,
                InterfaceIndex = data.ContainsKey(nameof(InterfaceIndex)) ? (int)data.Get(nameof(InterfaceIndex)).ToObject() : 0,
                LocationID = data.ContainsKey(nameof(LocationID)) ? (int)data.Get(nameof(LocationID)).ToObject() : 0,
                NetworkAddress = data.ContainsKey(nameof(NetworkAddress)) ? (byte[])data.Get(nameof(NetworkAddress)).ToObject() : null,
                ProductID = data.ContainsKey(nameof(ProductID)) ? (int)data.Get(nameof(ProductID)).ToObject() : 0,
                SerialNumber = (string)data.Get(nameof(SerialNumber)).ToObject(),
                UDID = data.ContainsKey(nameof(UDID)) ? (string)data.Get(nameof(UDID)).ToObject() : null,
                USBSerialNumber = data.ContainsKey(nameof(USBSerialNumber)) ? (string)data.Get(nameof(USBSerialNumber))?.ToObject() : null,
            };
        }
    }
}
