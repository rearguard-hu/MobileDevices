using MobileDevices.iOS.Lockdown;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// A <see cref="PairingRecordStore"/> which reads and writes pairing records for the muxer.
    /// </summary>
    public class MuxerPairingRecordStore : PairingRecordStore
    {
        private readonly MuxerClient muxer;
        private readonly ILogger<MuxerPairingRecordStore> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MuxerPairingRecordStore"/> class.
        /// </summary>
        /// <param name="muxer">
        /// A <see cref="MuxerClient"/> which represents the connectivity to the muxer.
        /// </param>
        /// <param name="logger">
        /// A logger which can be used to log messages.
        /// </param>
        public MuxerPairingRecordStore(MuxerClient muxer, ILogger<MuxerPairingRecordStore> logger)
        {
            this.muxer = muxer ?? throw new ArgumentNullException(nameof(muxer));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public override Task DeleteAsync(string udid, CancellationToken cancellationToken)
        {
            return this.muxer.DeletePairingRecordAsync(udid, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task<PairingRecord> ReadAsync(string udid, CancellationToken cancellationToken)
        {
            return this.muxer.ReadPairingRecordAsync(udid, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task WriteAsync(string udid, PairingRecord pairingRecord, CancellationToken cancellationToken)
        {
            return this.muxer.SavePairingRecordAsync(udid, pairingRecord, cancellationToken);
        }
    }
}
