using Claunia.PropertyList;
using MobileDevices.iOS.DeveloperProfiles;
using System;
using Xunit;

namespace MobileDevices.Tests.DeveloperProfiles
{
    /// <summary>
    /// Tests the <see cref="Entitlements"/> class.
    /// </summary>
    public class EntitlementsTests
    {
        /// <summary>
        /// <see cref="Entitlements.Read(NSDictionary)"/> throws when passed a <see langword="null"/> value.
        /// </summary>
        [Fact]
        public void Read_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Entitlements.Read(null));
        }
    }
}
