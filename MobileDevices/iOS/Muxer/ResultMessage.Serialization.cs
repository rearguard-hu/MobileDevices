using Claunia.PropertyList;
using System;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Adds the <see cref="ResultMessage.Read(NSDictionary)"/> method.
    /// </summary>
    public partial class ResultMessage
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
        public static ResultMessage Read(NSDictionary data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            ResultMessage value = new ResultMessage();
            value.MessageType = Enum.Parse<MuxerMessageType>((string)data.Get(nameof(MessageType)).ToObject());
            value.Number = (MuxerError)data.Get(nameof(Number)).ToObject();
            return value;
        }
    }
}
