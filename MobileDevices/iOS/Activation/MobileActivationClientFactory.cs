using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MobileDevices.iOS.Lockdown;
using MobileDevices.iOS.Muxer;
using MobileDevices.iOS.PropertyLists;

namespace MobileDevices.iOS.Activation
{
    /// <summary>
    /// Creates connections to the <see cref="DiagnosticsRelay"/> service running on an iOS device.
    /// </summary>
    public class MobileActivationClientFactory : ServiceClientFactory<MobileActivationClient>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MobileActivationClientFactory"/> class.
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
        public MobileActivationClientFactory(
            MuxerClient muxer, 
            DeviceContext context, 
            PropertyListProtocolFactory propertyListProtocolFactory, 
            ClientFactory<LockdownClient> lockdownClientFactory,
            ILogger<MobileActivationClient> logger)
            : base(muxer, context, propertyListProtocolFactory, lockdownClientFactory, logger)
        {
        }

        /// <inheritdoc/>
        public override Task<MobileActivationClient> CreateAsync(CancellationToken cancellationToken)
            => this.CreateAsync(MobileActivationClient.ServiceName, cancellationToken);

        /// <inheritdoc/>
        public override async Task<MobileActivationClient> CreateAsync(string serviceName, CancellationToken cancellationToken)
        {
            var protocol = await this.StartServiceAndConnectAsync(serviceName, startSession: true, cancellationToken);
            return new MobileActivationClient((PropertyListProtocol)protocol);
        }


        public async Task<MobileActivationClient> CreateAsync(ServiceDescriptor serviceDescriptor, CancellationToken cancellationToken)
        {
            var protocol = await this.ConnectServiceAsync(serviceDescriptor, cancellationToken);
            return new MobileActivationClient((PropertyListProtocol)protocol);
        }

    }
}