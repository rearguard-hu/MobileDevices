using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Adds the <see cref="MuxerClient.ListenAsync"/> method.
    /// </summary>
    public partial class MuxerClient
    {
        /// <summary>
        /// Listens for device notifications on a <see cref="MuxerProtocol"/>.
        /// </summary>
        /// <param name="onAttached">
        /// The action to take when an <see cref="DeviceAttachedMessage"/> is received.
        /// </param>
        /// <param name="onDetached">
        /// The action to take when an <see cref="DeviceDetachedMessage"/> is received.
        /// </param>
        /// <param name="onPaired">
        /// The action to take when an <see cref="DevicePairedMessage"/> is received.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation. Returns <see langword="true"/> when
        /// the client aborted the listen operation, <see langword="false"/> when the server disconnected.
        /// </returns>
        public virtual async Task<bool> ListenAsync(
            Func<DeviceAttachedMessage, CancellationToken, Task<MuxerListenAction>> onAttached,
            Func<DeviceDetachedMessage, CancellationToken, Task<MuxerListenAction>> onDetached,
            Func<DevicePairedMessage, CancellationToken, Task<MuxerListenAction>> onPaired,
            CancellationToken cancellationToken)
        {
            var protocol = await this.TryConnectToMuxerAsync(cancellationToken).ConfigureAwait(false);

            if (protocol == null)
            {
                return false;
            }

            await using (protocol)
            {
                // Send the Listen request
                await protocol.WriteMessageAsync(
                    new ListenMessage()
                    {
                        ConnType = 1,
                        MessageType = MuxerMessageType.Listen,
                        kLibUSBMuxVersion = 3,
                    },
                    cancellationToken).ConfigureAwait(false);

                var message = await protocol.ReadMessageAsync(cancellationToken).ConfigureAwait(false);

                if (message == null)
                {
                    this.logger.LogWarning("The server unexpectedly close the connection when sending the Listen command.");
                    return false;
                }

                var resultMessage = (ResultMessage)message;
                if (resultMessage.Number != MuxerError.Success)
                {
                    throw new MuxerException("An error occurred while listening for device notifications.", resultMessage.Number);
                }

                //设备监听
                while ((message = await protocol.ReadMessageAsync(cancellationToken).ConfigureAwait(false)) != null)
                {
                    if (message is DeviceAttachedMessage)
                    {
                        var attachedMessage = (DeviceAttachedMessage)message;

                        if (onAttached != null)
                        {
                            if (await onAttached(attachedMessage, cancellationToken) == MuxerListenAction.StopListening)
                            {
                                break;
                            }
                        }
                    }
                    else if (message is DeviceDetachedMessage)
                    {
                        var detachedMessage = (DeviceDetachedMessage)message;

                        if (onDetached != null)
                        {
                            if (await onDetached(detachedMessage, cancellationToken) == MuxerListenAction.StopListening)
                            {
                                break;
                            }
                        }
                    }
                    else if (message is DevicePairedMessage)
                    {
                        var pairedMessage = (DevicePairedMessage)message;

                        if (onPaired != null)
                        {
                            if (await onPaired(pairedMessage, cancellationToken) == MuxerListenAction.StopListening)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        throw new InvalidDataException();
                    }
                }

                // If message is null, the server closed the connection. Let the caller know.
                if (message == null)
                {
                    this.logger.LogWarning("The server unexpectedly close the connection when listening for device notifications.");
                }

                return message != null;
            }
        }
    }
}
