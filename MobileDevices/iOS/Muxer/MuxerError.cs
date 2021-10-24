namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Represents the error messages returned by the muxer.
    /// </summary>
    public enum MuxerError
    {
        /// <summary>
        /// The operation completed successfully.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The muxer received a bad command.
        /// </summary>
        BadCommand = 1,

        /// <summary>
        /// An attempt was made to connect to a bad device.
        /// </summary>
        BadDevice = 2,

        /// <summary>
        /// The connection was refused by the device.
        /// </summary>
        ConnectionRefused = 3,

        /// <summary>
        /// A bad muxer version was used.
        /// </summary>
        BadVersion = 6,

        /// <summary>
        /// A connection occurred when interacting with the muxer.
        /// </summary>
        MuxerError = -1,
    }
}
