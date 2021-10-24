using MobileDevices.iOS.Muxer;
using Xunit;

namespace MobileDevices.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="ReadPairingRecordMessage"/> class.
    /// </summary>
    public class ReadPairingRecordMessageTests
    {
        /// <summary>
        /// <see cref="ReadPairingRecordMessage.ToPropertyList"/> works correctly.
        /// </summary>
        [Fact]
        public void ToPropertyList_Works()
        {
            var dict = new ReadPairingRecordMessage()
            {
                PairRecordID = "abc",
            }.ToPropertyList();

            Assert.Collection(
                dict,
                k =>
                {
                    Assert.Equal("BundleID", k.Key);
                    Assert.Equal("MobileDevices", k.Value.ToObject());
                },
                k =>
                {
                    Assert.Equal("ClientVersionString", k.Key);
                    Assert.Equal("0.3.0", k.Value.ToObject());
                },
                k =>
                {
                    Assert.Equal("MessageType", k.Key);
                    Assert.Equal("ReadPairRecord", k.Value.ToObject());
                },
                k =>
                {
                    Assert.Equal("ProgName", k.Key);
                    Assert.Equal("MobileDevices", k.Value.ToObject());
                },
                k =>
                {
                    Assert.Equal("PairRecordID", k.Key);
                    Assert.Equal("abc", k.Value.ToObject());
                });
        }
    }
}
