using Claunia.PropertyList;
using Microsoft.Extensions.Logging.Abstractions;
using MobileDevices.iOS.DiagnosticsRelay;
using MobileDevices.iOS.PropertyLists;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MobileDevices.Tests.DiagnosticsRelay
{
    /// <summary>
    /// Tests the <see cref="DiagnosticsRelayClient"/> class.
    /// </summary>
    public class DiagnosticsRelayClientTests
    {
        /// <summary>
        /// The <see cref="DiagnosticsRelayClient"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new DiagnosticsRelayClient(null, NullLogger<DiagnosticsRelayClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new DiagnosticsRelayClient(Stream.Null, null));

            Assert.Throws<ArgumentNullException>(() => new DiagnosticsRelayClient(null));
        }

        /// <summary>
        /// Tests the <see cref="DiagnosticsRelayClient.RestartAsync(CancellationToken)"/> method.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task RestartTestAsync()
        {
            NSDictionary result = new NSDictionary();
            result.Add("Status", new NSString("Success"));

            var protocol = new Mock<PropertyListProtocol>();
            await using var client = new DiagnosticsRelayClient(protocol.Object);

            protocol
                .Setup(c => c.WriteMessageAsync(It.IsAny<IPropertyList>(), default))
                .Callback((IPropertyList pl, CancellationToken ct) =>
                {
                    var request = Assert.IsType<DiagnosticsRelayRequest>(pl);
                    Assert.Equal("Restart", request.Request);
                    Assert.True(request.WaitForDisconnect);
                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(c => c.ReadMessageAsync(default))
                .ReturnsAsync(result);

            await client.RestartAsync(default).ConfigureAwait(false);

            protocol.Verify();
        }

        /// <summary>
        /// Tests the <see cref="DiagnosticsRelayClient.ShutdownAsync(CancellationToken)"/> method.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task Shutdown_Works_Async()
        {
            NSDictionary result = new NSDictionary();
            result.Add("Status", new NSString("Success"));

            var protocol = new Mock<PropertyListProtocol>();
            await using var client = new DiagnosticsRelayClient(protocol.Object);

            protocol
                .Setup(c => c.WriteMessageAsync(It.IsAny<IPropertyList>(), default))
                .Callback((IPropertyList pl, CancellationToken ct) =>
                {
                    var request = Assert.IsType<DiagnosticsRelayRequest>(pl);
                    Assert.Equal("Shutdown", request.Request);
                    Assert.True(request.WaitForDisconnect);
                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(c => c.ReadMessageAsync(default))
                .ReturnsAsync(result);

            await client.ShutdownAsync(default).ConfigureAwait(false);

            protocol.Verify();
        }

        /// <summary>
        /// Tests the <see cref="DiagnosticsRelayClient.GoodbyeAsync(CancellationToken)"/> method.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task Goodbye_Works_Async()
        {
            NSDictionary result = new NSDictionary();
            result.Add("Status", new NSString("Success"));

            var protocol = new Mock<PropertyListProtocol>();
            await using var client = new DiagnosticsRelayClient(protocol.Object);

            protocol
                .Setup(c => c.WriteMessageAsync(It.IsAny<IPropertyList>(), default))
                .Callback((IPropertyList pl, CancellationToken ct) =>
                {
                    var request = Assert.IsType<DiagnosticsRelayRequest>(pl);
                    Assert.Equal("Goodbye", request.Request);
                    Assert.Null(request.WaitForDisconnect);
                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(c => c.ReadMessageAsync(default))
                .ReturnsAsync(result);

            await client.GoodbyeAsync(default).ConfigureAwait(false);

            protocol.Verify();
        }

        /// <summary>
        /// Tests the <see cref="DiagnosticsRelayClient.QueryIoRegistryEntryAsync(string, string, CancellationToken)"/> method.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task QueryIoRegistryEntry_Works_Async()
        {
            NSDictionary diagnostics = new NSDictionary();
            diagnostics.Add("IORegistry", new NSDictionary());

            NSDictionary result = new NSDictionary();
            result.Add("Status", new NSString("Success"));
            result.Add("Diagnostics", diagnostics);

            var protocol = new Mock<PropertyListProtocol>();
            await using var client = new DiagnosticsRelayClient(protocol.Object);

            protocol
                .Setup(c => c.WriteMessageAsync(It.IsAny<IPropertyList>(), default))
                .Callback((IPropertyList pl, CancellationToken ct) =>
                {
                    var request = Assert.IsType<DiagnosticsRelayRequest>(pl);
                    Assert.Equal("IORegistry", request.Request);
                    Assert.Equal("entry-class", request.EntryClass);
                    Assert.Equal("entry-name", request.EntryName);
                    Assert.Null(request.WaitForDisconnect);
                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(c => c.ReadMessageAsync(default))
                .ReturnsAsync(result);

            var value = await client.QueryIoRegistryEntryAsync("entry-name", "entry-class", default).ConfigureAwait(false);
            Assert.NotNull(value);

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="DiagnosticsRelayClient.QueryIoRegistryEntryAsync(string, string, CancellationToken)"/> returns <see langword="null"/>
        /// when the remote end unexpectedly closes the connection.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task QueryIoRegistryEntry_Disconnecst_ReturnsNull_Async()
        {
            NSDictionary diagnostics = new NSDictionary();
            diagnostics.Add("IORegistry", new NSDictionary());

            NSDictionary result = null;

            var protocol = new Mock<PropertyListProtocol>();
            await using var client = new DiagnosticsRelayClient(protocol.Object);

            protocol
                .Setup(c => c.WriteMessageAsync(It.IsAny<IPropertyList>(), default))
                .Callback((IPropertyList pl, CancellationToken ct) =>
                {
                    var request = Assert.IsType<DiagnosticsRelayRequest>(pl);
                    Assert.Equal("IORegistry", request.Request);
                    Assert.Equal("entry-class", request.EntryClass);
                    Assert.Equal("entry-name", request.EntryName);
                    Assert.Null(request.WaitForDisconnect);
                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(c => c.ReadMessageAsync(default))
                .ReturnsAsync(result);

            var value = await client.QueryIoRegistryEntryAsync("entry-name", "entry-class", default).ConfigureAwait(false);
            Assert.Null(value);

            protocol.Verify();
        }

        /// <summary>
        /// Tests the <see cref="DiagnosticsRelayClient.RestartAsync(CancellationToken)"/> method.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task RestartError_Works_Async()
        {
            NSDictionary result = new NSDictionary();
            result.Add("Status", new NSString("Failure"));

            var protocol = new Mock<PropertyListProtocol>();
            await using var client = new DiagnosticsRelayClient(protocol.Object);

            protocol
                .Setup(c => c.WriteMessageAsync(It.IsAny<IPropertyList>(), default))
                .Callback((IPropertyList pl, CancellationToken ct) =>
                {
                    var request = Assert.IsType<DiagnosticsRelayRequest>(pl);
                    Assert.Equal("Restart", request.Request);
                    Assert.True(request.WaitForDisconnect);
                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(c => c.ReadMessageAsync(default))
                .ReturnsAsync(result);

            await Assert.ThrowsAsync<DiagnosticsRelayException>(() => client.RestartAsync(default)).ConfigureAwait(false);
        }
    }
}
