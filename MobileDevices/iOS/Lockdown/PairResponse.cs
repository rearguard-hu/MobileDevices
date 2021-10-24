namespace MobileDevices.iOS.Lockdown
{
    /// <summary>
    /// Represents the device's response to a <see cref="PairRequest"/>.
    /// </summary>
    public partial class PairResponse : LockdownResponse
    {
        /// <summary>
        /// Gets or sets the escrow bag, if any.
        /// </summary>
        public byte[] EscrowBag { get; set; }
    }
}
