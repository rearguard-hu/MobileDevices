namespace MobileDevices.iOS.MobileImageMounter
{
    /// <summary>
    /// Represents errors returned by the mobile image mounter service running on iOS devices.
    /// </summary>
    public enum MobileImageMounterError
    {
        /// <summary>
        /// The service did not return an error.
        /// </summary>
        None = 0,

        /// <summary>
        /// The client did not specify the command to execute.
        /// </summary>
        MissingCommand,

        /// <summary>
        /// The client requested an unknown command.
        /// </summary>
        UnknownCommand,

        /// <summary>
        /// The image type was missing.
        /// </summary>
        MissingImageType,

        /// <summary>
        /// The image could not be mounted.
        /// </summary>
        ImageMountFailed,
    }
}
