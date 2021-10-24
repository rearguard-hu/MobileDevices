using Claunia.PropertyList;

namespace MobileDevices.iOS.NotificationProxy
{
    /// <summary>
    /// Represents a message which is sent by or received from the notification proxy running on the device.
    /// </summary>
    public partial class NotificationProxyMessage
    {
        /// <summary>
        /// Gets or sets the name of the command.
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// Gets or sets the name of the notification message.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Serializes this <see cref="NotificationProxyMessage"/> to a <see cref="NSDictionary"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="NSDictionary"/> which represents this <see cref="NotificationProxyMessage"/>.
        /// </returns>
        public NSDictionary ToPropertyList()
        {
            NSDictionary dict = new NSDictionary();
            dict.Add(nameof(this.Command), this.Command);
            dict.Add(nameof(this.Name), this.Name);
            return dict;
        }
    }
}
