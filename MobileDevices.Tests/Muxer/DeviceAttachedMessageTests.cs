using Claunia.PropertyList;
using MobileDevices.iOS.Muxer;
using System;
using Xunit;

namespace MobileDevices.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="DeviceAttachedMessage"/> class.
    /// </summary>
    public class DeviceAttachedMessageTests
    {
        /// <summary>
        /// The <see cref="DeviceAttachedMessage.Read(NSDictionary)"/> method throws an
        /// <see cref="ArgumentNullException"/> when passed a <see langword="null"/> value.
        /// </summary>
        [Fact]
        public void Read_NullArgument_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>("data", () => DeviceAttachedMessage.Read(null));
        }

        /// <summary>
        /// The <see cref="DeviceAttachedMessage.Read(NSDictionary)"/> method propertly
        /// parses its property list representation.
        /// </summary>
        [Fact]
        public void Read_Works()
        {
            var message = DeviceAttachedMessage.Read((NSDictionary)PropertyListParser.Parse("Muxer/attached.xml"));
            Assert.Equal(2, message.DeviceID);
            Assert.Equal(MuxerMessageType.Attached, message.MessageType);
            Assert.NotNull(message.Properties);

            Assert.Equal(480000000, message.Properties.ConnectionSpeed);
            Assert.Equal(MuxerConnectionType.USB, message.Properties.ConnectionType);
            Assert.Equal(2, message.Properties.DeviceID);
            Assert.Null(message.Properties.EscapedFullServiceName);
            Assert.Equal(0, message.Properties.InterfaceIndex);
            Assert.Null(message.Properties.IPAddress);
            Assert.Equal(65539, message.Properties.LocationID);
            Assert.Null(message.Properties.NetworkAddress);
            Assert.Equal(0x12A8, message.Properties.ProductID);
            Assert.Equal("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", message.Properties.SerialNumber);
            Assert.Null(message.Properties.UDID);
            Assert.Null(message.Properties.USBSerialNumber);
        }
    }
}
