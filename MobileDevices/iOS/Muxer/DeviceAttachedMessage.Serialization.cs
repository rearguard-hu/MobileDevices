using Claunia.PropertyList;
using System;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Contains the serialization methods for the <see cref="DeviceAttachedMessage"/> class.
    /// </summary>
    public partial class DeviceAttachedMessage
    {
        /// <summary>
        /// Reads a <see cref="DeviceAttachedMessage"/> from a <see cref="NSDictionary"/>.
        /// </summary>
        /// <param name="data">
        /// The message data.
        /// </param>
        /// <returns>
        /// A <see cref="DeviceAttachedMessage"/> object.
        /// </returns>
        public static DeviceAttachedMessage Read(NSDictionary data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            DeviceAttachedMessage value = new DeviceAttachedMessage();
            value.DeviceID = (int)data.Get(nameof(DeviceID)).ToObject();
            value.MessageType = Enum.Parse<MuxerMessageType>((string)data.Get(nameof(MessageType)).ToObject());
            value.Properties = DeviceProperties.Read((NSDictionary)data.Get(nameof(Properties)));
            return value;
        }
    }
}
