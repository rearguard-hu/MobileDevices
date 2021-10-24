using Claunia.PropertyList;
using MobileDevices.iOS.Muxer;
using System;
using Xunit;

namespace MobileDevices.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="BuidMessage"/> class.
    /// </summary>
    public class BuidMessageTests
    {
        /// <summary>
        /// The <see cref="BuidMessage.Read(NSDictionary)"/> method throws an
        /// <see cref="ArgumentNullException"/> when passed a <see langword="null"/> value.
        /// </summary>
        [Fact]
        public void Read_NullArgument_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>("data", () => BuidMessage.Read(null));
        }

        /// <summary>
        /// The <see cref="DeviceAttachedMessage.Read(NSDictionary)"/> method propertly
        /// parses its property list representation.
        /// </summary>
        [Fact]
        public void Read_Works()
        {
            var message = BuidMessage.Read((NSDictionary)PropertyListParser.Parse("Muxer/buid.xml"));
            Assert.Equal("32B5AAD2-16AE-7B85-DEBE-11DC865B9E32", message.BUID);
        }
    }
}
