using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Enables connecting to services running on the remote device.
    /// </summary>
    public partial class MuxerClient
    {
        /// <summary>
        /// Connects to a service on a device.
        /// </summary>
        /// <param name="device">
        /// The device to which to connect.
        /// </param>
        /// <param name="port">
        /// The TCP port number to which to connect.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and which returns a <see cref="Stream"/>
        /// which represents the connection with the device. You must dispose of this stream when you are done with it.
        /// </returns>
        public virtual async Task<Stream> ConnectAsync(MuxerDevice device, int port, CancellationToken cancellationToken)
        {
            var (error, stream) = await this.TryConnectAsync(device, port, cancellationToken).ConfigureAwait(false);

            if (error != MuxerError.Success)
            {
                // If you get error 2 (BadDevice) here, you're too late: the service has been shut
                // down because you didn't connect in time.
                throw new MuxerException(
                    $"The device returned an invalid response number when connecting. Expected Success but got {error}",
                    error);
            }

            return stream;
        }

        /// <summary>
        /// Attempts to connect to a service on a device.
        /// </summary>
        /// <param name="device">
        /// The device to which to connect.
        /// </param>
        /// <param name="port">
        /// The TCP port number to which to connect.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and which returns a <see cref="Stream"/>
        /// which represents the connection with the device. You must dispose of this stream when you are done with it.
        /// </returns>
        public virtual async Task<(MuxerError, Stream)> TryConnectAsync(MuxerDevice device, int port, CancellationToken cancellationToken)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            var bigEndianPort = unchecked((ushort)((port >> 8 & 0xFF) | (port & 0xFF) << 8));

            var protocol = await this.TryConnectToMuxerAsync(cancellationToken).ConfigureAwait(false);

            if (protocol == null)
            {
                this.logger.LogWarning("Could not connect to the server.");
                return (MuxerError.MuxerError, null);
            }

            // Send the connect request to the muxer.
            await protocol.WriteMessageAsync(
                new ConnectMessage()
                {
                    DeviceID = device.DeviceID,
                    PortNumber = bigEndianPort,
                    MessageType = MuxerMessageType.Connect,
                },
                cancellationToken).ConfigureAwait(false);

            // Read the response
            var response = await protocol.ReadMessageAsync(cancellationToken).ConfigureAwait(false);

            if (response == null)
            {
                this.logger.LogWarning("The server unexpectedly close the connection when sending the Connect command.");
                return (MuxerError.MuxerError, null);
            }

            var result = (ResultMessage)response;
            if (result.Number != MuxerError.Success)
            {
                return (result.Number, null);
            }

            // The muxer will now forward all messages to the service listening at the port specified on the device.
            // The caller can use this stream (but must dispose of it).
            return (result.Number, protocol.Stream);
        }
    }
}
