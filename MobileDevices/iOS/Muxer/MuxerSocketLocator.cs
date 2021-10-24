using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Provides methods for connecting to the Apple USB Device Multiplexor Daemon.
    /// </summary>
    public class MuxerSocketLocator
    {
        /// <summary>
        /// The name of the USBMUXD_SOCKET_ADDRESS environment variable, which can be used to
        /// override the socket address.
        /// </summary>
        public const string SocketAddressEnvironmentVariable = "USBMUXD_SOCKET_ADDRESS";

        /// <summary>
        /// The port at which the muxer is listening on Windows.
        /// </summary>
        public const int DefaultMuxerPort = 27015;

        /// <summary>
        /// The default IP address at which the muxer is listening on Windows.
        /// </summary>
        public const long DefaultMuxerHost = 0x0100007f;

        /// <summary>
        /// The socket at which the muxer is listening on Linux and macOS.
        /// </summary>
        public const string MuxerSocket = "/var/run/usbmuxd";

        private readonly Lazy<bool> isWindowsSubsystemForLinux;
        private readonly ILogger<MuxerSocketLocator> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MuxerSocketLocator"/> class.
        /// </summary>
        /// <param name="logger">
        /// A logger to use when logging.
        /// </param>
        public MuxerSocketLocator(ILogger<MuxerSocketLocator> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.isWindowsSubsystemForLinux = new Lazy<bool>(this.GetIsWindowsSubsystemForLinux);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MuxerSocketLocator"/> class.
        /// </summary>
        /// <remarks>
        /// Intended for mocking purposes only.
        /// </remarks>
#nullable disable
        protected MuxerSocketLocator()
        {
        }
#nullable restore

        /// <summary>
        /// Gets the logger to use when logging.
        /// </summary>
        protected ILogger Logger => this.logger;

        /// <summary>
        /// Gets a <see cref="Stream"/> which represents a connection to the usbmuxd daemon.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public virtual async Task<Stream> ConnectToMuxerAsync(CancellationToken cancellationToken)
        {
            var (socket, endPoint) = this.GetMuxerSocket();

            if (socket == null)
            {
                this.logger.LogWarning($"Could not connect to the muxer socket.");
                return null;
            }

            await socket.ConnectAsync(endPoint, cancellationToken).ConfigureAwait(false);

            return new NetworkStream(socket, ownsSocket: true);
        }

        /// <summary>
        /// Gets the <see cref="Socket"/> and <see cref="EndPoint"/> which can be used to connect to the USB Multiplexor daemon.
        /// </summary>
        /// <returns>
        /// A <see cref="MuxerProtocol"/> which represents the connection to <c>usbmuxd</c>, or <see langword="null"/> if
        /// usbmuxd is not running.
        /// </returns>
        public virtual (Socket, EndPoint) GetMuxerSocket()
        {
            Socket socket;
            EndPoint endPoint;

            var socketAddress = this.GetSocketAddressEnvironmentVariable();

            if (socketAddress != null && socketAddress.StartsWith("UNIX:", StringComparison.OrdinalIgnoreCase))
            {
                string socketName = socketAddress.Substring(5);

                this.logger.LogDebug("Connecting to Unix socket {socketName}, set using the " + SocketAddressEnvironmentVariable + " environment variable.", socketName);
                socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                endPoint = new UnixDomainSocketEndPoint(socketName);
            }
            else if (socketAddress != null)
            {
                this.logger.LogDebug("Connecting to TCP socket {address}, set using the " + SocketAddressEnvironmentVariable + " environment variable.", socketAddress);
                var separator = socketAddress.IndexOf(':');
                var host = IPAddress.Parse(socketAddress.Substring(0, separator));
                var port = int.Parse(socketAddress.Substring(separator + 1));

                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                endPoint = new IPEndPoint(host, port);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                || this.isWindowsSubsystemForLinux.Value)
            {
                this.logger.LogDebug("Connecting to the default Windows socket.");
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                endPoint = new IPEndPoint(DefaultMuxerHost, DefaultMuxerPort);
            }
            else
            {
                if (!File.Exists(MuxerSocket))
                {
                    this.logger.LogWarning("The file " + MuxerSocket + " does not exist.");
                    return (null, null);
                }

                this.logger.LogDebug("Connecting to the default Unix socket.");
                socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                endPoint = new UnixDomainSocketEndPoint(MuxerSocket);
            }

            this.logger.LogInformation("Connecting to {endPoint}", endPoint);
            return (socket, endPoint);
        }

        /// <summary>
        /// Gets the value of the <c>USBMUXD_SOCKET_ADDRESS</c> environment variable.
        /// </summary>
        /// <returns>
        /// The value of the <c>USBMUXD_SOCKET_ADDRESS</c> environment variable.
        /// </returns>
        public virtual string GetSocketAddressEnvironmentVariable()
        {
            return Environment.GetEnvironmentVariable(SocketAddressEnvironmentVariable);
        }

        /// <summary>
        /// Gets a value indicating whether the host is running Windows Subsystem for Linux.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> when the host is running Windows Subsystem for Linux;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public virtual bool GetIsWindowsSubsystemForLinux()
        {
            // https://github.com/microsoft/WSL/issues/423
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                || !File.Exists("/proc/sys/kernel/osrelease"))
            {
                return false;
            }

            var osRelease = File.ReadAllText("/proc/sys/kernel/osrelease");

            return osRelease.Contains("microsoft", StringComparison.OrdinalIgnoreCase);
        }
    }
}
