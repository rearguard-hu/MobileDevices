using Claunia.PropertyList;
using Microsoft.Extensions.Logging.Abstractions;
using MobileDevices.iOS.NotificationProxy;
using MobileDevices.iOS.PropertyLists;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MobileDevices.Tests.NotificationProxy
{
    /// <summary>
    /// Tests the <see cref="NotificationProxyClient"/> class.
    /// </summary>
    public class NotificationProxyClientTests
    {
        /// <summary>
        /// The <see cref="NotificationProxyClient"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new NotificationProxyClient(null, NullLogger<NotificationProxyClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new NotificationProxyClient(Stream.Null, null));

            Assert.Throws<ArgumentNullException>(() => new NotificationProxyClient(null));
        }

        /// <summary>
        /// The <see cref="NotificationProxyClient.ObserveNotificationAsync(string, CancellationToken)"/>
        /// method correctly relays the command to the device.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ObserveNotificationAsync_Works_Async()
        {
            await using (MemoryStream stream = new MemoryStream())
            await using (NotificationProxyClient client = new NotificationProxyClient(stream, NullLogger<NotificationProxyClient>.Instance))
            {
                await client.ObserveNotificationAsync("com.apple.mobile.application_installed", default).ConfigureAwait(false);

                var data = stream.ToArray();

                Assert.Equal(File.ReadAllBytes("NotificationProxy/notificationproxy-host.bin"), data);
            }
        }

        /// <summary>
        /// The <see cref="NotificationProxyClient.ReadRelayNotificationAsync(CancellationToken)"/> correctly processes
        /// messages sent by the client.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReadRelayNotificationAsync_Works_Async()
        {
            await using (Stream stream = File.OpenRead("NotificationProxy/notificationproxy-device.bin"))
            await using (NotificationProxyClient client = new NotificationProxyClient(stream, NullLogger<NotificationProxyClient>.Instance))
            {
                var notification = await client.ReadRelayNotificationAsync(default).ConfigureAwait(false);
                Assert.Equal("com.apple.mobile.application_installed", notification);
            }
        }

        /// <summary>
        /// <see cref="NotificationProxyClient.ReadRelayNotificationAsync(CancellationToken)"/> returns <see langword="null"/>
        /// when the remote server closes the connection.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReadRelayNotificationAsync_ReturnsNull_Async()
        {
            var protocol = new Mock<PropertyListProtocol>();
            protocol.Setup(p => p.ReadMessageAsync(default)).ReturnsAsync((NSDictionary)null);

            await using (NotificationProxyClient client = new NotificationProxyClient(protocol.Object))
            {
                Assert.Null(await client.ReadRelayNotificationAsync(default).ConfigureAwait(false));
            }
        }

        /// <summary>
        /// <see cref="NotificationProxyClient.ReadRelayNotificationAsync(CancellationToken)"/> throws
        /// when the remote server returns an invalid message.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReadRelayNotificationAsync_ReturnsInvalidMessage_Throws_Async()
        {
            var dict = new NSDictionary();
            dict.Add("Command", "invalid");

            var protocol = new Mock<PropertyListProtocol>();
            protocol.Setup(p => p.ReadMessageAsync(default)).ReturnsAsync(dict);

            await using (NotificationProxyClient client = new NotificationProxyClient(protocol.Object))
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() => client.ReadRelayNotificationAsync(default)).ConfigureAwait(false);
            }
        }
    }
}
