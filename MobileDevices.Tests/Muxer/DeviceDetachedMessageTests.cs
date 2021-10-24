using Claunia.PropertyList;
using MobileDevices.iOS.Muxer;
using System;
using Xunit;

namespace MobileDevices.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="DeviceDetachedMessage"/> class.
    /// </summary>
    public class DeviceDetachedMessageTests
    {
        /// <summary>
        /// The <see cref="DeviceDetachedMessage.Read(NSDictionary)"/> method throws an
        /// <see cref="ArgumentNullException"/> when passed a <see langword="null"/> value.
        /// </summary>
        [Fact]
        public void Read_NullArgument_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>("data", () => DeviceDetachedMessage.Read(null));
        }

        /// <summary>
        /// The <see cref="DeviceDetachedMessage.Read(NSDictionary)"/> method propertly
        /// parses its property list representation.
        /// </summary>
        [Fact]
        public void Read_Works()
        {
            var message = DeviceDetachedMessage.Read((NSDictionary)PropertyListParser.Parse("Muxer/detached.xml"));
            Assert.Equal(2, message.DeviceID);
            Assert.Equal(MuxerMessageType.Detached, message.MessageType);
        }
    }
}
