using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MobileDevices.iOS.Lockdown;
using MobileDevices.iOS.Muxer;

namespace MobileDevices.iOS.AfcServices
{
    public class AfcClientFactory : ServiceClientFactory<AfcClient>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="AfcClientFactory"/> class.
        /// </summary>
        /// <param name="muxer">
        /// The <see cref="MuxerClient"/> which represents the connection to the iOS USB Multiplexor.
        /// </param>
        /// <param name="context">
        /// The <see cref="DeviceContext"/> which contains information about the device with which
        /// we are interacting.
        /// </param>
        /// <param name="afcProtocolFactory">
        /// A <see cref="AfcProtocolFactory"/> which can be used to create new instances of the <see cref="AfcProtocolFactory"/> class.
        /// </param>
        /// <param name="lockDownClientFactory">
        /// A <see cref="LockdownClientFactory"/> which can create a connection to lockDown.
        /// </param>
        /// <param name="logger">
        /// A <see cref="ILogger"/> which can be used when logging.
        /// </param>
        public AfcClientFactory(MuxerClient muxer, DeviceContext context, AfcProtocolFactory afcProtocolFactory, ClientFactory<LockdownClient> lockDownClientFactory, ILogger<AfcClient> logger) 
            : base(muxer, context, afcProtocolFactory, lockDownClientFactory, logger)
        {

        }

        /// <inheritdoc/>
        public override Task<AfcClient> CreateAsync(CancellationToken cancellationToken)
            => this.CreateAsync(AfcClient.ServiceName, cancellationToken);

        /// <inheritdoc/>
        public override async Task<AfcClient> CreateAsync(string serviceName, CancellationToken cancellationToken)
        {
            var protocol = await this.StartServiceAndConnectAsync(serviceName, startSession: true, cancellationToken);

            return new AfcClient((AfcProtocol)protocol,Logger);
        }
    }
}
