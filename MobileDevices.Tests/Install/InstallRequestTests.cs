using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Claunia.PropertyList;
using MobileDevices.iOS.DiagnosticsRelay;
using MobileDevices.iOS.Install;
using Xunit;

namespace MobileDevices.Tests.Install
{
    /// <summary>
    /// Tests the <see cref="InstallRequest"/> class.
    /// </summary>
    public class InstallRequestTests
    {
        /// <summary>
        /// <see cref="InstallRequest.ToDictionary"/> works correctly.
        /// </summary>
        [Fact]
        public void ToDictionaryTest()
        {
            var clientOptions = new NSDictionary();
            var capabilities = new NSDictionary();

            var dict = new InstallRequest()
            {
                PackagePath = "test",
                Command = "a",
                ApplicationIdentifier = "1234.cn",
                Capabilities = capabilities,
                ClientOptions = clientOptions
            }.ToDictionary();

            Assert.Collection(
                dict,
                v =>
                {
                    Assert.Equal("Command", v.Key);
                    Assert.Equal("a", v.Value.ToObject());
                },
                v =>
                {
                    Assert.Equal("ClientOptions", v.Key);
                    Assert.Equal(clientOptions, v.Value);
                },
                v =>
                {
                    Assert.Equal("PackagePath", v.Key);
                    Assert.Equal("test", v.Value.ToObject());
                },
                v =>
                {
                    Assert.Equal("ApplicationIdentifier", v.Key);
                    Assert.Equal("1234.cn", v.Value.ToObject());
                },
                v =>
                {
                    Assert.Equal("Capabilities", v.Key);
                    Assert.Equal(capabilities, v.Value);
                });
        }
    }
}
