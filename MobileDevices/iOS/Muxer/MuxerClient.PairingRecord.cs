using MobileDevices.iOS.Lockdown;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.Muxer
{
    /// <content>
    /// Adds methods to the <see cref="MuxerClient"/> class which can be used to read and write pairing records.
    /// </content>
    public partial class MuxerClient
    {
        /// <summary>
        /// Asynchronously reads the pair record for a device.
        /// </summary>
        /// <param name="udid">
        /// The UDID of the device for which to read the pair record.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and which returns the pairing
        /// record.
        /// </returns>
        public virtual async Task<PairingRecord> ReadPairingRecordAsync(string udid, CancellationToken cancellationToken)
        {
            if (udid == null)
            {
                throw new ArgumentNullException(nameof(udid));
            }

            await using (var protocol = await this.TryConnectToMuxerAsync(cancellationToken).ConfigureAwait(false))
            {
                // Send the read ReadPairRecord message
                await protocol.WriteMessageAsync(
                    new ReadPairingRecordMessage()
                    {
                        MessageType = MuxerMessageType.ReadPairRecord,
                        PairRecordID = udid,
                    },
                    cancellationToken).ConfigureAwait(false);

                var response = (ResultMessage)await protocol.ReadMessageAsync(cancellationToken).ConfigureAwait(false);

                if (response.Number == MuxerError.BadDevice)
                {
                    return null;
                }
                else if (response.Number != MuxerError.Success)
                {
                    throw new MuxerException($"An error occurred while reading the pairing record for device {udid}: {response.Number}.", response.Number);
                }

                var pairingRecordResponse = (PairingRecordDataMessage)response;
                var pairingRecord = PairingRecord.Read(pairingRecordResponse.PairRecordData);
                return pairingRecord;
            }
        }

        /// <summary>
        /// Asynchronously writes the pair record for a device.
        /// </summary>
        /// <param name="udid">
        /// The UDID of the device for which to save the pair record.
        /// </param>
        /// <param name="pairingRecord">
        /// The pairing record for the device.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task SavePairingRecordAsync(string udid, PairingRecord pairingRecord, CancellationToken cancellationToken)
        {
            if (udid == null)
            {
                throw new ArgumentNullException(nameof(udid));
            }

            if (pairingRecord == null)
            {
                throw new ArgumentNullException(nameof(pairingRecord));
            }

            await using (var protocol = await this.TryConnectToMuxerAsync(cancellationToken).ConfigureAwait(false))
            {
                // Send the save ReadPairRecord message
                await protocol.WriteMessageAsync(
                    new SavePairingRecordMessage()
                    {
                        MessageType = MuxerMessageType.SavePairRecord,
                        PairRecordID = udid,
                        PairRecordData = pairingRecord.ToByteArray(),
                    },
                    cancellationToken).ConfigureAwait(false);

                var response = (ResultMessage)await protocol.ReadMessageAsync(cancellationToken).ConfigureAwait(false);

                if (response.Number != MuxerError.Success)
                {
                    throw new MuxerException($"An error occurred while saving the pairing record for device {udid}: {response.Number}.", response.Number);
                }
            }
        }

        /// <summary>
        /// Asynchronously deletes the pair record for a device.
        /// </summary>
        /// <param name="udid">
        /// The UDID of the device for which to delete the pair record.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task DeletePairingRecordAsync(string udid, CancellationToken cancellationToken)
        {
            if (udid == null)
            {
                throw new ArgumentNullException(nameof(udid));
            }

            await using (var protocol = await this.TryConnectToMuxerAsync(cancellationToken).ConfigureAwait(false))
            {
                await protocol.WriteMessageAsync(
                    new DeletePairingRecordMessage()
                    {
                        MessageType = MuxerMessageType.DeletePairRecord,
                        PairRecordID = udid,
                    },
                    cancellationToken).ConfigureAwait(false);

                var response = (ResultMessage)await protocol.ReadMessageAsync(cancellationToken).ConfigureAwait(false);

                if (response.Number != MuxerError.Success)
                {
                    throw new MuxerException($"An error occurred while saving the pairing record for device {udid}: {response.Number}.", response.Number);
                }
            }
        }
    }
}
