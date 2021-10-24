using Claunia.PropertyList;
using MobileDevices.iOS.Lockdown;
using System;
using System.IO;
using Xunit;

namespace MobileDevices.Tests.Lockdown
{
    /// <summary>
    /// Tests the <see cref="PairingRecord"/> class.
    /// </summary>
    public class PairingRecordTests
    {
        /// <summary>
        /// <see cref="PairingRecord.Read(NSDictionary)"/> correctly parses the property list data.
        /// </summary>
        [Fact]
        public void Read_Works()
        {
            var dict = (NSDictionary)PropertyListParser.Parse("Lockdown/0123456789abcdef0123456789abcdef01234567.plist");
            var pairingRecord = PairingRecord.Read(dict);

            Assert.NotNull(pairingRecord.DeviceCertificate);
            Assert.Equal("879D15EC44D67A89BF0AC3C0311DA035FDD56D0E", pairingRecord.DeviceCertificate.Thumbprint);
            Assert.Equal("MDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDA=", Convert.ToBase64String(pairingRecord.EscrowBag));
            Assert.NotNull(pairingRecord.HostCertificate);
            Assert.Equal("EE63391AA1FBA937E2784CC7DAAA9C22BA223B54", pairingRecord.HostCertificate.Thumbprint);
            Assert.Equal("01234567-012345678901234567", pairingRecord.HostId);
            Assert.NotNull(pairingRecord.HostPrivateKey);
            Assert.Equal(2048, pairingRecord.HostPrivateKey.KeySize);
            Assert.NotNull(pairingRecord.RootCertificate);
            Assert.Equal("DB0F6BAA694FA99879281A388D170CCE1412AC92", pairingRecord.RootCertificate.Thumbprint);
            Assert.NotNull(pairingRecord.RootPrivateKey);
            Assert.Equal(2048, pairingRecord.RootPrivateKey.KeySize);
            Assert.Equal("01234567890123456789012345", pairingRecord.SystemBUID);
            Assert.Equal("01:23:45:67:89:ab", pairingRecord.WiFiMacAddress);
        }

        /// <summary>
        /// <see cref="PairingRecord.ToPropertyList(bool)"/> correctly serializes the pairing record.
        /// </summary>
        [Fact]
        public void ToPropertyList_Works()
        {
            var raw = File.ReadAllText("Lockdown/0123456789abcdef0123456789abcdef01234567.plist");
            var dict = (NSDictionary)XmlPropertyListParser.ParseString(raw);

            var pairingRecord = PairingRecord.Read(dict);
            var serializedDict = pairingRecord.ToPropertyList();

            Assert.Equal(dict.Keys, serializedDict.Keys);

            foreach (var key in dict.Keys)
            {
                Assert.Equal(dict[key], serializedDict[key]);
            }

            var xml = serializedDict.ToXmlPropertyList();

            Assert.Equal(raw, xml, ignoreLineEndingDifferences: true);
        }

        /// <summary>
        /// <see cref="PairingRecord.ToPropertyList(bool)"/> correctly serializes the pairing record when the private
        /// keys are not available.
        /// </summary>
        [Fact]
        public void ToPropertyList_NoPrivateKeys_Works()
        {
            var raw = File.ReadAllText("Lockdown/0123456789abcdef0123456789abcdef01234567.plist");
            var dict = (NSDictionary)XmlPropertyListParser.ParseString(raw);

            var pairingRecord = PairingRecord.Read(dict);
            pairingRecord.HostPrivateKey = null;
            pairingRecord.RootPrivateKey = null;
            var serializedDict = pairingRecord.ToPropertyList();

            Assert.Equal(dict.Keys.Count - 2, serializedDict.Keys.Count);
        }

        /// <summary>
        /// <see cref="PairingRecord.ToPropertyList(bool)"/> correctly serializes the pairing record.
        /// </summary>
        [Fact]
        public void ToPropertyList_ExcludesPrivateKeysIfRequired()
        {
            var raw = File.ReadAllText("Lockdown/0123456789abcdef0123456789abcdef01234567.plist");
            var dict = (NSDictionary)XmlPropertyListParser.ParseString(raw);

            var pairingRecord = PairingRecord.Read(dict);
            var serializedDict = pairingRecord.ToPropertyList(includePrivateKeys: false);

            Assert.False(serializedDict.ContainsKey("HostPrivateKey"));
            Assert.False(serializedDict.ContainsKey("RootPrivateKey"));
        }

        /// <summary>
        /// <see cref="PairingRecord.Equals(object)"/> and <see cref="PairingRecord.GetHashCode"/> work correctly.
        /// </summary>
        /// <param name="systemBuid">
        /// The <see cref="PairingRecord.SystemBUID"/> for the first pairing record.
        /// </param>
        /// <param name="hostId">
        /// The <see cref="PairingRecord.HostId"/> for the first pairing record.
        /// </param>
        /// <param name="otherSystemBuid">
        /// The <see cref="PairingRecord.SystemBUID"/> for the second pairing record.
        /// </param>
        /// <param name="otherHostId">
        /// The <see cref="PairingRecord.HostId"/> for the second pairing record.
        /// </param>
        /// <param name="equal">
        /// A value indicating whether the two values are considered equal.
        /// </param>
        /// <param name="sameHashCode">
        /// A value indicating whether the two values have the same hashcode.
        /// </param>
        [Theory]
        [InlineData(null, null, null, null, false, true)]
        [InlineData(null, "a", null, "a", false, true)]
        [InlineData("a", null, "a", null, false, true)]
        [InlineData("a", "b", "a", "c", false, false)]
        [InlineData("a", "b", "c", "a", false, false)]
        [InlineData("a", "b", "a", "b", true, true)]
        public void Equals_Works(string systemBuid, string hostId, string otherSystemBuid, string otherHostId, bool equal, bool sameHashCode)
        {
            var record = new PairingRecord() { SystemBUID = systemBuid, HostId = hostId };
            var other = new PairingRecord() { SystemBUID = otherSystemBuid, HostId = otherHostId };

            Assert.Equal(equal, record.Equals(other));
            Assert.Equal(equal, other.Equals(record));
            Assert.Equal(equal, Equals(other, record));
            Assert.Equal(equal, Equals(record, other));
            Assert.Equal(equal, Equals(other, record));
            Assert.Equal(equal, Equals(record, other));

            Assert.Equal(sameHashCode, record.GetHashCode() == other.GetHashCode());
        }

        /// <summary>
        /// <see cref="PairingRecord.ToString"/> works for a pairing record which is empty.
        /// </summary>
        [Fact]
        public void ToString_EmptyPairingRecord()
        {
            var record = new PairingRecord();
            Assert.Equal("HostId: , SystemBUID: , Host certificate:  (expires: )", record.ToString());
        }

        /// <summary>
        /// <see cref="PairingRecord.ToString"/> works for a regular pairing record.
        /// </summary>
        [Fact]
        public void ToString_ValidPairingRecord()
        {
            var raw = File.ReadAllText("Lockdown/0123456789abcdef0123456789abcdef01234567.plist");
            var dict = (NSDictionary)XmlPropertyListParser.ParseString(raw);

            var pairingRecord = PairingRecord.Read(dict);

            Assert.Equal("HostId: 01234567-012345678901234567, SystemBUID: 01234567890123456789012345, Host certificate: EE63391AA1FBA937E2784CC7DAAA9C22BA223B54 (expires: 2026-11-28 11:20:17Z)", pairingRecord.ToString());
        }
    }
}
