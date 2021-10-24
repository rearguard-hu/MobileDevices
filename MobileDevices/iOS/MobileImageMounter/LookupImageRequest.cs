using Claunia.PropertyList;
using MobileDevices.iOS.PropertyLists;

namespace MobileDevices.iOS.MobileImageMounter
{
    /// <summary>
    /// Represents a request which is sent to the mobile image mounter service running on the device.
    /// </summary>
    public class LookupImageRequest : IPropertyList
    {
        /// <summary>
        /// Gets or sets a value indicating the command type.
        /// </summary>
        public string Command { get; set; } = "LookupImage";

        /// <summary>
        /// Gets or sets a value indicating the image type.
        /// </summary>
        public string ImageType
        {
            get; set;
        }

        /// <inheritdoc/>
        public NSDictionary ToDictionary()
        {
            var dict = new NSDictionary();
            dict.Add(nameof(this.Command), this.Command);
            dict.Add(nameof(this.ImageType), this.ImageType);
            return dict;
        }
    }
}
