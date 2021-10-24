using MobileDevices.iOS.PropertyLists;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.NotificationProxy
{
    /// <summary>
    /// The <see cref="NotificationProxyClient"/> connects to the notification proxy on the device and observes notifications.
    /// </summary>
    public class NotificationProxyClient : IAsyncDisposable
    {
        /// <summary>
        /// Gets the name of the notification proxy service on the device.
        /// </summary>
        public const string ServiceName = "com.apple.mobile.notification_proxy";

        /// <summary>
        /// Gets the name of the insecure notification proxy service on the device.
        /// </summary>
        public const string InsecureServiceName = "com.apple.mobile.insecure_notification_proxy";

        private readonly PropertyListProtocol protocol;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationProxyClient"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents a connection to the notification proxy running on the device.
        /// </param>
        /// <param name="logger">
        /// A <see cref="ILogger"/> which can be used when logging.
        /// </param>
        public NotificationProxyClient(Stream stream, ILogger<NotificationProxyClient> logger)
        {
            this.protocol = new PropertyListProtocol(stream, ownsStream: true, logger: logger);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationProxyClient"/> class.
        /// </summary>
        /// <param name="protocol">
        /// A <see cref="PropertyListProtocol"/> which represents a connection to the notification proxy running on the device.
        /// </param>
        public NotificationProxyClient(PropertyListProtocol protocol)
        {
            this.protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationProxyClient"/> class. Intended for unit
        /// testing purposes only.
        /// </summary>
        protected NotificationProxyClient()
        {
        }

        /// <summary>
        /// Instructs the device to start observing a specific notification.
        /// </summary>
        /// <param name="name">
        /// The name of the notification the device should start observing.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public virtual async Task ObserveNotificationAsync(string name, CancellationToken cancellationToken)
        {
            var request = new NotificationProxyMessage()
            {
                Command = "ObserveNotification",
                Name = name,
            };

            await this.protocol.WriteMessageAsync(request.ToPropertyList(), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Reads a notification which has been relayed by the device.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public virtual async Task<string> ReadRelayNotificationAsync(CancellationToken cancellationToken)
        {
            var message = await this.protocol.ReadMessageAsync(cancellationToken).ConfigureAwait(false);

            if (message == null)
            {
                return null;
            }

            var notificationMessage = NotificationProxyMessage.Read(message);

            if (notificationMessage.Command != "RelayNotification")
            {
                throw new InvalidOperationException($"The device sent an unexpected '{notificationMessage.Command}' command.");
            }

            return notificationMessage.Name;
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            if (this.protocol == null)
            {
                return ValueTask.CompletedTask;
            }
            else
            {
                return this.protocol.DisposeAsync();
            }
        }
    }
}
