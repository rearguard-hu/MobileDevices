using Claunia.PropertyList;
using MobileDevices.iOS.Muxer;
using Xunit;

namespace MobileDevices.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="RequestMessage"/> class.
    /// </summary>
    public class RequestMessageTests
    {
        /// <summary>
        /// Tests the <see cref="RequestMessage.ToPropertyList"/> method.
        /// </summary>
        [Fact]
        public void ToPropertyListTest()
        {
            var message = new RequestMessage();
            var dictionary = message.ToPropertyList();

            Assert.Equal(4, dictionary.Count);
            Assert.Equal(new NSString(message.BundleID), dictionary.Get("BundleID"));
            Assert.Equal(new NSString(message.ClientVersionString), dictionary.Get("ClientVersionString"));
            Assert.Equal(new NSString(message.MessageType.ToString()), dictionary.Get("MessageType"));
            Assert.Equal(new NSString(message.ProgName), dictionary.Get("ProgName"));
        }
    }
}
