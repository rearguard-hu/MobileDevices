using Claunia.PropertyList;
using MobileDevices.iOS.PropertyLists;

namespace MobileDevices.iOS.DiagnosticsRelay
{
    /// <summary>
    /// Represents a request sent to the <see cref="DiagnosticsRelayClient"/> service.
    /// </summary>
    public class DiagnosticsRelayRequest : IPropertyList
    {
        /// <summary>
        /// Gets or sets the type of this request. Known requests include <c>Restart</c>, <c>Shutdown</c>,
        /// <c>Goodbye</c> or <c>IORegistry</c>.
        /// </summary>
        public string Request { get; set; }

        /// <summary>
        /// Gets or sets, when <see cref="Request"/> is <c>MobileGestalt</c>, the name of the registry
        /// entry being queried.
        /// </summary>
        public NSArray MobileGestaltKeys { get; set; }

        /// <summary>
        /// Gets or sets, when <see cref="Request"/> is <c>IORegistry</c>, the name of the registry
        /// entry being queried.
        /// </summary>
        public string EntryName { get; set; }

        /// <summary>
        /// Gets or sets, when <see cref="Request"/> is <c>IORegistry</c>, the class of the registry
        /// entry being queried.
        /// </summary>
        public string EntryClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to wait for the client to disconnect before
        /// executing the request, or not.
        /// </summary>
        public bool? WaitForDisconnect { get; set; }

        /// <inheritdoc/>
        public NSDictionary ToDictionary()
        {
            var dict = new NSDictionary { { nameof(Request), Request } };

            dict.AddWhenNotNull(nameof(EntryName), EntryName);
            dict.AddWhenNotNull(nameof(EntryClass), EntryClass);
            dict.AddWhenNotNull(nameof(WaitForDisconnect), WaitForDisconnect);
            dict.AddWhenNotNull(nameof(MobileGestaltKeys), MobileGestaltKeys);
            return dict;
        }
    }
}
