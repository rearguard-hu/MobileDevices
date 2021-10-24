namespace MobileDevices.iOS.DiagnosticsRelay
{
    /// <summary>
    /// Represents the status code used by the diagnostics relay service.
    /// </summary>
    public enum DiagnosticsRelayStatus
    {
        /// <summary>
        /// The operation completed successfully.
        /// </summary>
        Success = 1,

        /// <summary>
        /// A failure occurred when processing the request.
        /// </summary>
        Failure = 2,

        /// <summary>
        /// An unknown requests has been received.
        /// </summary>
        UnknownRequest = 3,
    }
}
