namespace MobileDevices.iOS.MobileImageMounter
{
    /// <summary>
    /// Represents a status code returned by the mobile image mounter service running on a device.
    /// </summary>
    public enum MobileImageMounterStatus
    {
        /// <summary>
        /// The operation has completed successfully.
        /// </summary>
        Complete,

        /// <summary>
        /// The service is ready to receive bytes (usually the contents of the developer disk).
        /// </summary>
        ReceiveBytesAck,
    }
}
