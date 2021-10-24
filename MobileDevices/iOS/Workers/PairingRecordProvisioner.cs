using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MobileDevices.iOS.DependencyInjection;
using MobileDevices.iOS.Lockdown;
using MobileDevices.iOS.Muxer;

namespace MobileDevices.iOS.Workers
{
    /// <summary>
    /// Selects a pairing record which can be used with a device, by selecting a pairing record which is available in the muxer,
    /// in the cluster, or by creating a new pairing record.
    /// </summary>
    public class PairingRecordProvisioner
    {
        private readonly MuxerClient muxerClient;
        private readonly ILogger<PairingRecordProvisioner> logger;
        private readonly ClientFactory<LockdownClient> lockDownClient;
        private readonly PairingWorker pairingWorker;

        /// <summary>
        /// Initializes a new instance of the <see cref="PairingRecordProvisioner"/> class.
        /// </summary>
        /// <param name="muxerClient">
        /// A <see cref="MuxerClient"/> which represents the connection to the muxer.
        /// </param>
        /// <param name="serviceProvider">
        /// A <see cref="DeviceServiceProvider"/> which can be used to acquire services.
        /// </param>
        /// <param name="logger">
        /// A logger which can be used when logging.
        /// </param>
        public PairingRecordProvisioner(
            MuxerClient muxerClient,
            DeviceServiceProvider serviceProvider,
            ClientFactory<LockdownClient> lockDownClient, 
            PairingWorker pairingWorker,
            ILogger<PairingRecordProvisioner> logger)
        {
            this.muxerClient = muxerClient ?? throw new ArgumentNullException(nameof(muxerClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.pairingWorker = pairingWorker;
            this.lockDownClient = lockDownClient;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PairingRecordProvisioner"/> class. Intended for mocking purposes only.
        /// </summary>
        protected PairingRecordProvisioner(ClientFactory<LockdownClient> lockDownClient, PairingWorker pairingWorker)
        {
            this.lockDownClient = lockDownClient;
            this.pairingWorker = pairingWorker;
        }

        /// <summary>
        /// Asynchronously retrieves or generates a pairing record for a device.
        /// </summary>
        /// <param name="udid">
        /// The UDID of the device for which to retrieve or generate the pairing record.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public virtual async Task<PairingRecord> ProvisionPairingRecordAsync(string udid, CancellationToken cancellationToken)
        {
            // Pairing records can be stored at both the cluster level and locally. Use the first pairing record which is valid, and make sure the cluster
            // and local records are in sync.
            this.logger.LogInformation("Provisioning a pairing record for device {udid}", udid);

            var usbmuxdPairingRecord = await this.muxerClient.ReadPairingRecordAsync(udid, cancellationToken).ConfigureAwait(false);

            this.logger.LogInformation("Found pairing record {usbmuxdPairingRecord} in usbmuxd", usbmuxdPairingRecord);

            PairingRecord pairingRecord = null;


            await using (var lockdown = await lockDownClient.CreateAsync(cancellationToken).ConfigureAwait(false))
            {

                if (usbmuxdPairingRecord != null && await lockdown.ValidatePairAsync(usbmuxdPairingRecord, cancellationToken).ConfigureAwait(false))
                {

                    this.logger.LogInformation("The pairing record stored in usbmuxd is valid.");
                    pairingRecord = usbmuxdPairingRecord;
                }
                else
                {
                    this.logger.LogInformation("No valid pairing record could be found.");

                    this.logger.LogInformation("Starting a new pairing task");

                    pairingRecord = await pairingWorker.PairAsync(cancellationToken);
                }

                lockDownClient.Context.PairingRecord = pairingRecord;
            }



            // Update outdated pairing records if required.
            if (!PairingRecord.Equals(pairingRecord, usbmuxdPairingRecord))
            {
                this.logger.LogInformation("The pairing record stored in usbmuxd for device {device} is outdated. Updating.", udid);
                if (usbmuxdPairingRecord != null)
                {
                    await this.muxerClient.DeletePairingRecordAsync(udid, cancellationToken).ConfigureAwait(false);
                }

                await this.muxerClient.SavePairingRecordAsync(udid, pairingRecord, cancellationToken).ConfigureAwait(false);
                this.logger.LogInformation("Updated the pairing record in usbmuxd for device {device}.", udid);
            }

            return pairingRecord;
        }
    }
}
