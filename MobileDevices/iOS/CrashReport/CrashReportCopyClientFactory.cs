using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MobileDevices.iOS.AfcServices;
using MobileDevices.iOS.DiagnosticsRelay;
using MobileDevices.iOS.Lockdown;
using MobileDevices.iOS.Muxer;

namespace MobileDevices.iOS.CrashReport
{
    public class CrashReportCopyClientFactory:ServiceClientFactory<CrashReportCopyClient>
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
        /// <param name="afcProtocolFactory">
        /// A <see cref="AfcProtocolFactory"/> which can be used to create new instances of the <see cref="AfcProtocol"/> class.
        /// </param>
        /// <param name="lockDownClientFactory">
        /// A <see cref="LockdownClientFactory"/> which can create a connection to lockDown.
        /// </param>
        /// <param name="logger">
        /// A <see cref="ILogger"/> which can be used when logging.
        /// </param>
        public CrashReportCopyClientFactory(
            MuxerClient muxer, 
            DeviceContext context,
            AfcProtocolFactory afcProtocolFactory,
            ClientFactory<LockdownClient> lockDownClientFactory,

            ILogger<CrashReportCopyClient> logger)
            : base(muxer, context, afcProtocolFactory, lockDownClientFactory, logger)
        {
        }

        /// <inheritdoc/>
        public override Task<CrashReportCopyClient> CreateAsync(CancellationToken cancellationToken)
            => this.CreateAsync(CrashReportCopyClient.ServiceName, cancellationToken);

        /// <inheritdoc/>
        public override async Task<CrashReportCopyClient> CreateAsync(string serviceName, CancellationToken cancellationToken)
        {
            var protocol = await this.StartServiceAndConnectAsync(serviceName, startSession: true, cancellationToken);
            var afcClient = new AfcClient((AfcProtocol)protocol);

            return new CrashReportCopyClient(afcClient);
        }
    }
}