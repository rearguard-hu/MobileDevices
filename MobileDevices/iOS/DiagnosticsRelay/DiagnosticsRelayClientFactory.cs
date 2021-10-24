using MobileDevices.iOS.Lockdown;
using MobileDevices.iOS.Muxer;
using MobileDevices.iOS.PropertyLists;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.DiagnosticsRelay
{
    /// <summary>
    /// Creates connections to the <see cref="DiagnosticsRelay"/> service running on an iOS device.
    /// </summary>
    public class DiagnosticsRelayClientFactory : ServiceClientFactory<DiagnosticsRelayClient>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticsRelayClientFactory"/> class.
        /// </summary>
        /// <param name="muxer">
        /// The <see cref="MuxerClient"/> which represents the connection to the iOS USB Multiplexor.
        /// </param>
        /// <param name="context">
        /// The <see cref="DeviceContext"/> which contains information about the device with which
        /// we are interacting.
        /// </param>
        /// <param name="propertyListProtocolFactory">
        /// A <see cref="PropertyListProtocolFactory"/> which can be used to create new instances of the <see cref="PropertyListProtocol"/> class.
        /// </param>
        /// <param name="lockdownClientFactory">
        /// A <see cref="LockdownClientFactory"/> which can create a connection to lockdown.
        /// </param>
        /// <param name="logger">
        /// A <see cref="ILogger"/> which can be used when logging.
        /// </param>
        public DiagnosticsRelayClientFactory(MuxerClient muxer, DeviceContext context, PropertyListProtocolFactory propertyListProtocolFactory, ClientFactory<LockdownClient> lockdownClientFactory, ILogger<DiagnosticsRelayClient> logger)
            : base(muxer, context, propertyListProtocolFactory, lockdownClientFactory, logger)
        {
        }

        /// <inheritdoc/>
        public override Task<DiagnosticsRelayClient> CreateAsync(CancellationToken cancellationToken)
            => this.CreateAsync(DiagnosticsRelayClient.ServiceName, cancellationToken);

        /// <inheritdoc/>
        public override async Task<DiagnosticsRelayClient> CreateAsync(string serviceName, CancellationToken cancellationToken)
        {
            var protocol = await this.StartServiceAndConnectAsync(serviceName, startSession: true, cancellationToken);
            return new DiagnosticsRelayClient((PropertyListProtocol)protocol);
        }
    }
}
