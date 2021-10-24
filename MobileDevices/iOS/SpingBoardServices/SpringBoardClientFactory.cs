using MobileDevices.iOS.Lockdown;
using MobileDevices.iOS.Muxer;
using MobileDevices.iOS.PropertyLists;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.SpingBoardServices
{
    /// <summary>
    /// Connects to the SpringBoard service running on an iOS device, and creates a <see cref="SpringBoardClient"/> objects.
    /// </summary>
    public class SpringBoardClientFactory : ServiceClientFactory<SpringBoardClient>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpringBoardClientFactory"/> class.
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
        public SpringBoardClientFactory(MuxerClient muxer, DeviceContext context, PropertyListProtocolFactory propertyListProtocolFactory, LockdownClientFactory lockdownClientFactory, ILogger<SpringBoardClient> logger)
            : base(muxer, context, propertyListProtocolFactory, lockdownClientFactory, logger)
        {
        }

        /// <inheritdoc/>
        public override async Task<SpringBoardClient> CreateAsync(string serviceName, CancellationToken cancellationToken)
        {
            var protocol = await this.StartServiceAndConnectAsync(serviceName, startSession: true, cancellationToken);
            return new SpringBoardClient((PropertyListProtocol)protocol);
        }

        /// <inheritdoc/>
        public override Task<SpringBoardClient> CreateAsync(CancellationToken cancellationToken) => this.CreateAsync(SpringBoardClient.ServiceName, cancellationToken);
    }
}