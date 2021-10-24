using Claunia.PropertyList;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Nerdbank.Streams;
using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// The <see cref="MuxerProtocol"/> allows interacting with the Apple USB Multiplexor Daemon (usbmuxd) using a <see cref="Stream"/>.
    /// </summary>
    public class MuxerProtocol : IAsyncDisposable
    {
        private const int ProtocolVersion = 1;

        private readonly Stream stream;
        private readonly ILogger<MuxerProtocol> logger;
        private readonly bool ownsStream;
        private readonly MemoryPool<byte> memoryPool = MemoryPool<byte>.Shared;

        private uint tag = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="MuxerProtocol"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents the connection to the muxer.
        /// </param>
        /// <param name="ownsStream">
        /// A value indicating whether this <see cref="MuxerProtocol"/> instance owns the <paramref name="stream"/> or not.
        /// </param>
        /// <param name="logger">
        /// A logger which is used to log messages.
        /// </param>
        public MuxerProtocol(Stream stream, bool ownsStream, ILogger<MuxerProtocol> logger)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.ownsStream = ownsStream;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MuxerProtocol"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is intended for unit testing purposes only.
        /// </remarks>
        protected MuxerProtocol()
            : this(Stream.Null, false, NullLogger<MuxerProtocol>.Instance)
        {
        }

        /// <summary>
        /// Gets the <see cref="Stream"/> around which this <see cref="MuxerProtocol"/> wraps.
        /// </summary>
        public virtual Stream Stream => this.stream;

        /// <summary>
        /// Asynchronously sends a message to the remote muxer.
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
        public virtual async Task WriteMessageAsync(MuxerMessage message, CancellationToken cancellationToken)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            // Serialize the underlying message so we can calculate the packet size
            var dictionary = message.ToPropertyList();
            var xml = dictionary.ToXmlPropertyList();
            int messageLength = Encoding.UTF8.GetByteCount(xml);

            var packetLength = MuxerHeader.BinarySize + messageLength;

            using (var packet = this.memoryPool.Rent(packetLength))
            {
                // Construct the entire packet:
                // [muxer header] (16 bytes)
                // [UTF-8 XML-encoded property list message] (N bytes)
                var header = new MuxerHeader()
                {
                    Length = (uint)(MuxerHeader.BinarySize + messageLength),
                    Version = ProtocolVersion,
                    Message = MuxerMessageType.Plist,
                    Tag = this.tag++,
                };

                header.Write(packet.Memory.Slice(0, MuxerHeader.BinarySize).Span);

                Encoding.UTF8.GetBytes(xml, packet.Memory.Slice(MuxerHeader.BinarySize, messageLength).Span);

                // Send the packet
                await this.stream.WriteAsync(packet.Memory.Slice(0, packetLength), cancellationToken).ConfigureAwait(false);
                await this.stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Asynchronously receives a message from the remote muxer.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="DeviceListMessage"/> object which represents the property list value received
        /// from the remote muxer.
        /// </returns>
        public virtual async Task<MuxerMessage> ReadMessageAsync(CancellationToken cancellationToken)
        {
            int read;
            MuxerHeader header;

            using (var headerBuffer = this.memoryPool.Rent(MuxerHeader.BinarySize))
            {
                if ((read = await this.stream.ReadBlockAsync(headerBuffer.Memory.Slice(0, MuxerHeader.BinarySize), cancellationToken).ConfigureAwait(false)) != MuxerHeader.BinarySize)
                {
                    this.logger.LogInformation("Could only read {read}/{total} bytes of the muxer header; exiting.", read, MuxerHeader.BinarySize);
                    return null;
                }

                header = MuxerHeader.Read(headerBuffer.Memory.Slice(0, MuxerHeader.BinarySize).Span);
            }

            if (header.Message != MuxerMessageType.Plist)
            {
                throw new NotSupportedException($"Only Plist message types are supported; but a {header.Message} message was received");
            }

            int messageLength = (int)header.Length - MuxerHeader.BinarySize;
            using (var messageBuffer = this.memoryPool.Rent(messageLength))
            {
                if ((read = await this.stream.ReadBlockAsync(messageBuffer.Memory.Slice(0, messageLength), cancellationToken).ConfigureAwait(false)) != messageLength)
                {
                    this.logger.LogInformation("Could only read {read}/{total} bytes of the muxer header; exiting.", read, messageLength);
                    return null;
                }

                var propertyListData = messageBuffer.Memory.Slice(0, messageLength).ToArray();
                var propertyList = (NSDictionary)XmlPropertyListParser.Parse(propertyListData);

                return MuxerMessage.ReadAny(propertyList);
            }
        }

        /// <inheritdoc/>
        public virtual ValueTask DisposeAsync()
        {
            if (this.ownsStream)
            {
                return this.stream.DisposeAsync();
            }
            else
            {
                return ValueTask.CompletedTask;
            }
        }
    }
}
