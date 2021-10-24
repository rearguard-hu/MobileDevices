using MobileDevices.iOS.Lockdown;
using MobileDevices.iOS.Muxer;
using MobileDevices.iOS.NotificationProxy;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.Workers
{
    /// <summary>
    /// A worker which pairs a device with a host.
    /// </summary>
    public class PairingWorker
    {
        private readonly MuxerClient muxer;
        private readonly DeviceContext context;
        private readonly ClientFactory<LockdownClient> lockdownClientFactory;
        private readonly ClientFactory<NotificationProxyClient> notificationProxyClientFactory;
        private readonly PairingRecordGenerator pairingRecordGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="PairingWorker"/> class.
        /// </summary>
        /// <param name="muxerClient">
        /// A <see cref="MuxerClient"/> which can be used to interact with the muxer.
        /// </param>
        /// <param name="context">
        /// The device context.
        /// </param>
        /// <param name="lockdownClientFactory">
        /// A <see cref="LockdownClientFactory"/> which can be used to construct a lockdown client.
        /// </param>
        /// <param name="notificationProxyClientFactory">
        /// A <see cref="NotificationProxyClientFactory"/> which can be used to construct a notification proxy client.
        /// </param>
        /// <param name="pairingRecordGenerator">
        /// A <see cref="pairingRecordGenerator"/> which can be used to generate a pairing record.
        /// </param>
        public PairingWorker(
            MuxerClient muxerClient,
            DeviceContext context,
            ClientFactory<LockdownClient> lockdownClientFactory,
            ClientFactory<NotificationProxyClient> notificationProxyClientFactory,
            PairingRecordGenerator pairingRecordGenerator)
        {
            this.muxer = muxerClient ?? throw new ArgumentNullException(nameof(muxerClient));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.notificationProxyClientFactory = notificationProxyClientFactory ?? throw new ArgumentNullException(nameof(notificationProxyClientFactory));
            this.lockdownClientFactory = lockdownClientFactory ?? throw new ArgumentNullException(nameof(lockdownClientFactory));
            this.pairingRecordGenerator = pairingRecordGenerator ?? throw new ArgumentNullException(nameof(pairingRecordGenerator));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PairingWorker"/> class. Intended for mocking purposes only.
        /// </summary>
        protected PairingWorker()
        {
        }

        /// <summary>
        /// Asynchronously pairs a device with the host.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task completes when the device has
        /// paired successfully, or when the user has refused pairing.
        /// </returns>
        public virtual async Task<PairingRecord> PairAsync(CancellationToken cancellationToken)
        {
            PairingResult result = null;

            await using (var notificationProxyClient = await this.notificationProxyClientFactory.CreateAsync(NotificationProxyClient.InsecureServiceName, cancellationToken))
            {
                // Very early implementations listen for a "request host buid" notification, too. When this notification is received, the
                // host should set the "UntrustedHostBUID" lockdown property with the system buid. However, it looks like this is not needed
                // for recent versions of lockdown, and is currently not implemented.
                await notificationProxyClient.ObserveNotificationAsync(Notifications.RequestPair, cancellationToken).ConfigureAwait(false);

                // First, attempt to validate the current pairing record stored in usbmuxd (if any such pairing record is present).
                var pairingRecord = await this.muxer.ReadPairingRecordAsync(this.context.Device.Udid, cancellationToken).ConfigureAwait(false);
                if (pairingRecord != null)
                {
                    await using (var lockdownClient = await this.lockdownClientFactory.CreateAsync(cancellationToken))
                    {
                        // Check the validity of the current pairing record. If the pairing record is valid (i.e. we can start a Lockdown session),
                        // use that pairing record. If the pairing record is not valid, we need to check whether we're still pending user validation
                        // of this pairing record or whether the user has rejected pairing.
                        if (await lockdownClient.ValidatePairAsync(pairingRecord, cancellationToken).ConfigureAwait(false))
                        {
                            return pairingRecord;
                        }

                        result = await lockdownClient.PairAsync(pairingRecord, cancellationToken).ConfigureAwait(false);

                        // If the pairing record is invalid, delete the pairing record from the muxer.
                        if (result?.Status == PairingStatus.UserDeniedPairing)
                        {
                            result = await lockdownClient.UnpairAsync(pairingRecord, cancellationToken).ConfigureAwait(false);
                            await this.muxer.DeletePairingRecordAsync(this.context.Device.Udid, cancellationToken).ConfigureAwait(false);

                            pairingRecord = null;
                            result = null;
                        }
                        else if (result?.Status != PairingStatus.PairingDialogResponsePending
                            && result.Status != PairingStatus.Success)
                        {
                            await this.muxer.DeletePairingRecordAsync(this.context.Device.Udid, cancellationToken).ConfigureAwait(false);

                            pairingRecord = null;
                            result = null;
                        }
                    }
                }

                // If a valid pairing record was not found, create a new one, and store it in the muxer.
                if (pairingRecord == null)
                {
                    await using (var lockdownClient = await this.lockdownClientFactory.CreateAsync(cancellationToken))
                    {
                        var devicePublicKey = await lockdownClient.GetPublicKeyAsync(cancellationToken).ConfigureAwait(false);
                        var wifi = await lockdownClient.GetWifiAddressAsync(cancellationToken).ConfigureAwait(false);
                        var systemBuid = await this.muxer.ReadBuidAsync(cancellationToken).ConfigureAwait(false);

                        pairingRecord = this.pairingRecordGenerator.Generate(devicePublicKey, systemBuid);
                        pairingRecord.WiFiMacAddress = wifi;

                        result = await lockdownClient.PairAsync(pairingRecord, cancellationToken).ConfigureAwait(false);


                        Debug.Assert(
                            result?.Status == PairingStatus.PairingDialogResponsePending || result?.Status == PairingStatus.Success,
                            "Invalid response");

                        await this.muxer.SavePairingRecordAsync(this.context.Device.Udid, pairingRecord, cancellationToken).ConfigureAwait(false);
                    }
                }

                Debug.Assert(pairingRecord != null, "A pairing record should exist");

                // At this point, we have a pairing record. If the we're pending a response from the user, wait for that response.
                if (result?.Status == PairingStatus.PairingDialogResponsePending)
                {
                    string notification = await notificationProxyClient.ReadRelayNotificationAsync(cancellationToken).ConfigureAwait(false);
                    Debug.Assert(notification == Notifications.RequestPair, "Got an unexpected notification");

                    await using (var lockdownClient = await this.lockdownClientFactory.CreateAsync(cancellationToken))
                    {
                        result = await lockdownClient.PairAsync(pairingRecord, cancellationToken).ConfigureAwait(false);

                    }

                    // Save the escrow bag if one is available.
                    if (result.Status == PairingStatus.Success)
                    {
                        if (pairingRecord.EscrowBag == null && result.EscrowBag != null)
                        {
                            pairingRecord.EscrowBag = result.EscrowBag;
                            await this.muxer.SavePairingRecordAsync(udid: this.context.Device.Udid, pairingRecord, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        await this.muxer.DeletePairingRecordAsync(udid: this.context.Device.Udid, cancellationToken).ConfigureAwait(false);
                    }
                }

                // Return the pairing record if we paired successfully; otherwise, return null.
                if (result?.Status == PairingStatus.Success)
                {
                    return pairingRecord;
                }

                return null;
            }
        }
    }
}
