using Claunia.PropertyList;
using Microsoft.Extensions.Logging.Abstractions;
using MobileDevices.iOS.Lockdown;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MobileDevices.Tests.Lockdown
{
    /// <content>
    /// Tests the StartService-related methods on the <see cref="LockdownClient"/> class.
    /// </content>
    public partial class LockdownClientTests
    {
        /// <summary>
        /// <see cref="LockdownClient.StartServiceAsync(string, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task StartServiceAsync_ValidatesArguments_Async()
        {
            await using (var lockdown = new LockdownClient(Mock.Of<LockdownProtocol>(), NullLogger<LockdownClient>.Instance))
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => lockdown.StartServiceAsync(null, default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.StartServiceAsync(string, CancellationToken)"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task StartServiceAsync_Works_Async()
        {
            var protocol = new Mock<LockdownProtocol>(MockBehavior.Strict);

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Callback<LockdownMessage, CancellationToken>(
                (message, ct) =>
                {
                    var request = Assert.IsType<StartServiceRequest>(message);
                    Assert.Equal("test", request.Service);
                    Assert.Equal("StartService", request.Request);
                })
                .Returns(Task.CompletedTask);

            var dict = new NSDictionary();
            dict.Add("Port", 1234);

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(dict);

            protocol.Setup(p => p.ReadMessageAsync<StartServiceResponse>(default)).CallBase();
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask);

            await using (var lockdown = new LockdownClient(protocol.Object, NullLogger<LockdownClient>.Instance))
            {
                var result = await lockdown.StartServiceAsync("test", default).ConfigureAwait(false);

                Assert.False(result.EnableServiceSSL);
                Assert.Equal(1234, result.Port);
                Assert.Equal("test", result.ServiceName);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.StartServiceAsync(string, CancellationToken)"/> returns <see langword="null"/>
        /// when the server disconnects.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task StartServiceAsync_ServerDisconnects_ReturnsNull_Async()
        {
            var protocol = new Mock<LockdownProtocol>();

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Callback<LockdownMessage, CancellationToken>(
                (message, ct) =>
                {
                    var request = Assert.IsType<StartServiceRequest>(message);
                    Assert.Equal("test", request.Service);
                    Assert.Equal("StartService", request.Request);
                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync((NSDictionary)null);

            await using (var lockdown = new LockdownClient(protocol.Object, NullLogger<LockdownClient>.Instance))
            {
                var result = await lockdown.StartServiceAsync("test", default).ConfigureAwait(false);
                Assert.Null(result);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.StartServiceAsync(string, CancellationToken)"/> throws when an unexpected error
        /// occurs.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task StartServiceAsync_ThrowsOnError_Async()
        {
            var protocol = new Mock<LockdownProtocol>(MockBehavior.Strict);

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Callback<LockdownMessage, CancellationToken>(
                (message, ct) =>
                {
                    var request = Assert.IsType<StartServiceRequest>(message);
                    Assert.Equal("test", request.Service);
                    Assert.Equal("StartService", request.Request);
                })
                .Returns(Task.CompletedTask);

            NSDictionary dict = new NSDictionary();
            dict.Add("Error", nameof(LockdownError.SessionInactive));

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(dict);

            protocol.Setup(p => p.ReadMessageAsync<StartServiceResponse>(default)).CallBase();
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask);

            await using (var lockdown = new LockdownClient(protocol.Object, NullLogger<LockdownClient>.Instance))
            {
                await Assert.ThrowsAsync<LockdownException>(() => lockdown.StartServiceAsync("test", default)).ConfigureAwait(false);
            }
        }
    }
}
