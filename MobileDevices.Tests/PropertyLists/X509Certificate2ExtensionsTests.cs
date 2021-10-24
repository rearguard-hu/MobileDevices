using MobileDevices.iOS.Lockdown;
using MobileDevices.iOS.Services;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Xunit;

namespace MobileDevices.Tests.PropertyLists
{
    /// <summary>
    /// Tests the <see cref="X509Certificate2Extensions"/> class.
    /// </summary>
    public class X509Certificate2ExtensionsTests
    {
        /// <summary>
        /// <see cref="X509Certificate2Extensions.CopyWithPrivateKeyForSsl(X509Certificate2, RSA)"/> throws an exception
        /// when passed <see langword="null"/> values.
        /// </summary>
        [Fact]
        public void CopyWithPrivateKeyForSsl_ThrowsOnNull()
        {
            var pairingRecord = PairingRecord.Read(File.ReadAllBytes("Lockdown/0123456789abcdef0123456789abcdef01234567.plist"));

            Assert.Throws<ArgumentNullException>(() => pairingRecord.HostCertificate.CopyWithPrivateKeyForSsl(null));
            Assert.Throws<ArgumentNullException>(() => X509Certificate2Extensions.CopyWithPrivateKeyForSsl(null, pairingRecord.HostPrivateKey));
        }

        /// <summary>
        /// <see cref="X509Certificate2Extensions.CopyWithPrivateKeyForSsl(X509Certificate2, RSA)"/> returns a new certificate
        /// with a private key. On Windows, the private key is not ephemeral.
        /// </summary>
        [Fact]
        public void CopyWithPrivateKeyForSsl_Works()
        {
            var pairingRecord = PairingRecord.Read(File.ReadAllBytes("Lockdown/0123456789abcdef0123456789abcdef01234567.plist"));

            using (var certificate = pairingRecord.HostCertificate.CopyWithPrivateKeyForSsl(pairingRecord.HostPrivateKey))
            {
                Assert.NotNull(certificate.PrivateKey);

                // On Windows, because of a SCHANNEL bug (https://github.com/dotnet/runtime/issues/23749#issuecomment-485947319), private keys
                // for certificates cannot be marked as ephemeral because the in-memory TLS client certificate private key is not marshaled
                // between SCHANNEL and LSASS. Make sure the private key is not ephemeral.
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var key = Assert.IsType<RSACng>(certificate.PrivateKey);
                    Assert.False(key.Key.IsEphemeral);
                }
            }
        }
    }
}
