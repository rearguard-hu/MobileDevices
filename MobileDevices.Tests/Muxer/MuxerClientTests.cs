using Claunia.PropertyList;
using Microsoft.Extensions.Logging.Abstractions;
using MobileDevices.iOS.Muxer;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MobileDevices.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="MuxerClient"/> method.
    /// </summary>
    public partial class MuxerClientTests
    {
        /// <summary>
        /// The <see cref="MuxerClient"/> constructors validate the argments being passed.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("logger", () => new MuxerClient(new MuxerSocketLocator(NullLogger<MuxerSocketLocator>.Instance), null, NullLoggerFactory.Instance));
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new MuxerClient(new MuxerSocketLocator(NullLogger<MuxerSocketLocator>.Instance), NullLogger<MuxerClient>.Instance, null));
            Assert.Throws<ArgumentNullException>("socketLocator", () => new MuxerClient(null, NullLogger<MuxerClient>.Instance, NullLoggerFactory.Instance));
        }

        /// <summary>
        /// <see cref="MuxerClient.TryConnectToMuxerAsync(CancellationToken)"/> returns <see langword="null"/> if
        /// <see cref="MuxerSocketLocator.ConnectToMuxerAsync(CancellationToken)"/> returns <see langword="null"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task TryConnectToMuxerAsync_ReturnsNull_Async()
        {
            var locator = new Mock<MuxerSocketLocator>(NullLogger<MuxerSocketLocator>.Instance);
            locator.Setup(l => l.ConnectToMuxerAsync(default)).ReturnsAsync((Stream)null);

            var client = new MuxerClient(locator.Object, NullLogger<MuxerClient>.Instance, NullLoggerFactory.Instance);
            Assert.Null(await client.TryConnectToMuxerAsync(default).ConfigureAwait(false));
        }

        /// <summary>
        /// <see cref="MuxerClient.TryConnectToMuxerAsync(CancellationToken)"/> returns a <see cref="MuxerProtocol"/> if
        /// <see cref="MuxerSocketLocator.ConnectToMuxerAsync(CancellationToken)"/> returns a <see cref="Stream"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task TryConnectToMuxerAsync_ReturnsMuxerProtocol_Async()
        {
            var muxerStream = Mock.Of<Stream>();

            var locator = new Mock<MuxerSocketLocator>(NullLogger<MuxerSocketLocator>.Instance);
            locator.Setup(l => l.ConnectToMuxerAsync(default)).ReturnsAsync(muxerStream);

            var client = new MuxerClient(locator.Object, NullLogger<MuxerClient>.Instance, NullLoggerFactory.Instance);
            var protocol = await client.TryConnectToMuxerAsync(default).ConfigureAwait(false);
            Assert.NotNull(protocol);
        }

        /// <summary>
        /// <see cref="MuxerClient.ListDevicesAsync(CancellationToken)"/> returns an empty list if the usbmuxd
        /// socket is not available.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ListDevicesAsync_NoSocket_ReturnsEmptyList_Async()
        {
            var locator = new Mock<MuxerSocketLocator>(NullLogger<MuxerSocketLocator>.Instance);
            locator.Setup(l => l.ConnectToMuxerAsync(default)).ReturnsAsync((Stream)null);

            var client = new MuxerClient(locator.Object, NullLogger<MuxerClient>.Instance, NullLoggerFactory.Instance);
            var result = await client.ListDevicesAsync(default).ConfigureAwait(false);
            Assert.Empty(result);
        }

        /// <summary>
        /// The <see cref="MuxerClient.ListDevicesAsync(CancellationToken)"/> method returns a list of devices
        /// which are currently connected to the host.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ListDevicesAsync_WithWifiDevices_ReturnsDeviceList_Async()
        {
            var protocol = new Mock<MuxerProtocol>(MockBehavior.Strict);
            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<MuxerMessage>(), default))
                .Callback<MuxerMessage, CancellationToken>((message, ct) =>
                {
                    Assert.Equal(MuxerMessageType.ListDevices, message.MessageType);
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            protocol
                .Setup(p => p.DisposeAsync())
                .Returns(ValueTask.CompletedTask)
                .Verifiable();

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(
                    DeviceListMessage.Read(
                        (NSDictionary)PropertyListParser.Parse("Muxer/devicelist-wifi.xml")));

            var clientMock = new Mock<MuxerClient>(Mock.Of<MuxerSocketLocator>(), NullLogger<MuxerClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };
            clientMock.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync(protocol.Object);

            var client = clientMock.Object;
            var result = await client.ListDevicesAsync(default).ConfigureAwait(false);
            Assert.Collection(
                result,
                device =>
                {
                    Assert.Equal(MuxerConnectionType.Network, device.ConnectionType);
                    Assert.Equal(2, device.DeviceID);
                    Assert.Equal(IPAddress.Parse("192.168.10.239"), device.IPAddress);
                    Assert.Equal("cccccccccccccccccccccccccccccccccccccccc", device.Udid);
                });

            protocol.Verify();
        }

        /// <summary>
        /// The <see cref="MuxerClient.ListDevicesAsync(CancellationToken)"/> method returns a list of devices
        /// which are currently connected to the host.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ListDevicesAsync_WitLocalDevices_ReturnsDeviceList_Async()
        {
            var protocol = new Mock<MuxerProtocol>(MockBehavior.Strict);
            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<MuxerMessage>(), default))
                .Callback<MuxerMessage, CancellationToken>((message, ct) =>
                {
                    Assert.Equal(MuxerMessageType.ListDevices, message.MessageType);
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            protocol
                .Setup(p => p.DisposeAsync())
                .Returns(ValueTask.CompletedTask)
                .Verifiable();

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(
                    DeviceListMessage.Read(
                        (NSDictionary)PropertyListParser.Parse("Muxer/devicelist.xml")));

            var clientMock = new Mock<MuxerClient>(Mock.Of<MuxerSocketLocator>(), NullLogger<MuxerClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };
            clientMock.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync(protocol.Object);

            var client = clientMock.Object;
            var result = await client.ListDevicesAsync(default).ConfigureAwait(false);
            Assert.Collection(
                result,
                device =>
                {
                    Assert.Equal(MuxerConnectionType.USB, device.ConnectionType);
                    Assert.Equal(5, device.DeviceID);
                    Assert.Equal("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", device.Udid);
                },
                device =>
                {
                    Assert.Equal(MuxerConnectionType.USB, device.ConnectionType);
                    Assert.Equal(6, device.DeviceID);
                    Assert.Equal("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb", device.Udid);
                });

            protocol.Verify();
        }

        /// <summary>
        /// The <see cref="MuxerClient.ListDevicesAsync(CancellationToken)"/> method returns a list of devices
        /// which are currently connected to the host.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ListDevicesAsync_WithNewUdid_ReturnsDeviceList_Async()
        {
            var protocol = new Mock<MuxerProtocol>(MockBehavior.Strict);
            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<MuxerMessage>(), default))
                .Callback<MuxerMessage, CancellationToken>((message, ct) =>
                {
                    Assert.Equal(MuxerMessageType.ListDevices, message.MessageType);
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            protocol
                .Setup(p => p.DisposeAsync())
                .Returns(ValueTask.CompletedTask)
                .Verifiable();

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(
                    DeviceListMessage.Read(
                        (NSDictionary)PropertyListParser.Parse("Muxer/devicelist-udid2.xml")));

            var clientMock = new Mock<MuxerClient>(Mock.Of<MuxerSocketLocator>(), NullLogger<MuxerClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };
            clientMock.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync(protocol.Object);

            var client = clientMock.Object;
            var result = await client.ListDevicesAsync(default).ConfigureAwait(false);
            Assert.Collection(
                result,
                device =>
                {
                    Assert.Equal(MuxerConnectionType.USB, device.ConnectionType);
                    Assert.Equal(5, device.DeviceID);
                    Assert.Equal("00000000-0000000000000000", device.Udid);
                });

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="MuxerClient.ListenAsync"/> returns <see langword="false"/> when the muxer is unavailable.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task ListenAsync_NoMuxer_ReturnsFalse_Async()
        {
            var clientMock = new Mock<MuxerClient>()
            {
                CallBase = true,
            };
            clientMock.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync((MuxerProtocol)null);
            var client = clientMock.Object;

            Assert.False(await client.ListenAsync(null, null, null, default).ConfigureAwait(false));
        }

        /// <summary>
        /// <see cref="MuxerClient.ListenAsync"/> returns <see langword="false"/> when the muxer closes the connection
        /// after receiving the listen request.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task ListenAsync_NoResponse_ReturnsFalse_Async()
        {
            var protocol = new Mock<MuxerProtocol>();
            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<MuxerMessage>(), default))
                .Returns(Task.CompletedTask)
                .Callback<MuxerMessage, CancellationToken>((message, ct) =>
                {
                    var listenRequest = Assert.IsType<ListenMessage>(message);
                })
                .Verifiable();

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync((MuxerMessage)null);

            var clientMock = new Mock<MuxerClient>(Mock.Of<MuxerSocketLocator>(), NullLogger<MuxerClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync(protocol.Object);
            var client = clientMock.Object;

            Assert.False(await client.ListenAsync(null, null, null, default).ConfigureAwait(false));

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="MuxerClient.ListenAsync"/> throws an exception when the muxer returns an error message.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task ListenAsync_ErrorResponse_Throws_Async()
        {
            var protocol = new Mock<MuxerProtocol>();
            protocol.Setup(p => p.WriteMessageAsync(It.IsAny<MuxerMessage>(), default))
                .Returns(Task.CompletedTask)
                .Callback<MuxerMessage, CancellationToken>((message, ct) =>
                {
                    var listenRequest = Assert.IsType<ListenMessage>(message);
                })
                .Verifiable();

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(new ResultMessage() { Number = MuxerError.BadCommand });

            var clientMock = new Mock<MuxerClient>(Mock.Of<MuxerSocketLocator>(), NullLogger<MuxerClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync(protocol.Object);
            var client = clientMock.Object;

            await Assert.ThrowsAsync<MuxerException>(() => client.ListenAsync(null, null, null, default)).ConfigureAwait(false);

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="MuxerClient.ListenAsync"/> loops through all messages until the muxer closes the connection.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task ListenAsync_ReadAll_Async()
        {
            var queue = new Queue<MuxerMessage>(
                new MuxerMessage[]
                {
                    new ResultMessage() { Number = MuxerError.Success },
                    new DeviceAttachedMessage() { DeviceID = 1 },
                    new DevicePairedMessage() { DeviceID = 1 },
                    new DeviceDetachedMessage() { DeviceID = 1 },
                    new DeviceAttachedMessage() { DeviceID = 2 },
                    new DevicePairedMessage() { DeviceID = 2 },
                    new DeviceDetachedMessage() { DeviceID = 2 },
                    null,
                });

            var protocol = new Mock<MuxerProtocol>();
            protocol.Setup(p => p.WriteMessageAsync(It.IsAny<MuxerMessage>(), default))
                .Returns(Task.CompletedTask)
                .Callback<MuxerMessage, CancellationToken>((message, ct) =>
                {
                    var listenRequest = Assert.IsType<ListenMessage>(message);
                })
                .Verifiable();

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(queue.Dequeue);

            var clientMock = new Mock<MuxerClient>(Mock.Of<MuxerSocketLocator>(), NullLogger<MuxerClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync(protocol.Object);
            var client = clientMock.Object;

            Assert.False(
                await client.ListenAsync(
                    (onAttached, ct) => { return Task.FromResult(MuxerListenAction.ContinueListening); },
                    (onDetached, ct) => { return Task.FromResult(MuxerListenAction.ContinueListening); },
                    (onPaired, ct) => { return Task.FromResult(MuxerListenAction.ContinueListening); },
                    default).ConfigureAwait(false));

            Assert.Empty(queue);
            protocol.Verify();
        }

        /// <summary>
        /// <see cref="MuxerClient.ListenAsync"/> stops looping when the on attached callback instructs it to do so.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task ListenAsync_StopsOnAttached_Async()
        {
            var queue = new Queue<MuxerMessage>(
                new MuxerMessage[]
                {
                    new ResultMessage() { Number = MuxerError.Success },
                    new DeviceAttachedMessage() { DeviceID = 1 },
                    new DevicePairedMessage() { DeviceID = 1 },
                    new DeviceDetachedMessage() { DeviceID = 1 },
                    new DeviceAttachedMessage() { DeviceID = 2 },
                    new DevicePairedMessage() { DeviceID = 2 },
                    new DeviceDetachedMessage() { DeviceID = 2 },
                    null,
                });

            var protocol = new Mock<MuxerProtocol>();
            protocol.Setup(p => p.WriteMessageAsync(It.IsAny<MuxerMessage>(), default))
                .Returns(Task.CompletedTask)
                .Callback<MuxerMessage, CancellationToken>((message, ct) =>
                {
                    var listenRequest = Assert.IsType<ListenMessage>(message);
                })
                .Verifiable();

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(queue.Dequeue);

            var clientMock = new Mock<MuxerClient>(Mock.Of<MuxerSocketLocator>(), NullLogger<MuxerClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync(protocol.Object);
            var client = clientMock.Object;

            Assert.True(
                await client.ListenAsync(
                    (onAttached, ct) => { return Task.FromResult(onAttached.DeviceID == 1 ? MuxerListenAction.ContinueListening : MuxerListenAction.StopListening); },
                    (onDetached, ct) => { return Task.FromResult(MuxerListenAction.ContinueListening); },
                    (onPaired, ct) => { return Task.FromResult(MuxerListenAction.ContinueListening); },
                    default).ConfigureAwait(false));

            Assert.Equal(3, queue.Count);
            protocol.Verify();
        }

        /// <summary>
        /// <see cref="MuxerClient.ListenAsync"/> stops looping when the on paired callback instructs it to do so.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task ListenAsync_StopsOnPaired_Async()
        {
            var queue = new Queue<MuxerMessage>(
                new MuxerMessage[]
                {
                    new ResultMessage() { Number = MuxerError.Success },
                    new DeviceAttachedMessage() { DeviceID = 1 },
                    new DevicePairedMessage() { DeviceID = 1 },
                    new DeviceDetachedMessage() { DeviceID = 1 },
                    new DeviceAttachedMessage() { DeviceID = 2 },
                    new DevicePairedMessage() { DeviceID = 2 },
                    new DeviceDetachedMessage() { DeviceID = 2 },
                    null,
                });

            var protocol = new Mock<MuxerProtocol>();
            protocol.Setup(p => p.WriteMessageAsync(It.IsAny<MuxerMessage>(), default))
                .Returns(Task.CompletedTask)
                .Callback<MuxerMessage, CancellationToken>((message, ct) =>
                {
                    var listenRequest = Assert.IsType<ListenMessage>(message);
                })
                .Verifiable();

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(queue.Dequeue);

            var clientMock = new Mock<MuxerClient>(Mock.Of<MuxerSocketLocator>(), NullLogger<MuxerClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync(protocol.Object);
            var client = clientMock.Object;

            Assert.True(
                await client.ListenAsync(
                    (onAttached, ct) => { return Task.FromResult(MuxerListenAction.ContinueListening); },
                    (onDetached, ct) => { return Task.FromResult(onDetached.DeviceID == 1 ? MuxerListenAction.ContinueListening : MuxerListenAction.StopListening); },
                    (onPaired, ct) => { return Task.FromResult(MuxerListenAction.ContinueListening); },
                    default).ConfigureAwait(false));

            Assert.Single(queue);
            protocol.Verify();
        }

        /// <summary>
        /// <see cref="MuxerClient.ListenAsync"/> stops looping when the on paired callback instructs it to do so.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task ListenAsync_StopsOnDetached_Async()
        {
            var queue = new Queue<MuxerMessage>(
                new MuxerMessage[]
                {
                    new ResultMessage() { Number = MuxerError.Success },
                    new DeviceAttachedMessage() { DeviceID = 1 },
                    new DevicePairedMessage() { DeviceID = 1 },
                    new DeviceDetachedMessage() { DeviceID = 1 },
                    new DeviceAttachedMessage() { DeviceID = 2 },
                    new DevicePairedMessage() { DeviceID = 2 },
                    new DeviceDetachedMessage() { DeviceID = 2 },
                    null,
                });

            var protocol = new Mock<MuxerProtocol>();
            protocol.Setup(p => p.WriteMessageAsync(It.IsAny<MuxerMessage>(), default))
                .Returns(Task.CompletedTask)
                .Callback<MuxerMessage, CancellationToken>((message, ct) =>
                {
                    var listenRequest = Assert.IsType<ListenMessage>(message);
                })
                .Verifiable();

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(queue.Dequeue);

            var clientMock = new Mock<MuxerClient>(Mock.Of<MuxerSocketLocator>(), NullLogger<MuxerClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync(protocol.Object);
            var client = clientMock.Object;

            Assert.True(
                await client.ListenAsync(
                    (onAttached, ct) => { return Task.FromResult(MuxerListenAction.ContinueListening); },
                    (onDetached, ct) => { return Task.FromResult(MuxerListenAction.ContinueListening); },
                    (onPaired, ct) => { return Task.FromResult(onPaired.DeviceID == 1 ? MuxerListenAction.ContinueListening : MuxerListenAction.StopListening); },
                    default).ConfigureAwait(false));

            Assert.Equal(2, queue.Count);
            protocol.Verify();
        }

        /// <summary>
        /// <see cref="MuxerClient.ListenAsync"/> throws an <see cref="InvalidDataException"/> if an unexpected message
        /// is received.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task ListenAsync_ThrowsOnUnexceptedMessage_Async()
        {
            var queue = new Queue<MuxerMessage>(
                new MuxerMessage[]
                {
                    new ResultMessage() { Number = MuxerError.Success },
                    new ResultMessage() { Number = MuxerError.Success },
                });

            var protocol = new Mock<MuxerProtocol>();
            protocol.Setup(p => p.WriteMessageAsync(It.IsAny<MuxerMessage>(), default))
                .Returns(Task.CompletedTask)
                .Callback<MuxerMessage, CancellationToken>((message, ct) =>
                {
                    var listenRequest = Assert.IsType<ListenMessage>(message);
                })
                .Verifiable();

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(queue.Dequeue);

            var clientMock = new Mock<MuxerClient>(Mock.Of<MuxerSocketLocator>(), NullLogger<MuxerClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync(protocol.Object);
            var client = clientMock.Object;

            await Assert.ThrowsAsync<InvalidDataException>(
                () => client.ListenAsync(
                    (onAttached, ct) => { return Task.FromResult(MuxerListenAction.ContinueListening); },
                    (onDetached, ct) => { return Task.FromResult(MuxerListenAction.ContinueListening); },
                    (onPaired, ct) => { return Task.FromResult(MuxerListenAction.ContinueListening); },
                    default)).ConfigureAwait(false);

            Assert.Empty(queue);
            protocol.Verify();
        }
    }
}
