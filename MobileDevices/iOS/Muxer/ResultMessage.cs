namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Represents the result of a method invocation request.
    /// </summary>
    public partial class ResultMessage : MuxerMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResultMessage"/> class.
        /// </summary>
        public ResultMessage()
        {
            this.MessageType = MuxerMessageType.Result;
        }

        /// <summary>
        /// Gets or sets the response code.
        /// </summary>
        public MuxerError Number
        {
            get;
            set;
        }
    }
}
