using Claunia.PropertyList;
using System;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Represents a message which is sent to, or received from, <c>usbmuxd</c>.
    /// </summary>
    public abstract partial class MuxerMessage
    {
        /// <summary>
        /// Gets or sets the type of message which was sent or received.
        /// </summary>
        public MuxerMessageType MessageType
        {
            get;
            set;
        }

        /// <summary>
        /// Converest this <see cref="MuxerMessage"/> to its property list representation.
        /// </summary>
        /// <returns>
        /// A <see cref="NSDictionary"/> which represents this <see cref="MuxerMessage"/>
        /// as a property list.
        /// </returns>
        public virtual NSDictionary ToPropertyList()
        {
            throw new NotSupportedException();
        }
    }
}
