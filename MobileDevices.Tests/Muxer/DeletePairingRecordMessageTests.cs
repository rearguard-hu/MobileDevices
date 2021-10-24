using MobileDevices.iOS.Muxer;
using Xunit;

namespace MobileDevices.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="DeletePairingRecordMessage"/> class.
    /// </summary>
    public class DeletePairingRecordMessageTests
    {
        /// <summary>
        /// <see cref="DeletePairingRecordMessage.ToPropertyList"/> works correctly.
        /// </summary>
        [Fact]
        public void ToPropertyList_Works()
        {
            var dict = new DeletePairingRecordMessage()
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
                    Assert.Equal("DeletePairRecord", k.Value.ToObject());
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
