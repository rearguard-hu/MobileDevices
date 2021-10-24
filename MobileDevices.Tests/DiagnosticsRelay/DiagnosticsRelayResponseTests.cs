using Claunia.PropertyList;
using MobileDevices.iOS.DiagnosticsRelay;
using System;
using Xunit;

namespace MobileDevices.Tests.DiagnosticsRelay
{
    /// <summary>
    /// Tests the <see cref="DiagnosticsRelayResponse"/> class.
    /// </summary>
    public class DiagnosticsRelayResponseTests
    {
        /// <summary>
        /// <see cref="DiagnosticsRelayResponse.Read(NSDictionary)"/> validates its arguments.
        /// </summary>
        [Fact]
        public void Read_ThrowsOnNull()
        {
            Assert.Throws<ArgumentNullException>(() => DiagnosticsRelayResponse.Read(null));
        }

        /// <summary>
        /// <see cref="DiagnosticsRelayResponse.Read(NSDictionary)"/> works correctly.
        /// </summary>
        [Fact]
        public void Read_Works()
        {
            var dict = new NSDictionary();
            dict.Add("Status", "Success");

            var response = DiagnosticsRelayResponse.Read(dict);
            Assert.Equal(DiagnosticsRelayStatus.Success, response.Status);
        }
    }
}
