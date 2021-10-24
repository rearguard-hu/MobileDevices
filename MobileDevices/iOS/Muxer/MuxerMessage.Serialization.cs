using Claunia.PropertyList;
using System;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Adds the <see cref="MuxerMessage.ReadAny(NSDictionary)"/> method.
    /// </summary>
    public partial class MuxerMessage
    {
        /// <summary>
        /// Reads a <see cref="MuxerMessage"/> object from a <see cref="NSDictionary"/> value.
        /// </summary>
        /// <param name="data">
        /// The data to read.
        /// </param>
        /// <returns>
        /// The <see cref="MuxerMessage"/> representation of the <paramref name="data"/>.
        /// </returns>
        public static MuxerMessage ReadAny(NSDictionary data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.ContainsKey(nameof(MessageType)))
            {
                var messageType = Enum.Parse<MuxerMessageType>((string)data.Get(nameof(MessageType)).ToObject());

                switch (messageType)
                {
                    case MuxerMessageType.Attached:
                        return DeviceAttachedMessage.Read(data);

                    case MuxerMessageType.Detached:
                        return DeviceDetachedMessage.Read(data);

                    case MuxerMessageType.Paired:
                        return DevicePairedMessage.Read(data);

                    case MuxerMessageType.Result:
                        return ResultMessage.Read(data);

                    default:
                        throw new ArgumentOutOfRangeException(nameof(data));
                }
            }
            else if (data.ContainsKey("DeviceList"))
            {
                return DeviceListMessage.Read(data);
            }
            else if (data.ContainsKey("BUID"))
            {
                return BuidMessage.Read(data);
            }
            else if (data.ContainsKey("PairRecordData"))
            {
                return PairingRecordDataMessage.Read(data);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(data));
            }
        }
    }
}
