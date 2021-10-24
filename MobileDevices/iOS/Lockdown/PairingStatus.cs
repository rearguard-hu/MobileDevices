using System.Threading;

namespace MobileDevices.iOS.Lockdown
{
    /// <summary>
    /// The result of a <see cref="LockdownClient.PairAsync(PairingRecord, CancellationToken)"/>
    /// operation.
    /// </summary>
    public enum PairingStatus
    {
        /// <summary>
        /// The operation completed successfully. The host and device have paired successfully.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The user denied the pairing request.
        /// </summary>
        UserDeniedPairing = LockdownError.UserDeniedPairing,

        /// <summary>
        /// A pairing dialog is being displayed on the device, and the lockdown service
        /// is waiting for the server to accept or deny the pairing request.
        /// </summary>
        PairingDialogResponsePending = LockdownError.PairingDialogResponsePending,

        /// <summary>
        /// An invalid pair record was presented to the device.
        /// </summary>
        InvalidPairRecord = LockdownError.InvalidPairRecord,
    }
}
