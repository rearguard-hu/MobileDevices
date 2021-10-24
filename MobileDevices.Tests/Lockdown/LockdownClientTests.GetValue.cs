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
    /// Tests for the <see cref="LockdownClient.GetValueAsync(string, CancellationToken)"/> family of methods.
    /// </content>
    public partial class LockdownClientTests
    {
        /// <summary>
        /// <see cref="LockdownClient.GetValueAsync(string, CancellationToken)"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetValue_NoDomain_Key_Works_Async()
        {
            var dict = new NSDictionary();
            dict.Add("Request", "GetValue");
            dict.Add("Key", "my-key");
            dict.Add("Value", "my-value");

            var protocol = new Mock<LockdownProtocol>(MockBehavior.Strict);
            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Callback<LockdownMessage, CancellationToken>(
                (message, cancellationToken) =>
                {
                    var getValueRequest = Assert.IsType<GetValueRequest>(message);
                    Assert.Null(getValueRequest.Domain);
                    Assert.Equal("my-key", getValueRequest.Key);
                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(dict);

            protocol.Setup(p => p.ReadMessageAsync<GetValueResponse<string>>(default)).CallBase();
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask);

            await using (var client = new LockdownClient(protocol.Object, NullLogger<LockdownClient>.Instance))
            {
                var value = await client.GetValueAsync("my-key", default).ConfigureAwait(false);
                Assert.Equal("my-value", value);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.GetValueAsync{T}(string, string, CancellationToken)"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetValue_Domain_Key_Works_Async()
        {
            var dict = new NSDictionary();
            dict.Add("Request", "GetValue");
            dict.Add("Domain", "my-domain");
            dict.Add("Key", "my-key");
            dict.Add("Value", "my-value");

            var protocol = new Mock<LockdownProtocol>(MockBehavior.Strict);
            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Callback<LockdownMessage, CancellationToken>(
                (message, cancellationToken) =>
                {
                    var getValueRequest = Assert.IsType<GetValueRequest>(message);
                    Assert.Equal("my-domain", getValueRequest.Domain);
                    Assert.Equal("my-key", getValueRequest.Key);
                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(dict);

            protocol.Setup(p => p.ReadMessageAsync<GetValueResponse<string>>(default)).CallBase();
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask);

            await using (var client = new LockdownClient(protocol.Object, NullLogger<LockdownClient>.Instance))
            {
                var value = await client.GetValueAsync<string>("my-domain", "my-key", default).ConfigureAwait(false);
                Assert.Equal("my-value", value);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.GetValueAsync{T}(string, string, CancellationToken)"/> throws when the device
        /// returns an error message.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetValue_ThrowsOnError_Async()
        {
            var dict = new NSDictionary();
            dict.Add("Request", "GetValue");
            dict.Add("Domain", "my-domain");
            dict.Add("Key", "my-key");
            dict.Add("Error", "GetProhibited");

            var protocol = new Mock<LockdownProtocol>(MockBehavior.Strict);
            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Callback<LockdownMessage, CancellationToken>(
                (message, cancellationToken) =>
                {
                    var getValueRequest = Assert.IsType<GetValueRequest>(message);
                    Assert.Equal("my-domain", getValueRequest.Domain);
                    Assert.Equal("my-key", getValueRequest.Key);
                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(dict);

            protocol.Setup(p => p.ReadMessageAsync<GetValueResponse<string>>(default)).CallBase();
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask);

            await using (var client = new LockdownClient(protocol.Object, NullLogger<LockdownClient>.Instance))
            {
                await Assert.ThrowsAsync<LockdownException>(() => client.GetValueAsync<string>("my-domain", "my-key", default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.GetPublicKeyAsync(CancellationToken)"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetPublicKeyAsync_Works_Async()
        {
            var key = Convert.FromBase64String(
                "LS0tLS1CRUdJTiBSU0EgUFVCTElDIEtFWS0tLS0tCk1JR0pBb0dCQUorNXVIQjJycllw" +
                "VEt4SWNGUnJxR1ZqTHRNQ2wyWHhmTVhJeEhYTURrM01jV2hxK2RtWkcvWW0KeDJuTGZq" +
                "WWJPeUduQ1BxQktxcUU5Q2tyQy9DUi9mTlgwNjJqMU1pUHJYY2RnQ0tiNzB2bmVlMFNF" +
                "T2FmNVhEQworZWFZeGdjWTYvbjBXODNrSklXMGF0czhMWmUwTW9XNXpXSTh6cnM4eDIw" +
                "UFFJK1RGU1p4QWdNQkFBRT0KLS0tLS1FTkQgUlNBIFBVQkxJQyBLRVktLS0tLQo=");

            var dict = new NSDictionary();
            dict.Add("Request", "GetValue");
            dict.Add("Key", "DevicePublicKey");
            dict.Add("Value", key);

            var protocol = new Mock<LockdownProtocol>(MockBehavior.Strict);
            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Callback<LockdownMessage, CancellationToken>(
                (message, cancellationToken) =>
                {
                    var getValueRequest = Assert.IsType<GetValueRequest>(message);
                    Assert.Null(getValueRequest.Domain);
                    Assert.Equal("DevicePublicKey", getValueRequest.Key);
                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(dict);

            protocol.Setup(p => p.ReadMessageAsync<GetValueResponse<byte[]>>(default)).CallBase();
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask);

            await using (var client = new LockdownClient(protocol.Object, NullLogger<LockdownClient>.Instance))
            {
                var result = await client.GetPublicKeyAsync(default).ConfigureAwait(false);
                Assert.Equal(key, result);
            }
        }

        /// <summary>
        /// Tests the <see cref="LockdownClient.GetWifiAddressAsync(CancellationToken)"/> method.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetWifiAddress_Works_Async()
        {
            var dict = new NSDictionary();
            dict.Add("Request", "GetValue");
            dict.Add("Key", "WiFiAddress");
            dict.Add("Value", "aa:bb:cc");

            var protocol = new Mock<LockdownProtocol>(MockBehavior.Strict);
            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Callback<LockdownMessage, CancellationToken>(
                (message, cancellationToken) =>
                {
                    var getValueRequest = Assert.IsType<GetValueRequest>(message);
                    Assert.Null(getValueRequest.Domain);
                    Assert.Equal("WiFiAddress", getValueRequest.Key);
                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(dict);

            protocol.Setup(p => p.ReadMessageAsync<GetValueResponse<string>>(default)).CallBase();
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask);

            await using (var client = new LockdownClient(protocol.Object, NullLogger<LockdownClient>.Instance))
            {
                var result = await client.GetWifiAddressAsync(default).ConfigureAwait(false);
                Assert.Equal("aa:bb:cc", result);
            }
        }
    }
}
