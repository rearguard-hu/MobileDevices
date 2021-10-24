using System;

namespace MobileDevices.iOS.MobileImageMounter
{
    /// <summary>
    /// Represents errors when mounting iOS images.
    /// </summary>
    public class MobileImageMounterException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MobileImageMounterException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message that describes the error.
        /// </param>
        /// <param name="status">
        /// The status of the <see cref="MobileImageMounterResponse"/>.
        /// </param>
        /// <param name="error">
        /// The error of the <see cref="MobileImageMounterResponse"/>.
        /// </param>
        /// <param name="detailedError">
        /// The detailed error of the <see cref="MobileImageMounterResponse"/>.
        /// </param>
        public MobileImageMounterException(string message, MobileImageMounterStatus? status, MobileImageMounterError? error, string detailedError)
            : base(message)
        {
            this.Status = status;
            this.DetailedError = detailedError;
            this.Error = error;
        }

        /// <summary>
        /// Gets or sets the status of the <see cref="MobileImageMounterResponse"/>.
        /// </summary>
        public MobileImageMounterStatus? Status
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the error of the <see cref="MobileImageMounterResponse"/>.
        /// </summary>
        public MobileImageMounterError? Error
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the detailed error of the <see cref="MobileImageMounterResponse"/>.
        /// </summary>
        public string DetailedError
        {
            get;
            set;
        }
    }
}
