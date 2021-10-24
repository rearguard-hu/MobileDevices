namespace MobileDevices.iOS.Lockdown
{
    /// <summary>
    /// Captures the result of a pairing operation.
    /// </summary>
    public class PairingResult
    {
        /// <summary>
        /// Gets or sets the status of the operation, indicating whether the operation completed succesfully
        /// or with an error message.
        /// </summary>
        public PairingStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the escrow bag.
        /// </summary>
        /// <remarks>
        /// When a pair request completed successfully, this value will contain the escrow bag, which can be used
        /// to access services which are usually only available when the device is unlocked.
        /// </remarks>
        public byte[] EscrowBag { get; set; }
    }
}
