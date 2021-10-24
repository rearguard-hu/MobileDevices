using Microsoft.Extensions.DependencyInjection;
using MobileDevices.iOS;
using MobileDevices.iOS.Lockdown;
using MobileDevices.iOS.Muxer;
using MobileDevices.iOS.NotificationProxy;
using MobileDevices.iOS.Workers;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MobileDevices.Tests.Workers
{
    /// <summary>
    /// Tests the <see cref="PairingWorker"/> class.
    /// </summary>
    public class PairingWorkerTests
    {
        private readonly Mock<MuxerClient> muxer;
        private readonly IServiceProvider provider;
        private readonly Mock<LockdownClient> lockdown;
        private readonly Mock<NotificationProxyClient> notificationProxyClient;
        private readonly MuxerDevice device;
        private readonly Mock<PairingRecordGenerator> pairingRecordGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="PairingWorkerTests"/> class.
        /// </summary>
        public PairingWorkerTests()
        {
            muxer = new Mock<MuxerClient>(MockBehavior.Strict);
            notificationProxyClient = new Mock<NotificationProxyClient>(MockBehavior.Strict);
            lockdown = new Mock<LockdownClient>(MockBehavior.Strict);
            device = new MuxerDevice() { Udid = "my-udid" };
            pairingRecordGenerator = new Mock<PairingRecordGenerator>(MockBehavior.Strict);

            var lockdownFactory = new Mock<ClientFactory<LockdownClient>>(MockBehavior.Strict);
            lockdownFactory.Setup(f => f.CreateAsync(default)).ReturnsAsync(lockdown.Object);

            var notificationProxyFactory = new Mock<ClientFactory<NotificationProxyClient>>(MockBehavior.Strict);
            notificationProxyFactory.Setup(p => p.CreateAsync(NotificationProxyClient.InsecureServiceName, default)).ReturnsAsync(notificationProxyClient.Object);

            provider = new ServiceCollection()
                .AddSingleton(muxer.Object)
                .AddScoped((sp) => lockdownFactory.Object)
                .AddScoped((sp) => notificationProxyFactory.Object)
                .AddScoped((sp) => new DeviceContext() { Device = device })
                .AddScoped<PairingWorker>()
                .AddSingleton(pairingRecordGenerator.Object)
                .BuildServiceProvider();
        }

        /// <summary>
        /// The <see cref="PairingWorker"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new PairingWorker(null, new DeviceContext(), Mock.Of<ClientFactory<LockdownClient>>(), Mock.Of<ClientFactory<NotificationProxyClient>>(), Mock.Of<PairingRecordGenerator>()));
            Assert.Throws<ArgumentNullException>(() => new PairingWorker(Mock.Of<MuxerClient>(), null, Mock.Of<ClientFactory<LockdownClient>>(), Mock.Of<ClientFactory<NotificationProxyClient>>(), Mock.Of<PairingRecordGenerator>()));
            Assert.Throws<ArgumentNullException>(() => new PairingWorker(Mock.Of<MuxerClient>(), new DeviceContext(), null, Mock.Of<ClientFactory<NotificationProxyClient>>(), Mock.Of<PairingRecordGenerator>()));
            Assert.Throws<ArgumentNullException>(() => new PairingWorker(Mock.Of<MuxerClient>(), new DeviceContext(), Mock.Of<ClientFactory<LockdownClient>>(), null, Mock.Of<PairingRecordGenerator>()));
            Assert.Throws<ArgumentNullException>(() => new PairingWorker(Mock.Of<MuxerClient>(), new DeviceContext(), Mock.Of<ClientFactory<LockdownClient>>(), Mock.Of<ClientFactory<NotificationProxyClient>>(), null));
        }

        /// <summary>
        /// <see cref="PairingWorker.PairAsync(CancellationToken)"/> returns the pairing record stored in the muxer if that pairing record
        /// is a valid pairing record.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PairAsync_IsPaired_Works_Async()
        {
            notificationProxyClient.Setup(s => s.ObserveNotificationAsync(Notifications.RequestPair, default)).Returns(Task.CompletedTask).Verifiable();

            var pairingRecord = new PairingRecord();
            muxer.Setup(m => m.ReadPairingRecordAsync(device.Udid, default)).ReturnsAsync(pairingRecord);

            lockdown.Setup(l => l.ValidatePairAsync(pairingRecord, default)).ReturnsAsync(true);

            using (var scope = provider.CreateScope())
            {
                var worker = scope.ServiceProvider.GetRequiredService<PairingWorker>();

                var result = await worker.PairAsync(default).ConfigureAwait(false);

                Assert.Same(pairingRecord, result);
            }
        }

        /// <summary>
        /// <see cref="PairingWorker.PairAsync(CancellationToken)"/> creates a new pairing record and pairs with the device, if no pairing
        /// record is currently available.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PairAsync_NotPaired_Pairs_Async()
        {
            notificationProxyClient.Setup(s => s.ObserveNotificationAsync(Notifications.RequestPair, default)).Returns(Task.CompletedTask).Verifiable();

            muxer.Setup(m => m.ReadPairingRecordAsync(device.Udid, default)).ReturnsAsync((PairingRecord)null);

            var publicKey = new byte[] { 1, 2, 3, 4 };
            var wifiAddress = "AA:BB:CC";
            var buid = "123456789abcdef";

            lockdown.Setup(l => l.GetPublicKeyAsync(default)).ReturnsAsync(publicKey);
            lockdown.Setup(l => l.GetWifiAddressAsync(default)).ReturnsAsync(wifiAddress);
            muxer.Setup(m => m.ReadBuidAsync(default)).ReturnsAsync(buid);

            var pairingRecord = new PairingRecord();
            pairingRecordGenerator.Setup(p => p.Generate(publicKey, buid)).Returns(pairingRecord);

            var pairingResult = new PairingResult() { Status = PairingStatus.Success };
            lockdown.Setup(l => l.PairAsync(pairingRecord, default)).ReturnsAsync(pairingResult).Verifiable();

            muxer.Setup(m => m.SavePairingRecordAsync(device.Udid, pairingRecord, default)).Returns(Task.CompletedTask).Verifiable();

            using (var scope = provider.CreateScope())
            {
                var worker = scope.ServiceProvider.GetRequiredService<PairingWorker>();

                var result = await worker.PairAsync(default).ConfigureAwait(false);

                Assert.Same(pairingRecord, result);
            }

            lockdown.Verify();
            muxer.Verify();
        }

        /// <summary>
        /// <see cref="PairingWorker.PairAsync(CancellationToken)"/> creates a new pairing record and pairs with the device, if no pairing
        /// record is currently available, and waits for the user to accept the pairing response if required.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PairAsync_NotPaired_PairsAndWaitsForUserResponse_Async()
        {
            var escrowBag = new byte[] { 5, 6, 7, 8 };
            var pairingResult = new PairingResult() { Status = PairingStatus.PairingDialogResponsePending };

            notificationProxyClient.Setup(s => s.ObserveNotificationAsync(Notifications.RequestPair, default)).Returns(Task.CompletedTask).Verifiable();

            notificationProxyClient
                .Setup(s => s.ReadRelayNotificationAsync(default))
                .ReturnsAsync(Notifications.RequestPair)
                .Callback(() =>
                {
                    pairingResult.EscrowBag = escrowBag;
                    pairingResult.Status = PairingStatus.Success;
                })
                .Verifiable();

            muxer.Setup(m => m.ReadPairingRecordAsync(device.Udid, default)).ReturnsAsync((PairingRecord)null);

            var publicKey = new byte[] { 1, 2, 3, 4 };
            var wifiAddress = "AA:BB:CC";
            var buid = "123456789abcdef";

            lockdown.Setup(l => l.GetPublicKeyAsync(default)).ReturnsAsync(publicKey);
            lockdown.Setup(l => l.GetWifiAddressAsync(default)).ReturnsAsync(wifiAddress);
            muxer.Setup(m => m.ReadBuidAsync(default)).ReturnsAsync(buid);

            var pairingRecord = new PairingRecord();
            pairingRecordGenerator.Setup(p => p.Generate(publicKey, buid)).Returns(pairingRecord);

            lockdown.Setup(l => l.PairAsync(pairingRecord, default)).ReturnsAsync(pairingResult).Verifiable();

            muxer.Setup(m => m.SavePairingRecordAsync(device.Udid, pairingRecord, default)).Returns(Task.CompletedTask).Verifiable();

            using (var scope = provider.CreateScope())
            {
                var worker = scope.ServiceProvider.GetRequiredService<PairingWorker>();

                var result = await worker.PairAsync(default).ConfigureAwait(false);

                Assert.Same(pairingRecord, result);
                Assert.Equal(escrowBag, pairingRecord.EscrowBag);
            }

            lockdown.Verify();
            muxer.Verify();
            notificationProxyClient.Verify();
        }

        /// <summary>
        /// <see cref="PairingWorker.PairAsync(CancellationToken)"/> creates a new pairing record and pairs with the device, if no pairing
        /// record is currently available, and aborts the operation if the user rejects the pairing request.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PairAsync_NotPaired_PairsAndHandlesUserRejection_Async()
        {
            var pairingResult = new PairingResult() { Status = PairingStatus.PairingDialogResponsePending };

            notificationProxyClient.Setup(s => s.ObserveNotificationAsync(Notifications.RequestPair, default)).Returns(Task.CompletedTask).Verifiable();

            notificationProxyClient
                .Setup(s => s.ReadRelayNotificationAsync(default))
                .ReturnsAsync(Notifications.RequestPair)
                .Callback(() =>
                {
                    pairingResult.Status = PairingStatus.UserDeniedPairing;
                })
                .Verifiable();

            muxer.Setup(m => m.ReadPairingRecordAsync(device.Udid, default)).ReturnsAsync((PairingRecord)null);

            var publicKey = new byte[] { 1, 2, 3, 4 };
            var wifiAddress = "AA:BB:CC";
            var buid = "123456789abcdef";

            lockdown.Setup(l => l.GetPublicKeyAsync(default)).ReturnsAsync(publicKey);
            lockdown.Setup(l => l.GetWifiAddressAsync(default)).ReturnsAsync(wifiAddress);
            muxer.Setup(m => m.ReadBuidAsync(default)).ReturnsAsync(buid);

            var pairingRecord = new PairingRecord();
            pairingRecordGenerator.Setup(p => p.Generate(publicKey, buid)).Returns(pairingRecord);

            lockdown.Setup(l => l.PairAsync(pairingRecord, default)).ReturnsAsync(pairingResult).Verifiable();

            muxer.Setup(m => m.SavePairingRecordAsync(device.Udid, pairingRecord, default)).Returns(Task.CompletedTask).Verifiable();
            muxer.Setup(m => m.DeletePairingRecordAsync(device.Udid, default)).Returns(Task.CompletedTask).Verifiable();

            using (var scope = provider.CreateScope())
            {
                var worker = scope.ServiceProvider.GetRequiredService<PairingWorker>();

                var result = await worker.PairAsync(default).ConfigureAwait(false);
                Assert.Null(result);
            }

            lockdown.Verify();
            muxer.Verify();
            notificationProxyClient.Verify();
        }

        /// <summary>
        /// <see cref="PairingWorker.PairAsync(CancellationToken)"/> creates a new pairing record and pairs with the device, if no pairing
        /// record is currently available, and waits for the user to accept the pairing response if required.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PairAsync_PairedButDeniedPairing_AttemptsToPair_Async()
        {
            var escrowBag = new byte[] { 5, 6, 7, 8 };
            var pairingResult = new PairingResult() { Status = PairingStatus.UserDeniedPairing };

            notificationProxyClient.Setup(s => s.ObserveNotificationAsync(Notifications.RequestPair, default)).Returns(Task.CompletedTask).Verifiable();

            notificationProxyClient
                .Setup(s => s.ReadRelayNotificationAsync(default))
                .ReturnsAsync(Notifications.RequestPair)
                .Callback(() =>
                {
                    pairingResult.EscrowBag = escrowBag;
                    pairingResult.Status = PairingStatus.Success;
                })
                .Verifiable();

            var pairingRecord = new PairingRecord();
            muxer.Setup(m => m.ReadPairingRecordAsync(device.Udid, default)).ReturnsAsync(pairingRecord).Verifiable();

            var publicKey = new byte[] { 1, 2, 3, 4 };
            var wifiAddress = "AA:BB:CC";
            var buid = "123456789abcdef";

            lockdown.Setup(l => l.ValidatePairAsync(pairingRecord, default)).ReturnsAsync(false).Verifiable();
            lockdown
                .Setup(l => l.UnpairAsync(pairingRecord, default))
                .Callback(() => { pairingResult.Status = PairingStatus.PairingDialogResponsePending; })
                .ReturnsAsync(pairingResult)
                .Verifiable();
            lockdown.Setup(l => l.GetPublicKeyAsync(default)).ReturnsAsync(publicKey).Verifiable();
            lockdown.Setup(l => l.GetWifiAddressAsync(default)).ReturnsAsync(wifiAddress).Verifiable();
            muxer.Setup(m => m.ReadBuidAsync(default)).ReturnsAsync(buid).Verifiable();
            muxer.Setup(m => m.DeletePairingRecordAsync(device.Udid, default)).Returns(Task.CompletedTask).Verifiable();

            pairingRecordGenerator.Setup(p => p.Generate(publicKey, buid)).Returns(pairingRecord).Verifiable();

            lockdown.Setup(l => l.PairAsync(pairingRecord, default)).ReturnsAsync(pairingResult).Verifiable();

            muxer.Setup(m => m.SavePairingRecordAsync(device.Udid, pairingRecord, default)).Returns(Task.CompletedTask).Verifiable();

            using (var scope = provider.CreateScope())
            {
                var worker = scope.ServiceProvider.GetRequiredService<PairingWorker>();

                var result = await worker.PairAsync(default).ConfigureAwait(false);

                Assert.Same(pairingRecord, result);
                Assert.Equal(escrowBag, pairingRecord.EscrowBag);
            }

            lockdown.Verify();
            muxer.Verify();
            notificationProxyClient.Verify();
        }
    }
}
