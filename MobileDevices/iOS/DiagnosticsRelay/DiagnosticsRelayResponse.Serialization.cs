using Claunia.PropertyList;
using System;

namespace MobileDevices.iOS.DiagnosticsRelay
{
    /// <content>
    /// Serialization-related methods for <see cref="DiagnosticsRelayResponse"/>.
    /// </content>
    public partial class DiagnosticsRelayResponse
    {
        /// <summary>
        /// Reads a <see cref="DiagnosticsRelayResponse"/> from a <see cref="NSDictionary"/>.
        /// </summary>
        /// <param name="dict">
        /// The message data.
        /// </param>
        /// <returns>
        /// A <see cref="DiagnosticsRelayResponse"/> object.
        /// </returns>
        public static DiagnosticsRelayResponse Read(NSDictionary dict)
        {
            if (dict == null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            var value = new DiagnosticsRelayResponse();
            value.Status = Enum.Parse<DiagnosticsRelayStatus>(dict.GetString(nameof(Status)));
            value.Diagnostics = dict.GetDictionary(nameof(Diagnostics));
            return value;
        }
    }
}
