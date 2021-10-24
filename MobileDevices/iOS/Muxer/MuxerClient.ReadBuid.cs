using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Contains methods for retrieving the System BUID.
    /// </summary>
    public partial class MuxerClient
    {
        /// <summary>
        /// Asynchronously reads the Bipartite Unique Identifier which uniquely identifies the usbmuxd instance.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The result of the task is the BUID
        /// of the usbmuxd instance.
        /// </returns>
        public virtual async Task<string> ReadBuidAsync(CancellationToken cancellationToken)
        {
            // On Linux, usbmuxd is not running if no devices are connected. In this scenario,
            // TryConnect() will return null. Don't error out but return an empty list instead.
            var protocol = await this.TryConnectToMuxerAsync(cancellationToken).ConfigureAwait(false);

            if (protocol == null)
            {
                return null;
            }

            await using (protocol)
            {
                await protocol.WriteMessageAsync(
                    new RequestMessage()
                    {
                        MessageType = MuxerMessageType.ReadBUID,
                    },
                    cancellationToken).ConfigureAwait(false);

                var response = await protocol.ReadMessageAsync(cancellationToken).ConfigureAwait(false);
                var buidMessage = (BuidMessage)response;

                return buidMessage.BUID;
            }
        }
    }
}
