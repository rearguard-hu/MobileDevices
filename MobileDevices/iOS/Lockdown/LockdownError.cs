namespace MobileDevices.iOS.Lockdown
{
    /// <summary>
    /// Represents an error code returned by the lockdown daemon.
    /// </summary>
    public enum LockdownError
    {
        /// <summary>
        /// No error occurred.
        /// </summary>
        None = 0,

        /// <summary>
        /// The operation requires a session to be active, but no session is currently active.
        /// </summary>
        SessionInactive,

        /// <summary>
        /// An attempt was made to request a value of a property, for which getting the value is prohibited.
        /// </summary>
        GetProhibited,

        /// <summary>
        /// An attempt was made to request set a value of a property, for which setting the value is prohibited.
        /// </summary>
        SetProhibited,

        /// <summary>
        ///  An attempt was made to request a value of a property, for which getting the value is Missing.
        /// </summary>
        MissingValue,

        /// <summary>
        /// The host ID is invalid.
        /// </summary>
        InvalidHostID,

        /// <summary>
        /// The user denied the pairing request.
        /// </summary>
        UserDeniedPairing,

        /// <summary>
        /// A pairing dialog is being displayed on the device, and the lockdown service
        /// is waiting for the server to accept or deny the pairing request.
        /// </summary>
        PairingDialogResponsePending,

        /// <summary>
        /// An invalid pair record was presented to the device.
        /// </summary>
        InvalidPairRecord,

        /// <summary>
        /// An unknown exception
        /// </summary>
        Unknown
    }
}
