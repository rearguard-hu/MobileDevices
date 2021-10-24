using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MobileDevices.iOS.AfcServices;
using MobileDevices.iOS.PropertyLists;

namespace MobileDevices.iOS.CrashReport
{
    public class CrashReportCopyClient : IAsyncDisposable
    {
        /// <summary>
        /// Gets the name of the crash report mover running on the device.
        /// </summary>
        public const string ServiceName = "com.apple.crashreportcopymobile";

        private readonly AfcClient afcClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrashReportCopyClient"/> class.  统计报告数量
        /// </summary>
        /// <param name="afcClient">
        /// A <see cref="PropertyListProtocol"/> which represents a connection to the diagnostics relay service running on the device.
        /// </param>
        public CrashReportCopyClient(AfcClient afcClient)
        {
            this.afcClient = afcClient ?? throw new ArgumentNullException(nameof(afcClient));
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            return this.afcClient.DisposeAsync();
        }

        public async Task<Dictionary<string, object>> NumberStatisticalReportsAsync(CancellationToken cancellationToken)
        {
            var list = await afcClient.ReadDirectoryAsync(".", cancellationToken);

            var panicFullCount = list.Count(x => x.Contains("panic-full"));
            var resetCounterCount = list.Count(x => x.Contains("ResetCounter-"));


            var resultDic = new Dictionary<string, object>
            {
                {"ResetCounter", resetCounterCount.ToString()},
                {"panicFull", panicFullCount.ToString()}
            };

            return resultDic;
        }


    }
}