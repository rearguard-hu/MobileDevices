using System;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.Lockdown
{
    /// <content>
    /// Session-related methods.
    /// </content>
    public partial class LockdownClient
    {
        /// <summary>
        /// Attempts to start a new session.
        /// </summary>
        /// <param name="pairingRecord">
        /// The pairing record used to authenticate the host with the device.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// The response to the request to start a new session.
        /// </returns>
        public async Task<StartSessionResponse>  TryStartSessionAsync(PairingRecord pairingRecord, CancellationToken cancellationToken)
        {
            if (pairingRecord == null)
            {
                throw new ArgumentNullException(nameof(pairingRecord));
            }

            await this.protocol.WriteMessageAsync(
                new StartSessionRequest()
                {
                    Label = this.Label,
                    HostID = pairingRecord.HostId,
                    Request = "StartSession",
                    SystemBUID = pairingRecord.SystemBUID,
                },
                cancellationToken).ConfigureAwait(false);

            var message = await this.protocol.ReadMessageAsync<StartSessionResponse>(cancellationToken).ConfigureAwait(false);

            if (message.EnableSessionSSL)
            {
                await this.protocol.EnableSslAsync(pairingRecord, cancellationToken).ConfigureAwait(false);
            }

            return message;
        }

        /// <summary>
        /// Starts a new exception, and throws an exception when a new session could not be created.
        /// </summary>
        /// <param name="pairingRecord">
        /// The pairing record used to authenticate the host with the device.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// The response to the request to start a new session.
        /// </returns>
        public virtual async Task<StartSessionResponse> StartSessionAsync(PairingRecord pairingRecord, CancellationToken cancellationToken)
        {
            var message = await this.TryStartSessionAsync(pairingRecord, cancellationToken).ConfigureAwait(false);

            this.EnsureSuccess(message);

            return message;
        }

        /// <summary>
        /// Stop the currently active session.
        /// </summary>
        /// <param name="sessionId">
        /// The session ID of the session to stop.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public virtual async Task StopSessionAsync(string sessionId, CancellationToken cancellationToken)
        {
            if (sessionId == null)
            {
                throw new ArgumentNullException(nameof(sessionId));
            }

            await this.protocol.WriteMessageAsync(
                new StopSessionRequest()
                {
                    Label = this.Label,
                    SessionID = sessionId,
                    Request = "StopSession",
                },
                cancellationToken).ConfigureAwait(false);

            var response = await this.protocol.ReadMessageAsync<LockdownResponse>(cancellationToken).ConfigureAwait(false);
            this.EnsureSuccess(response);

            if (this.protocol.SslEnabled)
            {
                await this.protocol.DisableSslAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
