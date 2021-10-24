namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Represents the different types of messages which can be exchanged with the muxer.
    /// </summary>
    public enum MuxerMessageType : uint
    {
        /// <summary>
        /// The message type has not been intialized. This is not a valid value.
        /// </summary>
        None = 0,

        /// <summary>
        /// The message is a result.
        /// </summary>
        Result = 1,

        /// <summary>
        /// The message is a request to connect to the device.
        /// </summary>
        Connect = 2,

        /// <summary>
        /// The message is a request to receive updates about devices.
        /// </summary>
        Listen = 3,

        /// <summary>
        /// A device has been added.
        /// </summary>
        Attached = 4,

        /// <summary>
        /// A device has been removed.
        /// </summary>
        Detached = 5,

        /// <summary>
        /// A device has paired with the PC.
        /// </summary>
        Paired = 6,

        /// <summary>
        /// The message contains property list data.
        /// </summary>
        Plist = 8,

        /// <summary>
        /// Lists all devices currently connected to the PC.
        /// </summary>
        ListDevices = 100,

        /// <summary>
        /// Requests the BUID of the device.
        /// </summary>
        ReadBUID = 101,

        /// <summary>
        /// Reads the pair record for the device.
        /// </summary>
        ReadPairRecord = 102,

        /// <summary>
        /// Saves the pair record for the device.
        /// </summary>
        SavePairRecord = 103,

        /// <summary>
        /// Deletes the pair record for the device.
        /// </summary>
        DeletePairRecord = 104,
    }
}
