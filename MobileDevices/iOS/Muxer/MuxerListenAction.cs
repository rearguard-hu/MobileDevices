namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Defines whether a listener should continue listening or stop listening for device connect or disconnect events.
    /// </summary>
    public enum MuxerListenAction : byte
    {
        /// <summary>
        /// The listener should keep listening for new device connect or disconnect events.
        /// </summary>
        ContinueListening = 1,

        /// <summary>
        /// The listener should stop listening for device connect or disconnect events.
        /// </summary>
        StopListening = 2,
    }
}
