namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// The message which is sent by <c>usbmuxd</c> when a device has paired succesfully with the host.
    /// </summary>
    /// <remarks>
    /// This message is not sent for devices which have previously paired with the PC and are attached
    /// to the host; so the absence of a pairing message does not indicate that the device is not paired
    /// with the host.
    /// </remarks>
    public partial class DevicePairedMessage : DeviceMessage
    {
    }
}
