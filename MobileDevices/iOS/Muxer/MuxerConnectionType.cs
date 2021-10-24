namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Defines how a device is connected.
    /// </summary>
    public enum MuxerConnectionType
    {
        /// <summary>
        /// The device is connected over USB.
        /// </summary>
        USB,

        /// <summary>
        /// The device is connected over WiFi.
        /// </summary>
        Network,
    }
}
