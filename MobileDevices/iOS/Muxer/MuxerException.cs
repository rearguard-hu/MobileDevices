using System;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Represents an error returned by the muxer.
    /// </summary>
    public class MuxerException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MuxerException"/> class.
        /// </summary>
        public MuxerException()
            : this("An unexpected usbmuxd exception occurred.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MuxerException"/> class with an error message.
        /// </summary>
        /// <param name="message">
        /// A message which describes the error.
        /// </param>
        public MuxerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MuxerException"/> class with an error message
        /// and error number.
        /// </summary>
        /// <param name="message">
        /// A message which describes the error.
        /// </param>
        /// <param name="error">
        /// An error code which represents the error.
        /// </param>
        public MuxerException(string message, MuxerError error)
            : base(message)
        {
            this.HResult = (int)error;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MuxerException"/> class with an error message
        /// and an inner exception.
        /// </summary>
        /// <param name="message">
        /// A message which describes the error.
        /// </param>
        /// <param name="innerException">
        /// The inner exception which caused this exception.
        /// </param>
        public MuxerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
