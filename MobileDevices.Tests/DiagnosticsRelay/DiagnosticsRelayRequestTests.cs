using MobileDevices.iOS.DiagnosticsRelay;
using Xunit;

namespace MobileDevices.Tests.DiagnosticsRelay
{
    /// <summary>
    /// Tests the <see cref="DiagnosticsRelayRequest"/> class.
    /// </summary>
    public class DiagnosticsRelayRequestTests
    {
        /// <summary>
        /// <see cref="DiagnosticsRelayRequest.ToDictionary"/> works correctly.
        /// </summary>
        [Fact]
        public void ToDictionary_Works()
        {
            var dict = new DiagnosticsRelayRequest()
            {
                Request = "a",
                WaitForDisconnect = false,
            }.ToDictionary();

            Assert.Collection(
                dict,
                v =>
                {
                    Assert.Equal("Request", v.Key);
                    Assert.Equal("a", v.Value.ToObject());
                },
                v =>
                {
                    Assert.Equal("WaitForDisconnect", v.Key);
                    Assert.Equal(false, v.Value.ToObject());
                });
        }
    }
}
