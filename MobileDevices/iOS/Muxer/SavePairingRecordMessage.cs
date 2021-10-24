using Claunia.PropertyList;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Represents a request to save a pairing record.
    /// </summary>
    public class SavePairingRecordMessage : RequestMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SavePairingRecordMessage"/> class.
        /// </summary>
        public SavePairingRecordMessage()
        {
            this.MessageType = MuxerMessageType.SavePairRecord;
        }

        /// <summary>
        /// Gets or sets the UDID of the device for which to save the pairing record.
        /// </summary>
        public string PairRecordID { get; set; }

        /// <summary>
        /// Gets or sets a byte array which represents the pairing record.
        /// </summary>
        public byte[] PairRecordData { get; set; }

        /// <inheritdoc/>
        public override NSDictionary ToPropertyList()
        {
            var dict = base.ToPropertyList();
            dict.Add(nameof(this.PairRecordID), this.PairRecordID);
            dict.Add(nameof(this.PairRecordData), this.PairRecordData);
            return dict;
        }
    }
}
