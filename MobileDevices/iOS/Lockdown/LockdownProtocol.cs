using MobileDevices.iOS.PropertyLists;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.Lockdown
{
    /// <summary>
    /// The <see cref="LockdownProtocol"/> can be used to send and receive lockdown messages.
    /// </summary>
    public class LockdownProtocol : PropertyListProtocol
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LockdownProtocol"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents the connection to the muxer.
        /// </param>
        /// <param name="ownsStream">
        /// A value indicating whether this <see cref="LockdownProtocol"/> instance owns the <paramref name="stream"/> or not.
        /// </param>
        /// <param name="logger">
        /// A <see cref="ILogger"/> which can be used when logging.
        /// </param>
        public LockdownProtocol(Stream stream, bool ownsStream, ILogger logger)
            : base(stream, ownsStream, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockdownProtocol"/> class.
        /// Intended for unit testing purposes only.
        /// </summary>
        protected LockdownProtocol()
        {
        }

        /// <summary>
        /// Asynchronously sends a message to the remote lockdown client.
        /// </summary>
        /// <param name="message">
        /// The message to send.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public virtual Task WriteMessageAsync(LockdownMessage message, CancellationToken cancellationToken)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            // Serialize the underlying message so we can calculate the packet size
            var dictionary = message.ToDictionary();
            return this.WriteMessageAsync(dictionary, cancellationToken);
        }
    }
}
