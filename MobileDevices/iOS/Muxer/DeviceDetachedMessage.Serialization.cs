using Claunia.PropertyList;
using System;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Adds the <see cref="DeviceDetachedMessage.Read(NSDictionary)"/> method.
    /// </summary>
    public partial class DeviceDetachedMessage
    {
        /// <summary>
        /// Reads a <see cref="DeviceDetachedMessage"/> from a <see cref="NSDictionary"/>.
        /// </summary>
        /// <param name="data">
        /// The message data.
        /// </param>
        /// <returns>
        /// A <see cref="DeviceDetachedMessage"/> object.
        /// </returns>
        public static DeviceDetachedMessage Read(NSDictionary data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            DeviceDetachedMessage value = new DeviceDetachedMessage();
            value.DeviceID = (int)data.Get(nameof(DeviceID)).ToObject();
            value.MessageType = Enum.Parse<MuxerMessageType>((string)data.Get(nameof(MessageType)).ToObject());
            return value;
        }
    }
}
