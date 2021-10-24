using MobileDevices.iOS.Lockdown;
using MobileDevices.iOS.Muxer;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MobileDevices.Tests.Muxer
{
    /// <content>
    /// Tests the pairing record-related methods on the <see cref="MuxerClient"/> class.
    /// </content>
    public partial class MuxerClientTests
    {
        /// <summary>
        /// The <see cref="MuxerClient.ReadPairingRecordAsync(string, CancellationToken)"/> method validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReadPairingRecordAsync_ValidatesArguments_Async()
        {
            var muxerMock = new Mock<MuxerClient>();
            muxerMock.CallBase = true;
            var muxer = muxerMock.Object;

            await Assert.ThrowsAsync<ArgumentNullException>(() => muxer.ReadPairingRecordAsync(null, default));
        }

        /// <summary>
        /// <see cref="MuxerClient.ReadPairingRecordAsync(string, CancellationToken)"/> returns <see langword="null"/>
        /// if no pairing record could be found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReadPairingRecord_DeviceNotFound_ReturnsNull_Async()
        {
            var protocol = new Mock<MuxerProtocol>();
            var muxerMock = new Mock<MuxerClient>();
            muxerMock.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync(protocol.Object);
            var muxer = muxerMock.Object;

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<MuxerMessage>(), default))
                .Callback<MuxerMessage, CancellationToken>(
                (message, ct) =>
                {
                    var readMessage = Assert.IsType<ReadPairingRecordMessage>(message);
                    Assert.Equal(MuxerMessageType.ReadPairRecord, readMessage.MessageType);
                    Assert.Equal("abc", readMessage.PairRecordID);
                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(
                    new ResultMessage()
                    {
                        Number = MuxerError.BadDevice,
                    });

            Assert.Null(await muxer.ReadPairingRecordAsync("abc", default).ConfigureAwait(false));
        }

        /// <summary>
        /// <see cref="MuxerClient.ReadPairingRecordAsync(string, CancellationToken)"/> throws on errors.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReadPairingRecord_OtherError_Throws_Async()
        {
            var protocol = new Mock<MuxerProtocol>();
            var muxerMock = new Mock<MuxerClient>();
            muxerMock.CallBase = true;
            muxerMock.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync(protocol.Object);
            var muxer = muxerMock.Object;

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<MuxerMessage>(), default))
                .Callback<MuxerMessage, CancellationToken>(
                (message, ct) =>
                {
                    var readMessage = Assert.IsType<ReadPairingRecordMessage>(message);
                    Assert.Equal(MuxerMessageType.ReadPairRecord, readMessage.MessageType);
                    Assert.Equal("abc", readMessage.PairRecordID);
                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(
                    new ResultMessage()
                    {
                        Number = MuxerError.BadCommand,
                    });

            await Assert.ThrowsAsync<MuxerException>(() => muxer.ReadPairingRecordAsync("abc", default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="MuxerClient.ReadPairingRecordAsync(string, CancellationToken)"/> returns the requested pairing record.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReadPairingRecord_Works_Async()
        {
            var protocol = new Mock<MuxerProtocol>();
            var muxerMock = new Mock<MuxerClient>();
            muxerMock.CallBase = true;
            muxerMock.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync(protocol.Object);
            var muxer = muxerMock.Object;
            var pairingRecord = PairingRecord.Read(File.ReadAllBytes("Lockdown/0123456789abcdef0123456789abcdef01234567.plist"));

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<MuxerMessage>(), default))
                .Callback<MuxerMessage, CancellationToken>(
                (message, ct) =>
                {
                    var readMessage = Assert.IsType<ReadPairingRecordMessage>(message);
                    Assert.Equal(MuxerMessageType.ReadPairRecord, readMessage.MessageType);
                    Assert.Equal("abc", readMessage.PairRecordID);
                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(
                    new PairingRecordDataMessage()
                    {
                        PairRecordData = pairingRecord.ToByteArray(),
                    });

            var result = await muxer.ReadPairingRecordAsync("abc", default).ConfigureAwait(false);
            Assert.Equal(pairingRecord.ToByteArray(), result.ToByteArray());
        }

        /// <summary>
        /// <see cref="MuxerClient.SavePairingRecordAsync(string, PairingRecord, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SavePairingRecordAsync_ValidatesArguments_Async()
        {
            var muxerMock = new Mock<MuxerClient>();
            muxerMock.CallBase = true;
            var muxer = muxerMock.Object;

            await Assert.ThrowsAsync<ArgumentNullException>(() => muxer.SavePairingRecordAsync(null, new PairingRecord(), default));
            await Assert.ThrowsAsync<ArgumentNullException>(() => muxer.SavePairingRecordAsync("abc", null, default));
        }

        /// <summary>
        /// <see cref="MuxerClient.SavePairingRecordAsync(string, PairingRecord, CancellationToken)"/> throws on errors.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SavePairingRecord_ThrowsOnError_Async()
        {
            var protocol = new Mock<MuxerProtocol>();
            var muxerMock = new Mock<MuxerClient>();
            muxerMock.CallBase = true;
            muxerMock.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync(protocol.Object);
            var muxer = muxerMock.Object;
            var pairingRecord = PairingRecord.Read(File.ReadAllBytes("Lockdown/0123456789abcdef0123456789abcdef01234567.plist"));

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<MuxerMessage>(), default))
                .Callback<MuxerMessage, CancellationToken>(
                (message, ct) =>
                {
                    var readMessage = Assert.IsType<SavePairingRecordMessage>(message);
                    Assert.Equal(MuxerMessageType.SavePairRecord, readMessage.MessageType);
                    Assert.Equal("abc", readMessage.PairRecordID);
                    Assert.Equal(pairingRecord.ToByteArray(), readMessage.PairRecordData);
                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(new ResultMessage() { Number = MuxerError.BadCommand });

            await Assert.ThrowsAsync<MuxerException>(() => muxer.SavePairingRecordAsync("abc", pairingRecord, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="MuxerClient.SavePairingRecordAsync(string, PairingRecord, CancellationToken)"/> works.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SavePairingRecord_Works_Async()
        {
            var protocol = new Mock<MuxerProtocol>();
            var muxerMock = new Mock<MuxerClient>();
            muxerMock.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync(protocol.Object);
            var muxer = muxerMock.Object;
            var pairingRecord = PairingRecord.Read(File.ReadAllBytes("Lockdown/0123456789abcdef0123456789abcdef01234567.plist"));

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<MuxerMessage>(), default))
                .Callback<MuxerMessage, CancellationToken>(
                (message, ct) =>
                {
                    var readMessage = Assert.IsType<SavePairingRecordMessage>(message);
                    Assert.Equal(MuxerMessageType.SavePairRecord, readMessage.MessageType);
                    Assert.Equal("abc", readMessage.PairRecordID);
                    Assert.Equal(pairingRecord.ToByteArray(), readMessage.PairRecordData);
                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(new ResultMessage());

            await muxer.SavePairingRecordAsync("abc", pairingRecord, default).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="MuxerClient.DeletePairingRecordAsync(string, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteRecordAsync_ValidatesArguments_Async()
        {
            var muxerMock = new Mock<MuxerClient>();
            muxerMock.CallBase = true;
            var muxer = muxerMock.Object;

            await Assert.ThrowsAsync<ArgumentNullException>(() => muxer.DeletePairingRecordAsync(null, default));
        }

        /// <summary>
        /// <see cref="MuxerClient.DeletePairingRecordAsync(string, CancellationToken)"/> throws on errors.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeletePairingRecord_ThrowsOnError_Async()
        {
            var protocol = new Mock<MuxerProtocol>();
            var muxerMock = new Mock<MuxerClient>();
            muxerMock.CallBase = true;
            muxerMock.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync(protocol.Object);
            var muxer = muxerMock.Object;

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<MuxerMessage>(), default))
                .Callback<MuxerMessage, CancellationToken>(
                (message, ct) =>
                {
                    var readMessage = Assert.IsType<DeletePairingRecordMessage>(message);
                    Assert.Equal(MuxerMessageType.DeletePairRecord, readMessage.MessageType);
                    Assert.Equal("abc", readMessage.PairRecordID);
                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(new ResultMessage() { Number = MuxerError.BadCommand });

            await Assert.ThrowsAsync<MuxerException>(() => muxer.DeletePairingRecordAsync("abc", default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="MuxerClient.DeletePairingRecordAsync(string, CancellationToken)"/> works.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeletePairingRecord_Works_Async()
        {
            var protocol = new Mock<MuxerProtocol>();
            var muxerMock = new Mock<MuxerClient>();
            muxerMock.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync(protocol.Object);
            var muxer = muxerMock.Object;

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<MuxerMessage>(), default))
                .Callback<MuxerMessage, CancellationToken>(
                (message, ct) =>
                {
                    var readMessage = Assert.IsType<DeletePairingRecordMessage>(message);
                    Assert.Equal(MuxerMessageType.DeletePairRecord, readMessage.MessageType);
                    Assert.Equal("abc", readMessage.PairRecordID);
                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(new ResultMessage());

            await muxer.DeletePairingRecordAsync("abc", default).ConfigureAwait(false);
        }
    }
}
