namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Represents a device-specific message.
    /// </summary>
    public abstract class DeviceMessage : MuxerMessage
    {
        /// <summary>
        /// Gets or sets the ID of the device to which this message is related. This ID is not the UDID,
        /// but an ID which is internal to <c>usbmuxd</c>. It is incremented every time a device is connected,
        /// so if the same device is connected, disconnected and re-connected, it will have a different device ID.
        /// </summary>
        public int DeviceID
        {
            get;
            set;
        }
    }
}
