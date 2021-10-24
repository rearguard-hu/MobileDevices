using Claunia.PropertyList;
using MobileDevices.iOS.Muxer;
using System;
using Xunit;

namespace MobileDevices.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="DevicePairedMessageTests"/> class.
    /// </summary>
    public class DevicePairedMessageTests
    {
        /// <summary>
        /// The <see cref="DevicePairedMessage.Read(NSDictionary)"/> method throws an
        /// <see cref="ArgumentNullException"/> when passed a <see langword="null"/> value.
        /// </summary>
        [Fact]
        public void Read_NullArgument_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>("data", () => DevicePairedMessage.Read(null));
        }

        /// <summary>
        /// The <see cref="DevicePairedMessage.Read(NSDictionary)"/> method propertly
        /// parses its property list representation.
        /// </summary>
        [Fact]
        public void Read_Works()
        {
            var message = DevicePairedMessage.Read((NSDictionary)PropertyListParser.Parse("Muxer/paired.xml"));
            Assert.Equal(3, message.DeviceID);
            Assert.Equal(MuxerMessageType.Paired, message.MessageType);
        }
    }
}
