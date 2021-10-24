using Claunia.PropertyList;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Represents a request to connect to a port on the mobile device.
    /// </summary>
    public class ConnectMessage : RequestMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectMessage"/> class.
        /// </summary>
        public ConnectMessage()
        {
            this.MessageType = MuxerMessageType.Connect;
        }

        /// <summary>
        /// Gets or sets the ID of the device to which to connect.
        /// </summary>
        public int DeviceID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the port to which to connect.
        /// </summary>
        public int PortNumber
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public override NSDictionary ToPropertyList()
        {
            NSDictionary dictionary = new NSDictionary();
            dictionary.Add(nameof(this.BundleID), new NSString(this.BundleID));
            dictionary.Add(nameof(this.ClientVersionString), new NSString(this.ClientVersionString));
            dictionary.Add(nameof(this.DeviceID), new NSNumber(this.DeviceID));
            dictionary.Add(nameof(this.MessageType), new NSString(this.MessageType.ToString()));
            dictionary.Add(nameof(this.ProgName), new NSString(this.ProgName));
            dictionary.Add(nameof(this.PortNumber), new NSNumber(this.PortNumber));
            return dictionary;
        }
    }
}
