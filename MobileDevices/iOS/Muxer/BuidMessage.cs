using System;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Represents a mesage which contains a list of all devices which are currently connected to the host.
    /// </summary>
    public partial class BuidMessage : MuxerMessage
    {
        /// <summary>
        /// Gets or sets the Bipartite Unique Identifier which uniquely identifies the usbmuxd instance.
        /// </summary>
        /// <remarks>
        /// A BUID is similar to a <see cref="Guid"/> and can usually be managed as such.
        /// </remarks>
        public string BUID
        { get; set; }
    }
}
