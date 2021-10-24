namespace MobileDevices.iOS.SpingBoardServices
{
    /// <summary>
    /// Represents the orientation of the device.
    /// </summary>
    public enum SpringBoardServicesInterfaceOrientation
    {
        /// <summary>
        /// The orientation of the device is unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The orientation of the device is in portrait mode.
        /// </summary>
        Portrait = 1,

        /// <summary>
        /// The orientation of the device is upside down.
        /// </summary>
        PortraitUpsideDown = 2,

        /// <summary>
        /// The orientation of the device is in landscape mode to the right.
        /// </summary>
        LandscapeRight = 3,

        /// <summary>
        /// The orientation of the device is in landscape mode to the left.
        /// </summary>
        LandscapeLeft = 4,
    }
}
