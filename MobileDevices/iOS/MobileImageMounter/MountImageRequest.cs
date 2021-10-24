using Claunia.PropertyList;
using MobileDevices.iOS.PropertyLists;

namespace MobileDevices.iOS.MobileImageMounter
{
    /// <summary>
    /// Represents the mount image request.
    /// </summary>
    public class MountImageRequest : IPropertyList
    {
        /// <summary>
        /// Gets or sets a value indicating the command type.
        /// </summary>
        public string Command { get; set; } = "MountImage";

        /// <summary>
        /// Gets or sets the path to the image (on the device).
        /// </summary>
        public string ImagePath
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the image signature.
        /// </summary>
        public byte[] ImageSignature
        { get; set; }

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
            dict.Add(nameof(this.ImagePath), this.ImagePath);
            dict.Add(nameof(this.ImageSignature), this.ImageSignature);
            dict.Add(nameof(this.ImageType), this.ImageType);
            return dict;
        }
    }
}
