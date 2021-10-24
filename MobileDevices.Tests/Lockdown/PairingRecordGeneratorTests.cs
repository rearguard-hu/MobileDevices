using MobileDevices.iOS.Lockdown;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace MobileDevices.Tests.Lockdown
{
    /// <summary>
    /// Tests the <see cref="PairingRecordGenerator"/> class.
    /// </summary>
    public class PairingRecordGeneratorTests
    {
        /// <summary>
        /// The <see cref="PairingRecordGenerator.Generate(byte[], string)"/> method validates its arguments.
        /// </summary>
        [Fact]
        public void Generate_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("devicePublicKey", () => new PairingRecordGenerator().Generate(null, "test"));
            Assert.Throws<ArgumentNullException>("systemBuid", () => new PairingRecordGenerator().Generate(Array.Empty<byte>(), null));
        }

        /// <summary>
        /// The <see cref="PairingRecordGenerator.Generate(byte[], string, string)"/> method generates a valid
        /// pairing record.
        /// </summary>
        [Fact]
        public void Generate_GeneratesValidPair()
        {
            string udid = null;
            string caName = string.Empty;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                udid = "my-udid";
                caName = "CN=Root Certification Authority, OU=my-udid";
            }

            var key = Convert.FromBase64String(
                "LS0tLS1CRUdJTiBSU0EgUFVCTElDIEtFWS0tLS0tCk1JSUJDZ0tDQVFFQTBuNEJrdUM5" +
                "M0NJajJtUWxXSkQxWVVpaDgrTUsxbEdSS0ZlMStrQjE2aG5iOG5iL0tMazIKNHE4Y1Jq" +
                "aTArRVUxQlNDOGJxK2RjcHhSMlFvWnU1UWJiRUZBWjVxSXBqa082WUhWYjJwWVYwRXBP" +
                "cnVsZVQyLwpXSEI2YUZYdnU2RkRETEJjVktwUjVodlRBZk01UkxxTllkNGJmWXdVdnhE" +
                "MWI4QkJMK3VSVno1YnorK28zazBsCjB1QU02VjdqL3Fobkp6YTRKMDdiMFN6WlRwNUE3" +
                "T1VVUWV5UEE4UDVhMWpORklua2Joc1FTZ3RFejV2cHk4OWEKY3FMektMQ3ZocjJhY0dB" +
                "djRCT2lQLzh0TWdpa0lnVktGbWJ3bFU1dW5RbmNQYllmN2NoTmRmZnE3dEdRcVdFagpD" +
                "QUhidUx6M1VWOHpDdG5wOFNiUzRBWmFnNkR6dzMrakJRSURBUUFCCi0tLS0tRU5EIFJT" +
                "QSBQVUJMSUMgS0VZLS0tLS0=");

            var pairingRecord = new PairingRecordGenerator().Generate(key, udid, "system-buid");

            Assert.NotNull(pairingRecord.DeviceCertificate);
            Assert.NotNull(pairingRecord.HostCertificate);
            Assert.NotNull(pairingRecord.HostId);
            Assert.NotNull(pairingRecord.HostPrivateKey);
            Assert.NotNull(pairingRecord.RootCertificate);
            Assert.NotNull(pairingRecord.RootPrivateKey);

            // The host ID must be a well-formed, upper-case UUID
            Assert.True(Guid.TryParse(pairingRecord.HostId, out Guid hostId));
            Assert.Equal(hostId.ToString("D").ToUpperInvariant(), pairingRecord.HostId);

            var rootSubjectKeyIdentifier = pairingRecord.RootCertificate.Extensions.OfType<X509SubjectKeyIdentifierExtension>().Single();

            // Validate the root certificate
            Assert.Equal(caName, pairingRecord.RootCertificate.Subject);
            Assert.Equal(caName, pairingRecord.RootCertificate.Issuer);
            Assert.Equal("00", pairingRecord.RootCertificate.SerialNumber);
            Assert.Equal(2048, pairingRecord.RootCertificate.PublicKey.Key.KeySize);
            Assert.Equal("RSA", pairingRecord.RootCertificate.PublicKey.Key.SignatureAlgorithm);
            Assert.Equal("sha1RSA", pairingRecord.RootCertificate.SignatureAlgorithm.FriendlyName);
            Assert.Equal(3, pairingRecord.RootCertificate.Version);
            Assert.Equal(pairingRecord.RootCertificate.NotAfter.ToUniversalTime().AddDays(-10 * 365), pairingRecord.RootCertificate.NotBefore.ToUniversalTime());

            Assert.Collection(
                pairingRecord.RootCertificate.Extensions.Cast<X509Extension>(),
                (extension) =>
                {
                    var basicConstraintsExtension = Assert.IsType<X509BasicConstraintsExtension>(extension);
                    Assert.True(basicConstraintsExtension.CertificateAuthority);
                    Assert.True(basicConstraintsExtension.Critical);
                },
                (extension) =>
                {
                    var subjectKeyIdentifierExtension = Assert.IsType<X509SubjectKeyIdentifierExtension>(extension);
                    Assert.NotNull(subjectKeyIdentifierExtension.SubjectKeyIdentifier);
                    Assert.False(subjectKeyIdentifierExtension.Critical);
                });

            // Validate the host certificate
            Assert.Equal(string.Empty, pairingRecord.HostCertificate.Subject);
            Assert.Equal(caName, pairingRecord.HostCertificate.Issuer);
            Assert.Equal("00", pairingRecord.HostCertificate.SerialNumber);
            Assert.Equal(2048, pairingRecord.HostCertificate.PublicKey.Key.KeySize);
            Assert.Equal("RSA", pairingRecord.HostCertificate.PublicKey.Key.SignatureAlgorithm);
            Assert.Equal("sha1RSA", pairingRecord.RootCertificate.SignatureAlgorithm.FriendlyName);
            Assert.Equal(3, pairingRecord.HostCertificate.Version);
            Assert.Equal(
                pairingRecord.HostCertificate.NotAfter.ToUniversalTime().AddDays(-10 * 365),
                pairingRecord.HostCertificate.NotBefore.ToUniversalTime());

            Assert.True(IsTrusted(pairingRecord.RootCertificate, pairingRecord.HostCertificate));

            Assert.Collection(
                pairingRecord.HostCertificate.Extensions.Cast<X509Extension>(),
                (extension) =>
                {
                    var basicConstraintsExtension = Assert.IsType<X509BasicConstraintsExtension>(extension);
                    Assert.False(basicConstraintsExtension.CertificateAuthority);
                    Assert.True(basicConstraintsExtension.Critical);
                },
                (extension) =>
                {
                    var subjectKeyIdentifierExtension = Assert.IsType<X509SubjectKeyIdentifierExtension>(extension);
                    Assert.NotNull(subjectKeyIdentifierExtension.SubjectKeyIdentifier);
                    Assert.False(subjectKeyIdentifierExtension.Critical);
                },
                (extension) =>
                {
                    var keyUsageExtension = Assert.IsType<X509KeyUsageExtension>(extension);
                    Assert.Equal(X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, keyUsageExtension.KeyUsages);
                    Assert.True(keyUsageExtension.Critical);
                });

            // Validate the device certificate
            Assert.Equal(string.Empty, pairingRecord.DeviceCertificate.Subject);
            Assert.Equal(caName, pairingRecord.DeviceCertificate.Issuer);
            Assert.Equal("00", pairingRecord.DeviceCertificate.SerialNumber);
            Assert.Equal(2048, pairingRecord.DeviceCertificate.PublicKey.Key.KeySize);
            Assert.Equal("RSA", pairingRecord.DeviceCertificate.PublicKey.Key.SignatureAlgorithm);
            Assert.Equal("sha1RSA", pairingRecord.RootCertificate.SignatureAlgorithm.FriendlyName);
            Assert.Equal(3, pairingRecord.DeviceCertificate.Version);
            Assert.Equal(
                pairingRecord.DeviceCertificate.NotAfter.ToUniversalTime().AddDays(-10 * 365),
                pairingRecord.DeviceCertificate.NotBefore.ToUniversalTime());

            Assert.True(IsTrusted(pairingRecord.RootCertificate, pairingRecord.DeviceCertificate));

            Assert.Collection(
                pairingRecord.DeviceCertificate.Extensions.Cast<X509Extension>(),
                (extension) =>
                {
                    var basicConstraintsExtension = Assert.IsType<X509BasicConstraintsExtension>(extension);
                    Assert.False(basicConstraintsExtension.CertificateAuthority);
                    Assert.True(basicConstraintsExtension.Critical);
                },
                (extension) =>
                {
                    var subjectKeyIdentifierExtension = Assert.IsType<X509SubjectKeyIdentifierExtension>(extension);
                    Assert.NotNull(subjectKeyIdentifierExtension.SubjectKeyIdentifier);
                    Assert.False(subjectKeyIdentifierExtension.Critical);
                },
                (extension) =>
                {
                    var keyUsageExtension = Assert.IsType<X509KeyUsageExtension>(extension);
                    Assert.Equal(X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, keyUsageExtension.KeyUsages);
                    Assert.True(keyUsageExtension.Critical);
                });
        }

        private static bool IsTrusted(X509Certificate2 rootCertificate, X509Certificate2 certificate)
        {
            X509Chain chain = new X509Chain();
            chain.ChainPolicy.CustomTrustStore.Clear();
            chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;

            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

            chain.ChainPolicy.CustomTrustStore.Add(rootCertificate);

            return chain.Build(certificate);
        }
    }
}
