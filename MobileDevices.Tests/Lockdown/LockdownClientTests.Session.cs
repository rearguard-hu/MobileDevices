using Claunia.PropertyList;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MobileDevices.iOS;
using MobileDevices.iOS.DependencyInjection;
using MobileDevices.iOS.Lockdown;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MobileDevices.Tests.Lockdown
{
    /// <content>
    /// Tests the Session-related methods of the <see cref="LockdownClient"/> class.
    /// </content>
    public partial class LockdownClientTests
    {
        /// <summary>
        /// <see cref="LockdownClient.StartSessionAsync(PairingRecord, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task StartSessionAsync_ValidatesArguments_Async()
        {
            var client = new LockdownClient(Mock.Of<LockdownProtocol>(), NullLogger<LockdownClient>.Instance);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.StartSessionAsync(null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="LockdownClient.StartSessionAsync(PairingRecord, CancellationToken)"/> works.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task StartSessionAsync_Works_Async()
        {
            var protocol = new Mock<LockdownProtocol>(MockBehavior.Strict);

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Callback<LockdownMessage, CancellationToken>(
                (message, ct) =>
                {
                    var startSessionRequest = Assert.IsType<StartSessionRequest>(message);

                    Assert.Equal("buid", startSessionRequest.SystemBUID);
                    Assert.Equal("id", startSessionRequest.HostID);
                })
                .Returns(Task.CompletedTask);

            var dict = new NSDictionary();
            dict.Add("SessionID", "123");
            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(dict);

            protocol.Setup(p => p.ReadMessageAsync<StartSessionResponse>(default)).CallBase();
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask);

            await using (var client = new LockdownClient(protocol.Object, NullLogger<LockdownClient>.Instance))
            {
                var response = await client.StartSessionAsync(
                    new PairingRecord()
                    {
                        SystemBUID = "buid",
                        HostId = "id",
                    },
                    default).ConfigureAwait(false);

                Assert.Equal("123", response.SessionID);
                Assert.False(response.EnableSessionSSL);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.StartSessionAsync(PairingRecord, CancellationToken)"/> throws an error when
        /// the device returns an error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task StartSessionAsync_ThrowsOnError_Async()
        {
            var protocol = new Mock<LockdownProtocol>(MockBehavior.Strict);

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Callback<LockdownMessage, CancellationToken>(
                (message, ct) =>
                {
                    var startSessionRequest = Assert.IsType<StartSessionRequest>(message);

                    Assert.Equal("buid", startSessionRequest.SystemBUID);
                    Assert.Equal("id", startSessionRequest.HostID);
                })
                .Returns(Task.CompletedTask);

            var response = new NSDictionary();
            response.Add("Error", nameof(LockdownError.InvalidHostID));
            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(response);

            protocol.Setup(p => p.ReadMessageAsync<StartSessionResponse>(default)).CallBase();
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask);

            await using (var client = new LockdownClient(protocol.Object, NullLogger<LockdownClient>.Instance))
            {
                await Assert.ThrowsAsync<LockdownException>(
                    () => client.StartSessionAsync(
                    new PairingRecord()
                    {
                        SystemBUID = "buid",
                        HostId = "id",
                    },
                    default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.StartSessionAsync(PairingRecord, CancellationToken)"/> enables SSL when required.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task TryStartSessionAsync_EnablesSsl_Async()
        {
            var pairingRecord = new PairingRecord();
            var protocol = new Mock<LockdownProtocol>(MockBehavior.Strict);

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Returns(Task.CompletedTask);

            var response = new NSDictionary();
            response.Add("EnableSessionSSL", true);
            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(response);

            protocol.Setup(p => p.ReadMessageAsync<LockdownResponse>(default)).CallBase();

            protocol
                .Setup(p => p.EnableSslAsync(pairingRecord, default))
                .Returns(Task.CompletedTask)
                .Verifiable();

            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask);

            await using (var client = new LockdownClient(protocol.Object, NullLogger<LockdownClient>.Instance))
            {
                await client.StartSessionAsync(pairingRecord, default).ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="LockdownClient.StopSessionAsync(string, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task StopSessionAsync_ValidatesArguments_Async()
        {
            var client = new LockdownClient(Mock.Of<LockdownProtocol>(), NullLogger<LockdownClient>.Instance);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.StopSessionAsync(null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="LockdownClient.StopSessionAsync(string, CancellationToken)"/> throws when the device
        /// returns an error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task StopSessionAsync_ThrowsOnError_Async()
        {
            var protocol = new Mock<LockdownProtocol>(MockBehavior.Strict);

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Callback<LockdownMessage, CancellationToken>(
                (message, ct) =>
                {
                    var stopSessionRequest = Assert.IsType<StopSessionRequest>(message);

                    Assert.Equal("1234", stopSessionRequest.SessionID);
                })
                .Returns(Task.CompletedTask);

            var response = new NSDictionary();
            response.Add("Error", nameof(LockdownError.SessionInactive));
            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(response);

            protocol.Setup(p => p.ReadMessageAsync<LockdownResponse>(default)).CallBase();
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask);

            await using (var client = new LockdownClient(protocol.Object, NullLogger<LockdownClient>.Instance))
            {
                await Assert.ThrowsAsync<LockdownException>(
                    () => client.StopSessionAsync("1234", default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.StopSessionAsync(string, CancellationToken)"/> disables SSL encryption
        /// when SSL is enabled.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task StopSessionAsync_DisablesSsl_Async()
        {
            var protocol = new Mock<LockdownProtocol>(MockBehavior.Strict);

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Returns(Task.CompletedTask);

            protocol
                .Setup(p => p.SslEnabled)
                .Returns(true);

            protocol
                .Setup(p => p.DisableSslAsync(default))
                .Returns(Task.CompletedTask);

            var response = new NSDictionary();
            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(response);

            protocol.Setup(p => p.ReadMessageAsync<LockdownResponse>(default)).CallBase();
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask);

            await using (var client = new LockdownClient(protocol.Object, NullLogger<LockdownClient>.Instance))
            {
                await client.StopSessionAsync("1234", default).ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// Tests the <see cref="LockdownClient.StartSessionAsync(PairingRecord, CancellationToken)"/> method by creating a session and ensuring
        /// a protected property can be retreived.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact(Skip = "Requires a device")]
        public async Task StartSession_OverSsl_Works_Async()
        {
            var serviceProvider = new ServiceCollection()
                .AddAppleServices()
                .BuildServiceProvider();

            var deviceServiceProvider = new DeviceServiceProvider(serviceProvider);

            using (var scope = await deviceServiceProvider.CreateDeviceScopeAsync((string)null, default).ConfigureAwait(false))
            await using (var lockdown = await scope.StartServiceAsync<LockdownClient>(default).ConfigureAwait(false))
            {
                var context = scope.ServiceProvider.GetRequiredService<DeviceContext>();

                // Get a secured and an unsecured property
                var name = await lockdown.GetValueAsync("DeviceName", default).ConfigureAwait(false);
                await Assert.ThrowsAsync<LockdownException>(() => lockdown.GetValueAsync("SerialNumber", default)).ConfigureAwait(false);

                // Start the session
                var response = await lockdown.StartSessionAsync(context.PairingRecord, default).ConfigureAwait(false);

                // Try getting a secured and unsecured property
                name = await lockdown.GetValueAsync("DeviceName", default).ConfigureAwait(false);
                var developerStatus = await lockdown.GetValueAsync("SerialNumber", default).ConfigureAwait(false);

                // Stop the session
                await lockdown.StopSessionAsync(response.SessionID, default).ConfigureAwait(false);

                // Try getting a secured and unsecured proeprty.
                name = await lockdown.GetValueAsync("DeviceName", default).ConfigureAwait(false);
                await Assert.ThrowsAsync<LockdownException>(() => lockdown.GetValueAsync("SerialNumber", default)).ConfigureAwait(false);
            }
        }
    }
}
