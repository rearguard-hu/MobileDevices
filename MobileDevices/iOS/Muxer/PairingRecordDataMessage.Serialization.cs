using Claunia.PropertyList;
using System;

namespace MobileDevices.iOS.Muxer
{
    /// <content>
    /// Serialization-related methods for the <see cref="PairingRecordDataMessage"/> class.
    /// </content>
    public partial class PairingRecordDataMessage
    {
        /// <summary>
        /// Reads a <see cref="PairingRecordDataMessage"/> from a <see cref="NSDictionary"/>.
        /// </summary>
        /// <param name="dict">
        /// The <see cref="NSDictionary"/> from which to read the data.
        /// </param>
        /// <returns>
        /// A <see cref="PairingRecordDataMessage"/> which represens the server response.
        /// </returns>
        public static new PairingRecordDataMessage Read(NSDictionary dict)
        {
            if (dict == null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            return new PairingRecordDataMessage()
            {
                PairRecordData = (byte[])dict[nameof(PairRecordData)].ToObject(),
            };
        }
    }
}
