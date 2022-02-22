using Claunia.PropertyList;
using Microsoft;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MobileDevices.iOS.Services;

namespace MobileDevices.iOS.PropertyLists
{
    /// <summary>
    /// The <see cref="PropertyListProtocol"/> supports reading and writing messages in property list format.
    /// </summary>
    public class PropertyListProtocol : ServiceProtocol
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyListProtocol"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents the connection to the muxer.
        /// </param>
        /// <param name="ownsStream">
        /// A value indicating whether this <see cref="PropertyListProtocol"/> instance owns the <paramref name="stream"/> or not.
        /// </param>
        /// <param name="logger">
        /// A <see cref="ILogger"/> which can be used when logging.
        /// </param>
        public PropertyListProtocol(Stream stream, bool ownsStream, ILogger logger)
            : base(stream, ownsStream, logger)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyListProtocol"/> class.
        /// Intended for unit testing purposes only.
        /// </summary>
        protected PropertyListProtocol()
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
        public virtual Task WriteMessageAsync(IPropertyList message, CancellationToken cancellationToken)
        {
            Verify.NotDisposed(this);

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return WriteMessageAsync(message.ToDictionary(), cancellationToken);
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
        public virtual Task WriteMessageAsync(NSDictionary message, CancellationToken cancellationToken)
        {
            Verify.NotDisposed(this);

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            //Serialize the underlying message
            return WriteMessageAsync(message.ToXmlPropertyList(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads a lockdown message from the stream.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.
        /// </param>
        /// <returns>
        /// A <see cref="byte"/> array containing the message data when available; otherwise,
        /// <see langword="null"/>.
        /// </returns>
        public virtual async Task<NSDictionary> ReadMessageAsync(CancellationToken cancellationToken)
        {
            Verify.NotDisposed(this);

            using var owner = await ReadPipeDataAsync(cancellationToken);
            var dict = (NSDictionary)PropertyListParser.Parse(owner.Memory.Span[..owner.ValidLength]);

            if (Logger.IsEnabled(LogLevel.Trace))
            {
                Logger.LogTrace("Recieving data:\r\n{data}", dict.ToXmlPropertyList());
            }

            return dict;

        }

        /// <summary>
        /// Asynchronously reads a message.
        /// </summary>
        /// <typeparam name="T">
        /// The type of message to read.
        /// </typeparam>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and returns the message
        /// when completed.
        /// </returns>
        public virtual async Task<T> ReadMessageAsync<T>(CancellationToken cancellationToken)
            where T : class, IPropertyListDeserializable, new()
        {
            Verify.NotDisposed(this);


            var dict = await ReadMessageAsync(cancellationToken);

            if (dict == null)
            {
                return null;
            }

            var value = new T();
            value.FromDictionary(dict);
            return value;

        }

    }
}
