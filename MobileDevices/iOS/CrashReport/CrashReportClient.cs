using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MobileDevices.iOS.AfcServices;
using MobileDevices.iOS.DiagnosticsRelay;
using MobileDevices.iOS.PropertyLists;

namespace MobileDevices.iOS.CrashReport
{
    /// <summary>
    /// A client which interacts with the crash report mover running on an iOS device.
    /// </summary>
    public class CrashReportClient : IAsyncDisposable
    {
        /// <summary>
        /// Gets the name of the crash report mover running on the device.
        /// </summary>
        public const string ServiceName = "com.apple.crashreportmover";

        private readonly AfcProtocol protocol;


        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticsRelayClient"/> class.
        /// </summary>
        /// <param name="protocol">
        /// A <see cref="PropertyListProtocol"/> which represents a connection to the diagnostics relay service running on the device.
        /// </param>
        public CrashReportClient(AfcProtocol protocol)
        {
            this.protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
        }

        public async Task ReadPingAsync(CancellationToken token)
        {
            var num = 0;
            while (!token.IsCancellationRequested)
            {
                using var packetOwner = await protocol.ReceiveRawDataAsync(token);
                var buffer = packetOwner.Memory;

                var result = Encoding.UTF8.GetString(buffer.Span);

                if (!result.TrimEnd('\0').Equals("ping")|| num>=10)
                    break;

                num++;
            }
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            return protocol.DisposeAsync();
        }
    }
}
