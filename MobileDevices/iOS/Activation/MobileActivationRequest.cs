using Claunia.PropertyList;
using MobileDevices.iOS.PropertyLists;

namespace MobileDevices.iOS.Activation
{
    public class MobileActivationRequest : IPropertyList
    {
        public string Command { get; set; }

        public object Value { get; set; }

        public object ActivationResponseHeaders { get; set; }

        public NSDictionary ToDictionary()
        {
            var dict = new NSDictionary { { nameof(this.Command), this.Command } };

            dict.AddWhenNotNull(nameof(this.Value), this.Value);
            dict.AddWhenNotNull(nameof(this.ActivationResponseHeaders), this.ActivationResponseHeaders);

            return dict;
        }
    }
}