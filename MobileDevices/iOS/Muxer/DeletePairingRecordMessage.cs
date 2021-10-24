using Claunia.PropertyList;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Represents a request to delete a pairing record.
    /// </summary>
    public class DeletePairingRecordMessage : RequestMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeletePairingRecordMessage"/> class.
        /// </summary>
        public DeletePairingRecordMessage()
        {
            this.MessageType = MuxerMessageType.DeletePairRecord;
        }

        /// <summary>
        /// Gets or sets the UDID of the device for which to delete the pairing record.
        /// </summary>
        public string PairRecordID { get; set; }

        /// <inheritdoc/>
        public override NSDictionary ToPropertyList()
        {
            var dict = base.ToPropertyList();
            dict.Add(nameof(this.PairRecordID), this.PairRecordID);
            return dict;
        }
    }
}
