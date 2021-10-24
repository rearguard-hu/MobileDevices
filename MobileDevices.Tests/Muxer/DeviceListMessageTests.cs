using Claunia.PropertyList;
using MobileDevices.iOS.Muxer;
using System;
using System.Net;
using Xunit;

namespace MobileDevices.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="DeviceListMessage"/> class.
    /// </summary>
    public class DeviceListMessageTests
    {
        /// <summary>
        /// The <see cref="DeviceListMessage.Read(NSDictionary)"/> method throws a <see cref="ArgumentNullException"/>
        /// when passed <see langword="null"/> values.
        /// </summary>
        [Fact]
        public void Read_WithNullArgument_Throws()
        {
            Assert.Throws<ArgumentNullException>("data", () => DeviceListMessage.Read(null));
        }

        /// <summary>
        /// The <see cref="DeviceListMessage.Read(NSDictionary)"/> method properly reads messages which reference
        /// USB-connected devices.
        /// </summary>
        [Fact]
        public void Read_WithUsbDevices_Works()
        {
            var data = (NSDictionary)PropertyListParser.Parse("Muxer/devicelist.xml");
            var message = DeviceListMessage.Read(data);

            Assert.Collection(
                message.DeviceList,
                device =>
                {
                    Assert.Equal(MuxerMessageType.Attached, device.MessageType);
                    Assert.Equal(5, device.DeviceID);

                    Assert.Equal(480000000, device.Properties.ConnectionSpeed);
                    Assert.Equal(MuxerConnectionType.USB, device.Properties.ConnectionType);
                    Assert.Equal(5, device.Properties.DeviceID);
                    Assert.Null(device.Properties.EscapedFullServiceName);
                    Assert.Equal(0, device.Properties.InterfaceIndex);
                    Assert.Null(device.Properties.IPAddress);
                    Assert.Equal(65542, device.Properties.LocationID);
                    Assert.Null(device.Properties.NetworkAddress);
                    Assert.Equal(0x12A8, device.Properties.ProductID);
                    Assert.Equal("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", device.Properties.SerialNumber);
                    Assert.Null(device.Properties.UDID);
                    Assert.Null(device.Properties.USBSerialNumber);
                },
                device =>
                {
                    Assert.Equal(MuxerMessageType.Attached, device.MessageType);
                    Assert.Equal(6, device.DeviceID);

                    Assert.Equal(480000000, device.Properties.ConnectionSpeed);
                    Assert.Equal(MuxerConnectionType.USB, device.Properties.ConnectionType);
                    Assert.Equal(6, device.Properties.DeviceID);
                    Assert.Null(device.Properties.EscapedFullServiceName);
                    Assert.Equal(0, device.Properties.InterfaceIndex);
                    Assert.Null(device.Properties.IPAddress);
                    Assert.Equal(65543, device.Properties.LocationID);
                    Assert.Null(device.Properties.NetworkAddress);
                    Assert.Equal(0x12A8, device.Properties.ProductID);
                    Assert.Equal("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb", device.Properties.SerialNumber);
                    Assert.Null(device.Properties.UDID);
                    Assert.Null(device.Properties.USBSerialNumber);
                });
        }

        /// <summary>
        /// The <see cref="DeviceListMessage.Read(NSDictionary)"/> method properly reads messages which reference
        /// WiFi-connected devices.
        /// </summary>
        [Fact]
        public void Read_WithWifiDevices_Works()
        {
            var data = (NSDictionary)PropertyListParser.Parse("Muxer/devicelist-wifi.xml");
            var message = DeviceListMessage.Read(data);

            Assert.Collection(
                message.DeviceList,
                device =>
                {
                    Assert.Equal(MuxerMessageType.Attached, device.MessageType);
                    Assert.Equal(2, device.DeviceID);

                    Assert.Equal(0, device.Properties.ConnectionSpeed);
                    Assert.Equal(MuxerConnectionType.Network, device.Properties.ConnectionType);
                    Assert.Equal(2, device.Properties.DeviceID);
                    Assert.Equal("aa:aa:aa:aa:aa:aa@aaaa::aaaa:aaa:aaaa:aaaa._apple-mobdev2._tcp.local.", device.Properties.EscapedFullServiceName);
                    Assert.Equal(39, device.Properties.InterfaceIndex);
                    Assert.Equal(IPAddress.Parse("192.168.10.239"), device.Properties.IPAddress);
                    Assert.Equal(0, device.Properties.LocationID);
                    Assert.NotNull(device.Properties.NetworkAddress);
                    Assert.Equal(0, device.Properties.ProductID);
                    Assert.Equal("cccccccccccccccccccccccccccccccccccccccc", device.Properties.SerialNumber);
                    Assert.Null(device.Properties.UDID);
                    Assert.Null(device.Properties.USBSerialNumber);
                });
        }
    }
}
