using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Implements the <see cref="MuxerClient.ListDevicesAsync(CancellationToken)"/> method.
    /// </summary>
    public partial class MuxerClient
    {
        /// <summary>
        /// Lists all devices which are currently connected to this muxer in a single operation.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public virtual async Task<Collection<MuxerDevice>> ListDevicesAsync(CancellationToken cancellationToken)
        {
            Collection<MuxerDevice> devices = new Collection<MuxerDevice>();

            // On Linux, usbmuxd is not running if no devices are connected. In this scenario,
            // TryConnect() will return null. Don't error out but return an empty list instead.
            var protocol = await this.TryConnectToMuxerAsync(cancellationToken).ConfigureAwait(false);

            if (protocol == null)
            {
                return devices;
            }

            await using (protocol)
            {
                await protocol.WriteMessageAsync(
                    new RequestMessage()
                    {
                        MessageType = MuxerMessageType.ListDevices,
                    },
                    cancellationToken).ConfigureAwait(false);

                var response = await protocol.ReadMessageAsync(cancellationToken).ConfigureAwait(false);
                var deviceList = (DeviceListMessage)response;

                foreach (var device in deviceList.DeviceList)
                {
                    devices.Add(new MuxerDevice()
                    {
                        ConnectionType = device.Properties.ConnectionType,
                        DeviceID = device.Properties.DeviceID,
                        Udid = PatchUdid(device.Properties.SerialNumber),
                        IPAddress = device.Properties.ConnectionType == MuxerConnectionType.Network ? device.Properties.IPAddress : null,
                    });
                }
            }

            return devices;
        }

        /// <summary>
        /// Patches 24-character UDIDs. The USB multiplexer may return 24-character UDIDs, which are missing
        /// a dash (-) at the 9th position. This fixes that.
        /// </summary>
        /// <param name="udid">
        /// The UDID, as reported by the USB multiplexor.
        /// </param>
        /// <returns>
        /// The corrected UDID.
        /// </returns>
        public static string PatchUdid(string udid)
        {
            if (udid == null || udid.Length != 24)
            {
                return udid;
            }

            return udid.Substring(0, 8) + "-" + udid.Substring(8);
        }
    }
}
