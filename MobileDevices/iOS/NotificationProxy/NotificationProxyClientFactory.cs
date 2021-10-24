using MobileDevices.iOS.Lockdown;
using MobileDevices.iOS.Muxer;
using MobileDevices.iOS.PropertyLists;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.NotificationProxy
{
    /// <summary>
    /// A <see cref="ServiceClientFactory{T}"/> which can create <see cref="NotificationProxyClient"/> clients.
    /// </summary>
    public class NotificationProxyClientFactory : ServiceClientFactory<NotificationProxyClient>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationProxyClientFactory"/> class.
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
        public NotificationProxyClientFactory(MuxerClient muxer, DeviceContext context, PropertyListProtocolFactory propertyListProtocolFactory, ClientFactory<LockdownClient> lockdownClientFactory, ILogger<NotificationProxyClient> logger)
            : base(muxer, context, propertyListProtocolFactory, lockdownClientFactory, logger)
        {
        }

        /// <inheritdoc/>
        public override Task<NotificationProxyClient> CreateAsync(CancellationToken cancellationToken)
            => this.CreateAsync(NotificationProxyClient.ServiceName, cancellationToken);

        /// <inheritdoc/>
        public override async Task<NotificationProxyClient> CreateAsync(string serviceName, CancellationToken cancellationToken)
        {
            var protocol = await this.StartServiceAndConnectAsync(serviceName, startSession: serviceName == NotificationProxyClient.ServiceName, cancellationToken);
            return new NotificationProxyClient((PropertyListProtocol)protocol);
        }
    }
}
