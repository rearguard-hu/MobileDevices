using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.Lockdown
{
    /// <summary>
    /// A generic store for pairing records.
    /// </summary>
    public abstract class PairingRecordStore
    {
        /// <summary>
        /// Asynchronously retrieves the pairing record for a device.
        /// </summary>
        /// <param name="udid">
        /// The UDID of the device for which to retrieve the pairing record.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// If found, the requested <see cref="PairingRecord"/>; otherwise, <see langword="null"/>.
        /// </returns>
        public abstract Task<PairingRecord> ReadAsync(string udid, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously saves the pairing record for a device.
        /// </summary>
        /// <param name="udid">
        /// The UDID of the device for which to store the pairing record.
        /// </param>
        /// <param name="pairingRecord">
        /// The pairing record to store.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public abstract Task WriteAsync(string udid, PairingRecord pairingRecord, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously deletes a pairing record for a device.
        /// </summary>
        /// <param name="udid">
        /// The UDID of the device for which to delete the pairing record.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to delete the pairing record.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public abstract Task DeleteAsync(string udid, CancellationToken cancellationToken);
    }
}
