using Claunia.PropertyList;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MobileDevices.iOS.Lockdown
{
    /// <summary>
    /// Pairing records are used to identify which hosts are trusted by a device. They are created by whenever a device is connected to
    /// a host, is unlocked and the user taps "Trust" on the device.
    /// </summary>
    public class PairingRecord
    {
        /// <summary>
        /// Gets or sets the MAC address of the WiFi card of the mobile device.
        /// </summary>
        public string WiFiMacAddress { get; set; }

        /// <summary>
        /// Gets or sets the escrow bag for this device. The escrow bag contains a backup of the encryption keys used to encrypt the file
        /// system on the iOS device, and allow iTunes and other applications to access the file system of the device, even when the device
        /// is locked.
        /// </summary>
        public byte[] EscrowBag { get; set; }

        /// <summary>
        /// Gets or sets an ID which uniquely identifies the host (computer). The value is saved in the
        /// <c>C:\ProgramData\Apple\Lockdown\SystemConfiguration.plist</c> property list.
        /// </summary>
        public string SystemBUID { get; set; }

        /// <summary>
        /// Gets or sets an ID which uniquely identifies the host (computer).
        /// </summary>
        public string HostId { get; set; }

        /// <summary>
        /// Gets or sets the root certificate used to sign certificates.
        /// </summary>
        public X509Certificate2 RootCertificate { get; set; }

        /// <summary>
        /// Gets or sets the computer certificate.
        /// </summary>
        public X509Certificate2 HostCertificate { get; set; }

        /// <summary>
        /// Gets or sets the device certificate.
        /// </summary>
        public X509Certificate2 DeviceCertificate { get; set; }

        /// <summary>
        /// Gets or sets the private key for the <see cref="HostCertificate"/>.
        /// </summary>
        public RSA HostPrivateKey { get; set; }

        /// <summary>
        /// Gets or sets the private key for the <see cref="RootCertificate"/>.
        /// </summary>
        public RSA RootPrivateKey { get; set; }

        /// <summary>
        /// Reads a <see cref="PairingRecord"/> from a <see cref="NSDictionary"/>.
        /// </summary>
        /// <param name="dict">
        /// The <see cref="NSDictionary"/> which represents the pairing record.
        /// </param>
        /// <returns>
        /// An equivalent <see cref="PairingRecord"/> object.
        /// </returns>
        public static PairingRecord Read(NSDictionary dict)
        {
            return new PairingRecord()
            {
                WiFiMacAddress = dict.GetString("WiFiMACAddress"),
                EscrowBag = dict.GetData("EscrowBag"),
                SystemBUID = dict.GetString("SystemBUID"),
                HostId = dict.GetString("HostID"),
                RootCertificate = new X509Certificate2(dict.GetData("RootCertificate")),
                HostCertificate = new X509Certificate2(dict.GetData("HostCertificate")),
                DeviceCertificate = new X509Certificate2(dict.GetData("DeviceCertificate")),
                HostPrivateKey = DeserializePrivateKey(dict.GetData("HostPrivateKey")),
                RootPrivateKey = DeserializePrivateKey(dict.GetData("RootPrivateKey")),
            };
        }

        /// <summary>
        /// Reads a <see cref="PairingRecord"/> from a serialized <see cref="NSDictionary"/>.
        /// </summary>
        /// <param name="data">
        /// A <see cref="byte"/> array which represents the serialized pairing record.
        /// </param>
        /// <returns>
        /// An equivalent <see cref="PairingRecord"/> object.
        /// </returns>
        public static PairingRecord Read(byte[] data)
        {
            return Read((NSDictionary)PropertyListParser.Parse(data));
        }

        /// <summary>
        /// Serializes this <see cref="PairingRecord"/> to a <see cref=" NSDictionary"/> object.
        /// </summary>
        /// <param name="includePrivateKeys">
        /// <see langword="true"/> if the root and host private keys should be serialized;
        /// otherwise, <see langword="false"/>.
        /// </param>
        /// <returns>
        /// A <see cref="NSDictionary"/> which represens this pairing record.
        /// </returns>
        public NSDictionary ToPropertyList(bool includePrivateKeys = true)
        {
            NSDictionary dict = new NSDictionary();
            dict.Add("DeviceCertificate", SerializeCertificate(this.DeviceCertificate));
            dict.Add("EscrowBag", this.EscrowBag);
            dict.Add("HostCertificate", SerializeCertificate(this.HostCertificate));
            dict.Add("HostID", this.HostId);

            if (includePrivateKeys)
            {
                dict.Add("HostPrivateKey", SerializePrivateKey(this.HostPrivateKey));
            }

            dict.Add("RootCertificate", SerializeCertificate(this.RootCertificate));

            if (includePrivateKeys)
            {
                dict.Add("RootPrivateKey", SerializePrivateKey(this.RootPrivateKey));
            }

            dict.Add("SystemBUID", this.SystemBUID);
            dict.Add("WiFiMACAddress", this.WiFiMacAddress);

            return dict;
        }

        /// <summary>
        /// Serializes this <see cref="PairingRecord"/> to a <see cref="byte"/> array.
        /// </summary>
        /// <param name="includePrivateKeys">
        /// <see langword="true"/> if the root and host private keys should be serialized;
        /// otherwise, <see langword="false"/>.
        /// </param>
        /// <returns>
        /// A <see cref="byte"/> array which represens this pairing record.
        /// </returns>
        public byte[] ToByteArray(bool includePrivateKeys = true)
        {
            var dict = this.ToPropertyList(includePrivateKeys);
            var xml = dict.ToXmlPropertyList();
            return Encoding.UTF8.GetBytes(xml);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"HostId: {this.HostId}, SystemBUID: {this.SystemBUID}, Host certificate: {this.HostCertificate?.Thumbprint} (expires: {this.HostCertificate?.NotAfter:u})";
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is PairingRecord other
                   && other.HostId != null
                   && other.SystemBUID != null
                   && other.HostId == this.HostId
                   && other.SystemBUID == this.SystemBUID;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(this.SystemBUID, this.HostId);
        }

        private static RSA DeserializePrivateKey(byte[] data)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(Encoding.UTF8.GetString(data));
            return rsa;
        }

        private static byte[] SerializeCertificate(X509Certificate certificate)
        {
            if (certificate == null)
            {
                return null;
            }

            var pemEncoded =
                PemEncoding.Write(
                    "CERTIFICATE",
                    certificate.Export(X509ContentType.Cert));

            // Append a \n character at the end
            var length = Encoding.UTF8.GetByteCount(pemEncoded) + 1;
            var bytes = new byte[length];
            Encoding.UTF8.GetBytes(pemEncoded, bytes);
            bytes[length - 1] = 0xA;

            return bytes;
        }

        private static byte[] SerializePrivateKey(RSA privateKey)
        {
            if (privateKey == null)
            {
                return null;
            }

            var pemEncoded =
                PemEncoding.Write(
                    "RSA PRIVATE KEY",
                    privateKey.ExportRSAPrivateKey());

            // Append a \n character at the end
            var length = Encoding.UTF8.GetByteCount(pemEncoded) + 1;
            var bytes = new byte[length];
            Encoding.UTF8.GetBytes(pemEncoded, bytes);
            bytes[length - 1] = 0xA;

            return bytes;
        }
    }
}
