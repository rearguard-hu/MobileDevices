using MobileDevices.iOS.DeveloperProfiles;
using System;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace MobileDevices.Tests.DeveloperProfiles
{
    /// <summary>
    /// Tests the <see cref="Identity"/> class.
    /// </summary>
    public class IdentityTests
    {
        /// <summary>
        /// <see cref="Identity.FromX509Certificate(X509Certificate2)"/> throws a <see cref="ArgumentNullException"/> when
        /// passed <see langword="null"/> values.
        /// </summary>
        [Fact]
        public void FromX509Certificate_ThrowsOnNull()
        {
            Assert.Throws<ArgumentNullException>("certificate", () => Identity.FromX509Certificate(null));
        }

        /// <summary>
        /// <see cref="Identity.FromX509Certificate(X509Certificate2)"/> returns a valid <see cref="Identity"/> object
        /// when passed a <see cref="X509Certificate2"/> object.
        /// </summary>
        [Fact]
        public void FromX509Certificate_Works()
        {
            var certificate = new X509Certificate2("DeveloperProfiles/E7P4EE896K.cer");
            var identity = Identity.FromX509Certificate(certificate);

            Assert.Equal("iPhone Distribution: Felix Krause (439BBM9367)", identity.CommonName);
            Assert.False(identity.HasPrivateKey);
            Assert.Equal("Felix Krause", identity.Name);
            Assert.Equal(new DateTimeOffset(2016, 12, 3, 4, 2, 8, TimeSpan.FromHours(1)), identity.NotAfter);
            Assert.Equal("439BBM9367", identity.PersonID);
            Assert.Equal("EF4751CA452094E26A79D6F8BFDC08413CE6C90D", identity.Thumbprint);
            Assert.Equal("iPhone Distribution", identity.Type);

            Assert.Equal("Felix Krause (EF4751CA452094E26A79D6F8BFDC08413CE6C90D)", identity.ToString());
        }
    }
}
