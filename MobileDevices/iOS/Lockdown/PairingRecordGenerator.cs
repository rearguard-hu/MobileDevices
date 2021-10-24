using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MobileDevices.iOS.Lockdown
{
    /// <summary>
    /// Generates a pairing record which can be used to pair a usbmuxd host with a device.
    /// </summary>
    public class PairingRecordGenerator
    {
        private const int KeySize = 2048;
        private static readonly X500DistinguishedName Name = new X500DistinguishedName(string.Empty);

        /// <summary>
        /// Generates a new pairing record.
        /// </summary>
        /// <param name="devicePublicKey">
        /// A <see cref="byte"/> array which represents the public key of the device with which to pair.
        /// </param>
        /// <param name="systemBuid">
        /// A <see cref="string"/> which uniquely identifies the host.
        /// </param>
        /// <returns>
        /// A new <see cref="PairingRecord"/> which can be used to pair the host with the device.
        /// </returns>
        public virtual PairingRecord Generate(byte[] devicePublicKey, string systemBuid)
        {
            return this.Generate(devicePublicKey, null, systemBuid);
        }

        /// <summary>
        /// Generates a new pairing record.
        /// </summary>
        /// <param name="devicePublicKey">
        /// A <see cref="byte"/> array which represents the public key of the device with which to pair.
        /// </param>
        /// <param name="udid">
        /// The UDID of the device. This will be included in the subjectName of the root certificate.
        /// This can be used to match the child certificates with the parent certificate based on the
        /// issuer name / subject name.
        /// This is not the default behavior of the canoncial implementation and should be needed on
        /// Windows for testing purposes only.
        /// </param>
        /// <param name="systemBuid">
        /// A <see cref="string"/> which uniquely identifies the host.
        /// </param>
        /// <returns>
        /// A new <see cref="PairingRecord"/> which can be used to pair the host with the device.
        /// </returns>
        public virtual PairingRecord Generate(byte[] devicePublicKey, string udid, string systemBuid)
        {
            if (devicePublicKey == null)
            {
                throw new ArgumentNullException(nameof(devicePublicKey));
            }

            if (systemBuid == null)
            {
                throw new ArgumentNullException(nameof(systemBuid));
            }

            RSA rootKeyPair = RSA.Create(KeySize);
            RSA hostKeyPair = RSA.Create(KeySize);

            DateTimeOffset notBefore = DateTimeOffset.Now;
            DateTimeOffset notAfter = notBefore.AddDays(365 * 10);

            X500DistinguishedName caName = Name;

            if (udid != null)
            {
                caName = new X500DistinguishedName($"CN=Root Certification Authority,OU={udid}");
            }

            var generator = new RSASha1Pkcs1SignatureGenerator(rootKeyPair);
            CertificateRequest rootRequest = new CertificateRequest(
                caName,
                rootKeyPair,
                HashAlgorithmName.SHA1,
                RSASignaturePadding.Pkcs1);
            rootRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(certificateAuthority: true, hasPathLengthConstraint: false, pathLengthConstraint: -1, critical: true));
            rootRequest.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(rootRequest.PublicKey, critical: false));

            var rootCert = rootRequest.Create(
                issuerName: caName,
                generator,
                notBefore: notBefore,
                notAfter: notAfter,
                serialNumber: new byte[] { 0 });

            CertificateRequest hostRequest = new CertificateRequest(
                Name,
                hostKeyPair,
                HashAlgorithmName.SHA1,
                RSASignaturePadding.Pkcs1);
            hostRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(certificateAuthority: false, hasPathLengthConstraint: false, pathLengthConstraint: -1, critical: true));
            hostRequest.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(hostRequest.PublicKey, critical: false));
            hostRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, critical: true));

            var hostCert = hostRequest.Create(
                issuerName: caName,
                generator,
                notBefore: notBefore,
                notAfter: notAfter,
                serialNumber: new byte[] { 0 });

            var device = RSA.Create();
            device.ImportFromPem(Encoding.UTF8.GetString(devicePublicKey));
            var deviceRequest = new CertificateRequest(
                Name,
                device,
                HashAlgorithmName.SHA1,
                RSASignaturePadding.Pkcs1);
            deviceRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(certificateAuthority: false, hasPathLengthConstraint: false, pathLengthConstraint: -1, critical: true));
            deviceRequest.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension("hash", critical: false));
            deviceRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, critical: true));

            var deviceCertificate = deviceRequest.Create(
                issuerName: caName,
                generator,
                notBefore: notBefore,
                notAfter: notAfter,
                serialNumber: new byte[] { 0 });

            var hostId = Guid.NewGuid();

            return new PairingRecord()
            {
                DeviceCertificate = deviceCertificate,
                HostPrivateKey = hostKeyPair,
                HostCertificate = hostCert,
                RootPrivateKey = rootKeyPair,
                RootCertificate = rootCert,
                SystemBUID = systemBuid,
                HostId = hostId.ToString("D").ToUpperInvariant(),
            };
        }

        private sealed class RSASha1Pkcs1SignatureGenerator : X509SignatureGenerator
        {
            private readonly X509SignatureGenerator generator;

            internal RSASha1Pkcs1SignatureGenerator(RSA rsa)
            {
                this.generator = X509SignatureGenerator.CreateForRSA(rsa, RSASignaturePadding.Pkcs1);
            }

            public override byte[] GetSignatureAlgorithmIdentifier(HashAlgorithmName hashAlgorithm)
            {
                Debug.Assert(hashAlgorithm == HashAlgorithmName.SHA1, "Only SHA1 hashes are supported");
                return new byte[] { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x05, 0x05, 0x00 };
            }

            public override byte[] SignData(byte[] data, HashAlgorithmName hashAlgorithm) =>
                this.generator.SignData(data, hashAlgorithm);

            protected override PublicKey BuildPublicKey() => this.generator.PublicKey;
        }
    }
}
