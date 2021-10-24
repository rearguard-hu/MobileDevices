using Claunia.PropertyList;
using MobileDevices.iOS.Muxer;
using System;
using System.Net;
using Xunit;

namespace MobileDevices.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="DeviceProperties"/> class.
    /// </summary>
    public class DevicePropertiesTests
    {
        /// <summary>
        /// The <see cref="DeviceProperties.Read(NSDictionary)"/> method throws an
        /// <see cref="ArgumentNullException"/> when passed a <see langword="null"/> value.
        /// </summary>
        [Fact]
        public void Read_NullArgument_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>("data", () => DeviceProperties.Read(null));
        }

        /// <summary>
        /// The <see cref="DeviceProperties.IPAddress"/> correctly parses the binary representation of the
        /// IP address.
        /// </summary>
        [Fact]
        public void IPAdress_ParsesCorrectly()
        {
            Assert.Equal(
                IPAddress.Loopback,
                new DeviceProperties() { NetworkAddress = new byte[] { 0x02, 0, 0, 0, /* AF_INET */ 0x7f, 0x00, 0x00, 0x01 }, }.IPAddress);

            Assert.Equal(
                IPAddress.IPv6Loopback,
                new DeviceProperties() { NetworkAddress = new byte[] { 0x1e, 0, 0, 0, /* AF_INET */ 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 }, }.IPAddress);

            Assert.Null(new DeviceProperties() { NetworkAddress = new byte[] { 0x01, 0, 0, 0, /* invalid */ 0x7f, 0x00, 0x00, 0x01 }, }.IPAddress);
            Assert.Null(new DeviceProperties() { NetworkAddress = new byte[] { }, }.IPAddress);
            Assert.Null(new DeviceProperties() { NetworkAddress = null, }.IPAddress);
        }
    }
}
