using MobileDevices.iOS.Lockdown;
using Xunit;

namespace MobileDevices.Tests.Lockdown
{
    /// <summary>
    /// Tests the <see cref="GetValueRequest"/> class.
    /// </summary>
    public class GetValueRequestTests
    {
        /// <summary>
        /// <see cref="GetValueRequest.ToDictionary"/> works in a scenario where no domain is set.
        /// </summary>
        [Fact]
        public void ToDictionary_NoDomain()
        {
            var dict = new GetValueRequest()
            {
                Key = "test",
            }.ToDictionary();

            Assert.Collection(
                dict,
                v =>
                {
                    Assert.Equal("Label", v.Key);
                    Assert.Equal("MobileDevices", v.Value.ToObject());
                },
                v =>
                {
                    Assert.Equal("ProtocolVersion", v.Key);
                    Assert.Equal("2", v.Value.ToObject());
                },
                v =>
                {
                    Assert.Equal("Key", v.Key);
                    Assert.Equal("test", v.Value.ToObject());
                });
        }

        /// <summary>
        /// <see cref="GetValueRequest.ToDictionary"/> works in a scenario where a domain is set.
        /// </summary>
        [Fact]
        public void ToDictionary_WithDomain()
        {
            var dict = new GetValueRequest()
            {
                Domain = "foo",
                Key = "test",
            }.ToDictionary();

            Assert.Collection(
                dict,
                v =>
                {
                    Assert.Equal("Label", v.Key);
                    Assert.Equal("MobileDevices", v.Value.ToObject());
                },
                v =>
                {
                    Assert.Equal("ProtocolVersion", v.Key);
                    Assert.Equal("2", v.Value.ToObject());
                },
                v =>
                {
                    Assert.Equal("Domain", v.Key);
                    Assert.Equal("foo", v.Value.ToObject());
                },
                v =>
                {
                    Assert.Equal("Key", v.Key);
                    Assert.Equal("test", v.Value.ToObject());
                });
        }
    }
}
