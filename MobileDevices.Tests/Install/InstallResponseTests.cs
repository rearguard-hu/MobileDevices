using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Claunia.PropertyList;
using MobileDevices.iOS.Install;
using Xunit;

namespace MobileDevices.Tests.Install
{
    /// <summary>
    /// Tests the <see cref="InstallResponse"/> class.
    /// </summary>
    public class InstallResponseTests
    {
        /// <summary>
        /// <see cref="InstallResponse.FromDictionary(NSDictionary)"/> validates its arguments.
        /// </summary>
        [Fact]
        public void FromDictionaryThrowsOnNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => new InstallResponse().FromDictionary(null));
        }

        /// <summary>
        /// <see cref="InstallResponse.FromDictionary(NSDictionary)"/> works correctly.
        /// </summary>
        [Fact]
        public void FromDictionaryTest()
        {
            var dict = new NSDictionary { { "Status", "Complete" }, { "PercentComplete", 100 }, { "Error", "Error" }, { "ErrorDescription", "ErrorDescription" } };

            var response = new InstallResponse();
            response.FromDictionary(dict);

            Assert.Equal("Complete", response.Status);
            Assert.Equal(100, response.PercentComplete);
            Assert.Equal("Error", response.Error);
            Assert.Equal("ErrorDescription", response.ErrorDescription);
        }
    }
}
