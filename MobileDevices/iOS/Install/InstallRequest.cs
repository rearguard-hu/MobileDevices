using System.Collections.Generic;
using System.Linq;
using System.Text;
using Claunia.PropertyList;
using MobileDevices.iOS.DiagnosticsRelay;
using MobileDevices.iOS.PropertyLists;

namespace MobileDevices.iOS.Install
{
    /// <summary>
    /// Represents a request sent to the <see cref="InstallClient"/> service.
    /// </summary>
    public class InstallRequest : IPropertyList
    {
        public string Command { get; set; }

        public NSObject ClientOptions { get; set; }

        public string PackagePath { get; set; }

        public string ApplicationIdentifier { get; set; }

        public NSObject Capabilities { get; set; }

        public NSArray BundleIDs { get; set; }

        public NSDictionary ToDictionary()
        {
            var dict = new NSDictionary { { nameof(Command), Command } };

            dict.AddWhenNotNull(nameof(ClientOptions), ClientOptions);
            dict.AddWhenNotNull(nameof(PackagePath), PackagePath);
            dict.AddWhenNotNull(nameof(ApplicationIdentifier), ApplicationIdentifier);
            dict.AddWhenNotNull(nameof(Capabilities), Capabilities);
            return dict;
        }
    }
}
