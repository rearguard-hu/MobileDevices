using MobileDevices.iOS.Muxer;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.Lockdown
{
    /// <summary>
    /// A <see cref="ClientFactory{T}"/> which can create lockdown clients.
    /// </summary>
    public class LockdownClientFactory : ClientFactory<LockdownClient>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LockdownClientFactory"/> class.
        /// </summary>
        /// <param name="muxer">
        /// The <see cref="MuxerClient"/> which represents the connection to the iOS USB Multiplexor.
        /// </param>
        /// <param name="context">
        /// The <see cref="DeviceContext"/> which contains information about the device with which
        /// we are interacting.
        /// </param>
        /// <param name="logger">
        /// A <see cref="ILogger"/> which can be used when logging.
        /// </param>
        public LockdownClientFactory(MuxerClient muxer, DeviceContext context, ILogger<LockdownClient> logger)
            : base(muxer, context, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref=" LockdownClientFactory"/> class. Intended
        /// for mocking purposes only.
        /// </summary>
        protected LockdownClientFactory()
        {
        }

        /// <inheritdoc/>
        public override async Task<LockdownClient> CreateAsync(CancellationToken cancellationToken)
        {
            var stream = await this.Muxer.ConnectAsync(this.Context.Device, LockdownClient.LockdownPort, cancellationToken).ConfigureAwait(false);

            LockdownClient client = new LockdownClient(stream, this.Logger);

            // Make sure we are really connected to lockdown
            var type = await client.QueryTypeAsync(cancellationToken).ConfigureAwait(false);

            if (type != LockdownClient.ServiceName)
            {
                throw new LockdownException($"Expected a connection to Lockdown but got a connection to '{type}' instead");
            }

            return client;
        }

        /// <inheritdoc/>
        public override Task<LockdownClient> CreateAsync(string serviceName, CancellationToken cancellationToken) => this.CreateAsync(cancellationToken);
    }
}
