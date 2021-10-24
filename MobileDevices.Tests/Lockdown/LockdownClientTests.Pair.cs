using Claunia.PropertyList;
using Microsoft.Extensions.Logging.Abstractions;
using MobileDevices.iOS.Lockdown;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MobileDevices.Tests.Lockdown
{
    /// <content>
    /// Tests the Pair methods on the <see cref="LockdownClient"/> class.
    /// </content>
    public partial class LockdownClientTests
    {
        /// <summary>
        /// <see cref="LockdownClient.PairAsync(PairingRecord, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PairAsync_ValidatesArguments_Async()
        {
            await using (var lockdown = new LockdownClient(Mock.Of<LockdownProtocol>(), NullLogger<LockdownClient>.Instance))
            {
                await Assert.ThrowsAsync<ArgumentNullException>("pairingRecord", () => lockdown.PairAsync(null, default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.PairAsync(PairingRecord, CancellationToken)"/> works.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PairAsync_Works_Async()
        {
            var pairingRecord = new PairingRecord();

            var protocol = new Mock<LockdownProtocol>(MockBehavior.Strict);

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Callback<LockdownMessage, CancellationToken>(
                (message, ct) =>
                {
                    var request = Assert.IsType<PairRequest>(message);

                    Assert.Same(pairingRecord, request.PairRecord);
                    Assert.NotNull(request.PairingOptions);
                    Assert.True(request.PairingOptions.ExtendedPairingErrors);
                    Assert.Equal("Pair", request.Request);
                })
                .Returns(Task.CompletedTask);

            var dict = new NSDictionary();

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(dict);

            protocol.Setup(p => p.ReadMessageAsync<PairResponse>(default)).CallBase();
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask);

            await using (var lockdown = new LockdownClient(protocol.Object, NullLogger<LockdownClient>.Instance))
            {
                var result = await lockdown.PairAsync(pairingRecord, default).ConfigureAwait(false);
                Assert.Equal(PairingStatus.Success, result.Status);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.PairAsync(PairingRecord, CancellationToken)"/> returns an escrow bag if one is provided.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PairAsync_ReturnsEscrowBag_Async()
        {
            var pairingRecord = new PairingRecord();

            var protocol = new Mock<LockdownProtocol>(MockBehavior.Strict);

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Callback<LockdownMessage, CancellationToken>(
                (message, ct) =>
                {
                    var request = Assert.IsType<PairRequest>(message);

                    Assert.Same(pairingRecord, request.PairRecord);
                    Assert.NotNull(request.PairingOptions);
                    Assert.True(request.PairingOptions.ExtendedPairingErrors);
                    Assert.Equal("Pair", request.Request);
                })
                .Returns(Task.CompletedTask);

            protocol.Setup(p => p.ReadMessageAsync<PairResponse>(default)).CallBase();
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask);

            var dict = new NSDictionary();
            var data = new byte[] { 1, 2, 3, 4 };
            dict.Add("EscrowBag", data);

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(dict);

            await using (var lockdown = new LockdownClient(protocol.Object, NullLogger<LockdownClient>.Instance))
            {
                var result = await lockdown.PairAsync(pairingRecord, default).ConfigureAwait(false);
                Assert.Equal(PairingStatus.Success, result.Status);
                Assert.Equal(data, result.EscrowBag);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.PairAsync(PairingRecord, CancellationToken)"/> parses well-known
        /// error messages.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PairAsync_ParsesErrors_Async()
        {
            var pairingRecord = new PairingRecord();

            var protocol = new Mock<LockdownProtocol>(MockBehavior.Strict);

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Returns(Task.CompletedTask);

            var dict = new NSDictionary();
            dict.Add("Error", "PairingDialogResponsePending");

            protocol
                .Setup(p => p.ReadMessageAsync<PairResponse>(default))
                .CallBase();

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(dict);

            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask);

            await using (var lockdown = new LockdownClient(protocol.Object, NullLogger<LockdownClient>.Instance))
            {
                var result = await lockdown.PairAsync(pairingRecord, default).ConfigureAwait(false);
                Assert.Equal(PairingStatus.PairingDialogResponsePending, result.Status);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.PairAsync(PairingRecord, CancellationToken)"/> throws on unknown errors.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PairAsync_ThrowsOnError_Async()
        {
            var pairingRecord = new PairingRecord();

            var protocol = new Mock<LockdownProtocol>(MockBehavior.Strict);

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Returns(Task.CompletedTask);

            var dict = new NSDictionary();
            dict.Add("Error", "SessionInactive");

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(dict);

            protocol.Setup(p => p.ReadMessageAsync<PairResponse>(default)).CallBase();
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask);

            await using (var lockdown = new LockdownClient(protocol.Object, NullLogger<LockdownClient>.Instance))
            {
                await Assert.ThrowsAsync<LockdownException>(() => lockdown.PairAsync(pairingRecord, default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.PairAsync(PairingRecord, CancellationToken)"/> returns <see langword="null"/> when
        /// the device disconnects.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PairAsync_ReturnsNullOnDisconnect_Async()
        {
            var pairingRecord = new PairingRecord();

            var protocol = new Mock<LockdownProtocol>();

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Returns(Task.CompletedTask);

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync((NSDictionary)null);

            await using (var lockdown = new LockdownClient(protocol.Object, NullLogger<LockdownClient>.Instance))
            {
                var result = await lockdown.PairAsync(pairingRecord, default).ConfigureAwait(false);
                Assert.Null(result);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.UnpairAsync(PairingRecord, CancellationToken)"/> works.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UnpairAsync_Works_Async()
        {
            var pairingRecord = new PairingRecord();

            var protocol = new Mock<LockdownProtocol>(MockBehavior.Strict);

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Callback<LockdownMessage, CancellationToken>(
                (message, ct) =>
                {
                    var request = Assert.IsType<PairRequest>(message);

                    Assert.Same(pairingRecord, request.PairRecord);
                    Assert.Null(request.PairingOptions);
                    Assert.Equal("Unpair", request.Request);
                })
                .Returns(Task.CompletedTask);

            var dict = new NSDictionary();

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(dict);

            protocol.Setup(p => p.ReadMessageAsync<PairResponse>(default)).CallBase();
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask);

            await using (var lockdown = new LockdownClient(protocol.Object, NullLogger<LockdownClient>.Instance))
            {
                var result = await lockdown.UnpairAsync(pairingRecord, default).ConfigureAwait(false);
                Assert.Equal(PairingStatus.Success, result.Status);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.ValidatePairAsync(PairingRecord, CancellationToken)"/> works.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidatePairAsync_Works_Async()
        {
            var pairingRecord = new PairingRecord()
            {
                HostId = "abc",
                SystemBUID = "def",
            };

            List<LockdownMessage> requests = new List<LockdownMessage>();
            var protocol = new Mock<LockdownProtocol>(MockBehavior.Strict);

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Callback<LockdownMessage, CancellationToken>(
                (message, ct) =>
                {
                    requests.Add(message);
                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(p => p.SslEnabled)
                .Returns(false);

            var dict = new NSDictionary();
            dict.Add("SessionID", "1234");

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(dict);

            protocol.Setup(p => p.ReadMessageAsync<StartSessionResponse>(default)).CallBase();
            protocol.Setup(p => p.ReadMessageAsync<LockdownResponse>(default)).CallBase();
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask);

            await using (var lockdown = new LockdownClient(protocol.Object, NullLogger<LockdownClient>.Instance))
            {
                var result = await lockdown.ValidatePairAsync(pairingRecord, default).ConfigureAwait(false);
                Assert.True(result);
            }

            Assert.Collection(
                requests,
                message =>
                {
                    var request = Assert.IsType<StartSessionRequest>(message);

                    Assert.Equal("abc", request.HostID);
                    Assert.Equal("def", request.SystemBUID);
                    Assert.Equal("StartSession", request.Request);
                },
                message =>
                {
                    var request = Assert.IsType<StopSessionRequest>(message);

                    Assert.Equal("1234", request.SessionID);
                });
        }

        /// <summary>
        /// <see cref="LockdownClient.ValidatePairAsync(PairingRecord, CancellationToken)"/> works.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidatePairAsync_ThrowsOnError_Async()
        {
            var pairingRecord = new PairingRecord()
            {
                HostId = "abc",
                SystemBUID = "def",
            };

            List<LockdownMessage> requests = new List<LockdownMessage>();
            var protocol = new Mock<LockdownProtocol>(MockBehavior.Strict);

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Callback<LockdownMessage, CancellationToken>(
                (message, ct) =>
                {
                    requests.Add(message);
                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(p => p.SslEnabled)
                .Returns(false);

            var dict = new NSDictionary();
            dict.Add("Error", "InvalidHostID");

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(dict);

            protocol.Setup(p => p.ReadMessageAsync<StartSessionResponse>(default)).CallBase();
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask);

            await using (var lockdown = new LockdownClient(protocol.Object, NullLogger<LockdownClient>.Instance))
            {
                var result = await lockdown.ValidatePairAsync(pairingRecord, default).ConfigureAwait(false);
                Assert.False(result);
            }

            Assert.Collection(
                requests,
                message =>
                {
                    var request = Assert.IsType<StartSessionRequest>(message);

                    Assert.Equal("abc", request.HostID);
                    Assert.Equal("def", request.SystemBUID);
                    Assert.Equal("StartSession", request.Request);
                });
        }
    }
}
