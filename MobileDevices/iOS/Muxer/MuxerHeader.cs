namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Represents the header of a muxer message.
    /// </summary>
    public partial struct MuxerHeader
    {
        /// <summary>
        /// Gets or sets the total message length, including the header length.
        /// </summary>
        public uint Length;

        /// <summary>
        /// Gets or sets the version of the protocol being used.
        /// </summary>
        public uint Version;

        /// <summary>
        /// Gets or sets the type of message being sent.
        /// </summary>
        public MuxerMessageType Message;

        /// <summary>
        /// Gets or sets a sequence number which matches requests and responses.
        /// </summary>
        public uint Tag;
    }
}
