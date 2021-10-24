using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MobileDevices.iOS.Lockdown;
using MobileDevices.iOS.Muxer;
using MobileDevices.iOS.PropertyLists;

namespace MobileDevices.iOS.Install
{
    public class InstallClientFactory : ServiceClientFactory<InstallClient>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InstallClientFactory"/> class.
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
        /// <param name="lockDownClientFactory">
        /// A <see cref="LockdownClientFactory"/> which can create a connection to lockDown.
        /// </param>
        /// <param name="logger">
        /// A <see cref="ILogger"/> which can be used when logging.
        /// </param>
        public InstallClientFactory(
            MuxerClient muxer, 
            DeviceContext context,
            PropertyListProtocolFactory propertyListProtocolFactory, 
            ClientFactory<LockdownClient> lockDownClientFactory, 
            ILogger<InstallClient> logger)
            : base(muxer, context, propertyListProtocolFactory, lockDownClientFactory, logger)
        {
        }

        /// <inheritdoc/>
        public override Task<InstallClient> CreateAsync(CancellationToken cancellationToken)
        {
            return this.CreateAsync(InstallClient.ServiceName, cancellationToken);
        }

        /// <inheritdoc/>
        public override async Task<InstallClient> CreateAsync(string serviceName, CancellationToken cancellationToken)
        {
            var protocol = await this.StartServiceAndConnectAsync(serviceName, startSession: true, cancellationToken);
            return new InstallClient((PropertyListProtocol)protocol);
        }
    }
}