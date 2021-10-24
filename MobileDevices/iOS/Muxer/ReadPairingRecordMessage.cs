using Claunia.PropertyList;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Represents a request to read the pair record.
    /// </summary>
    public class ReadPairingRecordMessage : RequestMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadPairingRecordMessage"/> class.
        /// </summary>
        public ReadPairingRecordMessage()
        {
            this.MessageType = MuxerMessageType.ReadPairRecord;
        }

        /// <summary>
        /// Gets or sets the UDID of the device for which to read the pair record.
        /// </summary>
        public string PairRecordID
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public override NSDictionary ToPropertyList()
        {
            var dict = base.ToPropertyList();
            dict.Add(nameof(this.PairRecordID), this.PairRecordID);
            return dict;
        }
    }
}
