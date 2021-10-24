using Claunia.PropertyList;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Represents a single method invocation request message. This class is usually subclassed
    /// for specific requests.
    /// </summary>
    public class RequestMessage : MuxerMessage
    {
        /// <summary>
        /// Gets or sets an ID which identifies the current program.
        /// </summary>
        public string BundleID
        { get; set; } = "MobileDevices";

        /// <summary>
        /// Gets or sets the version of the <c>usbmuxd</c> which is being used.
        /// </summary>
        public string ClientVersionString
        { get; set; } = "0.3.0";

        /// <summary>
        /// Gets or sets the name of the current program.
        /// </summary>
        public string ProgName
        { get; set; } = "MobileDevices";

        /// <inheritdoc/>
        public override NSDictionary ToPropertyList()
        {
            NSDictionary dictionary = new NSDictionary();
            dictionary.Add(nameof(this.BundleID), new NSString(this.BundleID));
            dictionary.Add(nameof(this.ClientVersionString), new NSString(this.ClientVersionString));
            dictionary.Add(nameof(this.MessageType), new NSString(this.MessageType.ToString()));
            dictionary.Add(nameof(this.ProgName), new NSString(this.ProgName));
            return dictionary;
        }
    }
}
