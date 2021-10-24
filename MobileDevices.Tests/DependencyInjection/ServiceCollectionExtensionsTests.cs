using Microsoft.Extensions.DependencyInjection;
using MobileDevices.iOS;
using MobileDevices.iOS.DependencyInjection;
using MobileDevices.iOS.Lockdown;
using MobileDevices.iOS.Muxer;
using MobileDevices.iOS.NotificationProxy;
using Xunit;

namespace MobileDevices.Tests.DependencyInjection
{
    /// <summary>
    /// Tests the <see cref="ServiceCollectionExtensions"/> class.
    /// </summary>
    public class ServiceCollectionExtensionsTests
    {
        /// <summary>
        /// The <see cref="ServiceCollectionExtensions.AddAppleServices(IServiceCollection)"/> method properly configures
        /// the service collection.
        /// </summary>
        [Fact]
        public void AddAppleServices_Works()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddAppleServices()
                .BuildServiceProvider();

            // Make sure we can get the most common types
            var muxer = serviceProvider.GetRequiredService<MuxerClient>();
            var lockdownFactory = serviceProvider.GetRequiredService<ClientFactory<LockdownClient>>();
            var notificationProxyClientFactory = serviceProvider.GetRequiredService<ClientFactory<NotificationProxyClient>>();
        }
    }
}
