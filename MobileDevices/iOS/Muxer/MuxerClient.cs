using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Provides a client interface for the Apple Mobile Device Multiplexor (usbmuxd) running on the host.
    /// </summary>
    public partial class MuxerClient
    {
        private readonly ILogger<MuxerClient> logger;
        private readonly ILoggerFactory loggerFactory;
        private readonly MuxerSocketLocator socketLocator;

        /// <summary>
        /// Initializes a new instance of the <see cref="MuxerClient"/> class.
        /// </summary>
        /// <param name="socketLocator">
        /// A <see cref="MuxerSocketLocator"/> which is used to locate the usbmuxd socket.
        /// </param>
        /// <param name="logger">
        /// The logger to use when logging.
        /// </param>
        /// <param name="loggerFactory">
        /// A <see cref="ILoggerFactory"/> which can be used to initialise new loggers.
        /// </param>
        public MuxerClient(MuxerSocketLocator socketLocator, ILogger<MuxerClient> logger, ILoggerFactory loggerFactory)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.socketLocator = socketLocator ?? throw new ArgumentNullException(nameof(socketLocator));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MuxerClient"/> class. Intended for mocking purposes only.
        /// </summary>
        protected MuxerClient()
        {
            this.logger = NullLogger<MuxerClient>.Instance;
        }

        /// <summary>
        /// Creates a new connection to <c>usbmuxd</c>.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="MuxerProtocol"/> which represents the connection to <c>usbmuxd</c>, or <see langword="null"/> if
        /// usbmuxd is not running.
        /// </returns>
        public virtual async Task<MuxerProtocol> TryConnectToMuxerAsync(CancellationToken cancellationToken)
        {
            var stream = await this.socketLocator.ConnectToMuxerAsync(cancellationToken).ConfigureAwait(false);

            if (stream == null)
            {
                return null;
            }

            return new MuxerProtocol(
                stream,
                ownsStream: true,
                this.loggerFactory.CreateLogger<MuxerProtocol>());
        }
    }
}
