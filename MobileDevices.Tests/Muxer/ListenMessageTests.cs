using MobileDevices.iOS.Muxer;
using System.IO;
using Xunit;

namespace MobileDevices.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="ListenMessage"/> class.
    /// </summary>
    public class ListenMessageTests
    {
        /// <summary>
        /// The <see cref="ListenMessage.ToPropertyList"/> method generates a valid property list representation of the object.
        /// </summary>
        [Fact]
        public void ToPropertyList_Works()
        {
            var message = new ListenMessage()
            {
                BundleID = "com.apple.iTunes",
                ClientVersionString = "usbmuxd-374.70",
                ConnType = 1,
                MessageType = MuxerMessageType.Listen,
                ProgName = "iTunes",
                kLibUSBMuxVersion = 3,
            };

            var dict = message.ToPropertyList();
            var xml = dict.ToXmlPropertyList();

            Assert.Equal(File.ReadAllText("Muxer/listen.xml"), xml, ignoreLineEndingDifferences: true);
        }
    }
}
