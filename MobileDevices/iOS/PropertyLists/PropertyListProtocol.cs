using Claunia.PropertyList;
using Microsoft;
using Microsoft.Extensions.Logging;
using Nerdbank.Streams;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServiceProtocol = MobileDevices.iOS.Services.ServiceProtocol;

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

        public virtual async Task<NSDictionary> RequestAsync(NSDictionary message, CancellationToken cancellationToken)
        {

            await this.WriteMessageAsync(message, cancellationToken);

            return await this.ReadMessageAsync(cancellationToken);
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

            return this.WriteMessageAsync(message.ToDictionary(), cancellationToken);
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
        public virtual async Task WriteMessageAsync(NSDictionary message, CancellationToken cancellationToken)
        {
            Verify.NotDisposed(this);

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            // Serialize the underlying message so we can calculate the packet size
            var xml = message.ToXmlPropertyList();

            if (this.Logger.IsEnabled(LogLevel.Trace))
            {
                this.Logger.LogTrace("Sending data:\r\n{data}", xml);
            }

            int messageLength = Encoding.UTF8.GetByteCount(xml);

            var packetLength = 4 + messageLength;

            using (var packet = this.MemoryPool.Rent(packetLength))
            {
                // Construct the entire packet:
                // [length] (4 bytes)
                // [UTF-8 XML-encoded property list message] (N bytes)
                BinaryPrimitives.WriteInt32BigEndian(packet.Memory.Span[0..4], messageLength);

                Encoding.UTF8.GetBytes(xml, packet.Memory.Span[4..(messageLength + 4)]);

                // Send the packet
                await this.Stream.WriteAsync(packet.Memory[0..packetLength], cancellationToken).ConfigureAwait(false);
                await this.Stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
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

            int length;

            using (var lengthBuffer = this.MemoryPool.Rent(4))
            {
                if (await this.Stream.ReadBlockAsync(lengthBuffer.Memory[0..4], cancellationToken).ConfigureAwait(false) != 4)
                {
                    return null;
                }

                length = BinaryPrimitives.ReadInt32BigEndian(lengthBuffer.Memory[0..4].Span);
            }

            using (var messageBuffer = this.MemoryPool.Rent(length))
            {
                if (await this.Stream.ReadBlockAsync(messageBuffer.Memory[0..length], cancellationToken).ConfigureAwait(false) != length)
                {
                    return null;
                }

                var dict = (NSDictionary)PropertyListParser.Parse(messageBuffer.Memory[0..length].Span);

                if (this.Logger.IsEnabled(LogLevel.Trace))
                {
                    this.Logger.LogTrace("Recieving data:\r\n{data}", dict.ToXmlPropertyList());
                }

                return dict;
            }
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


            var dict = await this.ReadMessageAsync(cancellationToken);

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
