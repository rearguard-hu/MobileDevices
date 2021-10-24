using System.Collections.ObjectModel;

namespace MobileDevices.iOS.DiagnosticsRelay
{
    /// <summary>
    /// Represents a response sent by the Diagnostics Relay service.
    /// </summary>
    public partial class DiagnosticsRelayResponse
    {
        /// <summary>
        /// Gets or sets the status as reported by the device.
        /// </summary>
        public DiagnosticsRelayStatus Status { get; set; }

        /// <summary>
        /// Gets or sets a dictionary which contains detailed diagnostics information.
        /// </summary>
        public ReadOnlyDictionary<string, object> Diagnostics { get; set; }
    }
}
