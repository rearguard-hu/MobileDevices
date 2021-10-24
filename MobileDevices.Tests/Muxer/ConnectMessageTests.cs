using MobileDevices.iOS.Muxer;
using Xunit;

namespace MobileDevices.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="ConnectMessage"/> class.
    /// </summary>
    public class ConnectMessageTests
    {
        /// <summary>
        /// THe <see cref="ConnectMessage.ToPropertyList"/> method returns the correct values.
        /// </summary>
        [Fact]
        public void ToPropertyListTest()
        {
            var message = new ConnectMessage()
            {
                DeviceID = 1,
                PortNumber = 2,
            };

            var dict = message.ToPropertyList();

            Assert.Collection(
                dict,
                (v) =>
                {
                    Assert.Equal("BundleID", v.Key);
                    Assert.Equal("MobileDevices", v.Value.ToObject());
                },
                (v) =>
                {
                    Assert.Equal("ClientVersionString", v.Key);
                },
                (v) =>
                {
                    Assert.Equal("DeviceID", v.Key);
                    Assert.Equal(1, v.Value.ToObject());
                },
                (v) =>
                {
                    Assert.Equal("MessageType", v.Key);
                    Assert.Equal("Connect", v.Value.ToObject());
                },
                (v) =>
                {
                    Assert.Equal("ProgName", v.Key);
                    Assert.Equal("MobileDevices", v.Value.ToObject());
                },
                (v) =>
                {
                    Assert.Equal("PortNumber", v.Key);
                    Assert.Equal(2, v.Value.ToObject());
                });
        }
    }
}
