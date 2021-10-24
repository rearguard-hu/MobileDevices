using System;

namespace MobileDevices.iOS.DiagnosticsRelay
{
    /// <summary>
    /// An <see cref="Exception"/> which is throw when an error occurs when interacting with the diagnostics relay service
    /// on an iOS device.
    /// </summary>
    public class DiagnosticsRelayException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticsRelayException"/> class.
        /// </summary>
        /// <param name="status">
        /// An error code which describes the error.
        /// </param>
        public DiagnosticsRelayException(DiagnosticsRelayStatus status)
            : base($"An error occurred while sending a diagnostics relay request: {status}.")
        {
            this.Status = status;
        }

        /// <summary>
        /// Gets an error code which describes the error.
        /// </summary>
        public DiagnosticsRelayStatus Status
        {
            get;
            private set;
        }
    }
}
