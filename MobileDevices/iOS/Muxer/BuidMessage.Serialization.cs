using Claunia.PropertyList;
using System;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Contains the serialization methods for the <see cref="BuidMessage"/> class.
    /// </summary>
    public partial class BuidMessage
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
        public static BuidMessage Read(NSDictionary data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            BuidMessage value = new BuidMessage();
            value.BUID = data.GetString(nameof(BUID));
            return value;
        }
    }
}
