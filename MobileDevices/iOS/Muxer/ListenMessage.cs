using Claunia.PropertyList;
using System.Diagnostics.CodeAnalysis;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Represents a message requesting <c>usbmuxd</c> to send notifications about device connect/disconnect/trust
    /// events.
    /// </summary>
    public class ListenMessage : RequestMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListenMessage"/> class.
        /// </summary>
        public ListenMessage()
        {
            this.MessageType = MuxerMessageType.Listen;
        }

        /// <summary>
        /// Gets or sets the connection type being used.
        /// </summary>
        public int ConnType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the version of the <c>usbmux</c> protocol being used.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element must begin with upper-case letter", Justification = "Standard iOS naming")]
        public int kLibUSBMuxVersion
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
            dictionary.Add(nameof(this.ConnType), new NSNumber(this.ConnType));
            dictionary.Add(nameof(this.MessageType), new NSString(this.MessageType.ToString()));
            dictionary.Add(nameof(this.ProgName), new NSString(this.ProgName));
            dictionary.Add(nameof(this.kLibUSBMuxVersion), new NSNumber(this.kLibUSBMuxVersion));
            return dictionary;
        }
    }
}
