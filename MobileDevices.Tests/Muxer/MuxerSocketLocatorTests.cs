using Microsoft.Extensions.Logging.Abstractions;
using MobileDevices.iOS.Muxer;
using Moq;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Xunit;

namespace MobileDevices.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="MuxerSocketLocator"/> class.
    /// </summary>
    public class MuxerSocketLocatorTests
    {
        /// <summary>
        /// The <see cref="MuxerSocketLocator"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new MuxerSocketLocator(null));
        }

        /// <summary>
        /// Tests the default values for the <see cref="MuxerSocketLocator.GetMuxerSocket"/> class.
        /// </summary>
        [Fact]
        public void GetMuxerSocket_DefaultValues()
        {
            var locator = new MuxerSocketLocator(NullLogger<MuxerSocketLocator>.Instance);

            (Socket socket, EndPoint endPoint) = locator.GetMuxerSocket();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.Equal(ProtocolType.Tcp, socket.ProtocolType);
                Assert.Equal(SocketType.Stream, socket.SocketType);

                var ipEndPoint = Assert.IsType<IPEndPoint>(endPoint);
                Assert.Equal(IPAddress.Loopback, ipEndPoint.Address);
                Assert.Equal(27015, ipEndPoint.Port);
            }
            else
            {
                if (!File.Exists("/var/run/usbmuxd"))
                {
                    Assert.Null(socket);
                    Assert.Null(endPoint);
                }
                else
                {
                    Assert.Equal(ProtocolType.Unspecified, socket.ProtocolType);
                    Assert.Equal(AddressFamily.Unix, socket.AddressFamily);
                    Assert.Equal(SocketType.Stream, socket.SocketType);

                    var unixEndPoint = Assert.IsType<UnixDomainSocketEndPoint>(endPoint);
                    Assert.Equal("/var/run/usbmuxd", unixEndPoint.ToString());
                }
            }
        }

        /// <summary>
        /// When the <c>USBMUXD_SOCKET_ADDRESS</c> variable is set, <see cref="MuxerSocketLocator.GetMuxerSocket"/>
        /// returns a Unix socket.
        /// </summary>
        [Fact]
        public void GetMuxerSocket_ForceUnix()
        {
            var locator = new Mock<MuxerSocketLocator>(NullLogger<MuxerSocketLocator>.Instance)
            {
                CallBase = true,
            };
            locator.Setup(l => l.GetSocketAddressEnvironmentVariable()).Returns("unix:/tmp/socket");

            (Socket socket, EndPoint endPoint) = locator.Object.GetMuxerSocket();

            Assert.Equal(ProtocolType.Unspecified, socket.ProtocolType);
            Assert.Equal(AddressFamily.Unix, socket.AddressFamily);
            Assert.Equal(SocketType.Stream, socket.SocketType);

            var unixEndPoint = Assert.IsType<UnixDomainSocketEndPoint>(endPoint);
            Assert.Equal("/tmp/socket", unixEndPoint.ToString());
        }

        /// <summary>
        /// When the <c>USBMUXD_SOCKET_ADDRESS</c> variable is set, <see cref="MuxerSocketLocator.GetMuxerSocket"/>
        /// returns a TCP socket.
        /// </summary>
        [Fact]
        public void GetMuxerSocket_ForceTcp()
        {
            var locator = new Mock<MuxerSocketLocator>(NullLogger<MuxerSocketLocator>.Instance)
            {
                CallBase = true,
            };
            locator.Setup(l => l.GetSocketAddressEnvironmentVariable()).Returns("10.0.0.1:1234");

            (Socket socket, EndPoint endPoint) = locator.Object.GetMuxerSocket();

            Assert.Equal(ProtocolType.Tcp, socket.ProtocolType);
            Assert.Equal(AddressFamily.InterNetworkV6, socket.AddressFamily);
            Assert.Equal(SocketType.Stream, socket.SocketType);

            var ipEndPoint = Assert.IsType<IPEndPoint>(endPoint);
            Assert.Equal(IPAddress.Parse("10.0.0.1"), ipEndPoint.Address);
            Assert.Equal(1234, ipEndPoint.Port);
        }
    }
}
