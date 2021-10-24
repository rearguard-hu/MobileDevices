using Microsoft.Extensions.DependencyInjection;
using MobileDevices.iOS;
using MobileDevices.iOS.DependencyInjection;
using MobileDevices.iOS.Lockdown;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MobileDevices.Tests.DependencyInjection
{
    /// <summary>
    /// Tests the <see cref="DeviceServiceScope"/> class.
    /// </summary>
    public class DeviceServiceScopeTests
    {
        /// <summary>
        /// The <see cref="DeviceServiceScope"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new DeviceServiceScope(null));
        }

        /// <summary>
        /// The <see cref="DeviceServiceScope"/> constructor throws an exception if the service scope
        /// does not contain at least a <see cref="DeviceContext"/> value.
        /// </summary>
        [Fact]
        public void Constructor_RequiresDeviceContext()
        {
            using (var provider = new ServiceCollection().BuildServiceProvider())
            using (var scope = provider.CreateScope())
            {
                Assert.Throws<InvalidOperationException>(() => new DeviceServiceScope(scope));
            }
        }

        /// <summary>
        /// The <see cref="DeviceServiceScope"/> constructor correctly initializes the properties.
        /// </summary>
        [Fact]
        public void Constructor_InitializesProperties()
        {
            using (var provider = new ServiceCollection()
                .AddScoped<DeviceContext>()
                .BuildServiceProvider())
            using (var scope = provider.CreateScope())

            using (var deviceScope = new DeviceServiceScope(scope))
            {
                Assert.Equal(scope.ServiceProvider, deviceScope.ServiceProvider);
                Assert.NotNull(deviceScope.Context);
            }
        }

        /// <summary>
        /// <see cref="DeviceServiceScope.StartServiceAsync{T}(CancellationToken)"/> returns a service client.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task StartServiceAsync_Works_Async()
        {
            var lockdown = new Mock<LockdownClient>(MockBehavior.Strict);

            var lockdownFactory = new Mock<ClientFactory<LockdownClient>>(MockBehavior.Strict);
            lockdownFactory.Setup(l => l.CreateAsync(default)).ReturnsAsync(lockdown.Object);

            using (var provider = new ServiceCollection()
                .AddScoped<DeviceContext>()
                .AddScoped((sp) => lockdownFactory.Object)
                .BuildServiceProvider())
            using (var scope = provider.CreateScope())
            using (var deviceScope = new DeviceServiceScope(scope))
            {
                Assert.Equal(lockdown.Object, await deviceScope.StartServiceAsync<LockdownClient>(default).ConfigureAwait(false));
            }
        }
    }
}
