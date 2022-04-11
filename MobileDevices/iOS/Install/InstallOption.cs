using Claunia.PropertyList;
using MobileDevices.iOS.PropertyLists;

namespace MobileDevices.iOS.Install
{
    public class InstallOption : IPropertyList
    {
        public string CfBundleIdentifier { get; set; }

        public byte[] ApplicationSinf { get; set; }

        public object TunesMetadata { get; set; }

        public string[] BundleIDs { get; set; }

        public NSDictionary ToDictionary()
        {
            var dict = new NSDictionary();

            dict.AddWhenNotNull("CFBundleIdentifier", CfBundleIdentifier);
            dict.AddWhenNotNull("ApplicationSINF", ApplicationSinf);
            dict.AddWhenNotNull("iTunesMetadata", TunesMetadata);
            dict.AddWhenNotNull("BundleIDs", BundleIDs);
            return dict;
        }
    }
}