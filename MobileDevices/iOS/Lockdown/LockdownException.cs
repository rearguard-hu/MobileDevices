using System;

namespace MobileDevices.iOS.Lockdown
{
    /// <summary>
    /// The exception which is thrown when an error occurs interacting with the Lockdown daemon.
    /// </summary>
    public class LockdownException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LockdownException"/> class.
        /// </summary>
        public LockdownException()
            : base("An unknown lockdown error occurred")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockdownException"/> class, using a
        /// <see cref="LockdownError"/> code.
        /// </summary>
        /// <param name="error">
        /// A <see cref="LockdownError"/> code which describes the error.
        /// </param>
        public LockdownException(LockdownError error)
            : this($"An unexpected lockdown error occurred: {error}")
        {
            this.HResult = (int)error;
            this.Error = error;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockdownException"/> class with a specified error
        /// message.
        /// </summary>
        /// <param name="message">
        /// The message that describes the error.
        /// </param>
        public LockdownException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Gets the <see cref="LockdownError"/> returned by the lockdown daemon.
        /// </summary>
        public LockdownError Error { get; private set; }
    }
}
