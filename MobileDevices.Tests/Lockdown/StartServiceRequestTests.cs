using MobileDevices.iOS.Lockdown;
using Xunit;

namespace MobileDevices.Tests.Lockdown
{
    /// <summary>
    /// Tests the <see cref="StartServiceRequest"/> class.
    /// </summary>
    public class StartServiceRequestTests
    {
        /// <summary>
        /// <see cref="StartServiceRequest.ToDictionary()"/> works correctly.
        /// </summary>
        [Fact]
        public void ToDictionary_Works()
        {
            var dict = new StartServiceRequest()
            {
                Service = "abc",
            }.ToDictionary();

            Assert.Collection(
                dict,
                k =>
                {
                    Assert.Equal("Label", k.Key);
                    Assert.Equal("MobileDevices", k.Value.ToObject());
                },
                k =>
                {
                    Assert.Equal("ProtocolVersion", k.Key);
                    Assert.Equal("2", k.Value.ToObject());
                },
                k =>
                {
                    Assert.Equal("Service", k.Key);
                    Assert.Equal("abc", k.Value.ToObject());
                });
        }
    }
}
