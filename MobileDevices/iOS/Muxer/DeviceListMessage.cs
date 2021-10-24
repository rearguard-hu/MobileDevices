using System.Collections.ObjectModel;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Represents a mesage which contains a list of all devices which are currently connected to the host.
    /// </summary>
    public partial class DeviceListMessage : MuxerMessage
    {
        /// <summary>
        /// Gets a list of devices which are currently connected to the host.
        /// </summary>
        public Collection<DeviceAttachedMessage> DeviceList
        { get; } = new Collection<DeviceAttachedMessage>();
    }
}
