using Claunia.PropertyList;
using System;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Adds the <see cref="DevicePairedMessage.DevicePairedMessage"/> method.
    /// </summary>
    public partial class DevicePairedMessage
    {
        /// <summary>
        /// Reads a <see cref="DevicePairedMessage"/> from a <see cref="NSDictionary"/>.
        /// </summary>
        /// <param name="data">
        /// The message data.
        /// </param>
        /// <returns>
        /// A <see cref="DeviceDetachedMessage"/> object.
        /// </returns>
        public static DevicePairedMessage Read(NSDictionary data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            DevicePairedMessage value = new DevicePairedMessage();
            value.DeviceID = (int)data.Get(nameof(DeviceID)).ToObject();
            value.MessageType = Enum.Parse<MuxerMessageType>((string)data.Get(nameof(MessageType)).ToObject());
            return value;
        }
    }
}
