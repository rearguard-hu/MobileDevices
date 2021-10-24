namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// The message that is sent by <c>usbmuxd</c> when a new device is attached (connected) to the host.
    /// </summary>
    public partial class DeviceAttachedMessage : DeviceMessage
    {
        /// <summary>
        /// Gets or sets the properties of the device which has been attached to the host.
        /// </summary>
        public DeviceProperties Properties
        {
            get;
            set;
        }
    }
}
