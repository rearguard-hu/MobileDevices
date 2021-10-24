using Claunia.PropertyList;
using MobileDevices.iOS.PropertyLists;

namespace MobileDevices.iOS.Lockdown
{
    /// <summary>
    /// Represents an individual message which is exchanged with lockdown.
    /// </summary>
    public class LockdownMessage : IPropertyList
    {
        /// <summary>
        /// Gets or sets a label which defines the client program.
        /// </summary>
        public string Label
        { get; set; } = "MobileDevices";//ThisAssembly.AssemblyName;

        /// <summary>
        /// Gets or sets the protocol version being used.
        /// </summary>
        public string ProtocolVersion
        { get; set; } = "2";

        /// <summary>
        /// Gets or sets the name of the request.
        /// </summary>
        public string Request
        { get; set; }

        /// <inheritdoc/>
        public virtual NSDictionary ToDictionary()
        {
            NSDictionary dictionary = new NSDictionary();
            dictionary.Add(nameof(this.Label), this.Label);
            dictionary.Add(nameof(this.ProtocolVersion), this.ProtocolVersion);
            dictionary.Add(nameof(this.Request), this.Request);
            return dictionary;
        }
    }
}
