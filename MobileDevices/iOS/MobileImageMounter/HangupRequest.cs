using Claunia.PropertyList;
using MobileDevices.iOS.PropertyLists;

namespace MobileDevices.iOS.MobileImageMounter
{
    /// <summary>
    /// A request to the remote end to terminate the connection.
    /// </summary>
    public class HangupRequest : IPropertyList
    {
        /// <summary>
        /// Gets or sets a value indicating the command type.
        /// </summary>
        public string Command { get; set; } = "Hangup";

        /// <inheritdoc/>
        public NSDictionary ToDictionary()
        {
            var dict = new NSDictionary();
            dict.Add(nameof(this.Command), this.Command);
            return dict;
        }
    }
}
