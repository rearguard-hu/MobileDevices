using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.Lockdown
{
    public partial class LockdownClient
    {
        /// <summary>
        /// Asynchronously sends a request to Activate a device with the host.
        /// </summary>
        /// <param name="pairingRecord">
        /// A pairing record which represents the pairing request.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous
        /// operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task<LockdownResponse> ActivateAsync(object activationRecord, CancellationToken cancellationToken)
        {
            await this.protocol.WriteMessageAsync(
                new ActivateRequest()
                {
                    Label = this.Label,
                    ActivationRecord = activationRecord
                },
                cancellationToken).ConfigureAwait(false);

            var response = await this.protocol.ReadMessageAsync<LockdownResponse>(cancellationToken).ConfigureAwait(false);

            return response;
        }

        public virtual async Task<LockdownResponse> DeactivateAsync(CancellationToken cancellationToken)
        {
            await this.protocol.WriteMessageAsync(
                new ActivateRequest()
                {
                    Label = this.Label,
                    Request = "Deactivate"
                },
                cancellationToken).ConfigureAwait(false);

            var response = await this.protocol.ReadMessageAsync<LockdownResponse>(cancellationToken).ConfigureAwait(false);

            return response;
        }

    }
}
