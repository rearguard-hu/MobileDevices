using Claunia.PropertyList;
using System;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Contains serialization methods for the <see cref="DeviceListMessage"/> class.
    /// </summary>
    public partial class DeviceListMessage
    {
        /// <summary>
        /// Reads a <see cref="DeviceListMessage"/> from a <see cref="NSDictionary"/>.
        /// </summary>
        /// <param name="data">
        /// The message data.
        /// </param>
        /// <returns>
        /// A <see cref="DeviceListMessage"/> object.
        /// </returns>
        public static DeviceListMessage Read(NSDictionary data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var value = new DeviceListMessage();
            var deviceList = (NSArray)data.Get(nameof(DeviceList));

            foreach (var entry in deviceList)
            {
                value.DeviceList.Add(DeviceAttachedMessage.Read((NSDictionary)entry));
            }

            return value;
        }
    }
}
