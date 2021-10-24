using Microsoft.Extensions.Logging.Abstractions;
using MobileDevices.iOS.Lockdown;
using MobileDevices.iOS.Muxer;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MobileDevices.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="MuxerPairingRecordStore"/> class.
    /// </summary>
    public class MuxerPairingRecordStoreTests
    {
        /// <summary>
        /// The <see cref="MuxerPairingRecordStore"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new MuxerPairingRecordStore(null, NullLogger<MuxerPairingRecordStore>.Instance));
            Assert.Throws<ArgumentNullException>(() => new MuxerPairingRecordStore(Mock.Of<MuxerClient>(), null));
        }

        /// <summary>
        /// The <see cref="MuxerPairingRecordStore.DeleteAsync(string, CancellationToken)"/> forwards request
        /// to the muxer.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteAsync_Works_Async()
        {
            var muxerClient = new Mock<MuxerClient>(MockBehavior.Strict);
            muxerClient.Setup(c => c.DeletePairingRecordAsync("abc", default)).Returns(Task.CompletedTask);

            var store = new MuxerPairingRecordStore(muxerClient.Object, NullLogger<MuxerPairingRecordStore>.Instance);

            await store.DeleteAsync("abc", default).ConfigureAwait(false);

            muxerClient.Verify();
        }

        /// <summary>
        /// The <see cref="MuxerPairingRecordStore.ReadAsync(string, CancellationToken)"/> forwards request
        /// to the muxer.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReadAsync_Works_Async()
        {
            var record = new PairingRecord();
            var muxerClient = new Mock<MuxerClient>(MockBehavior.Strict);
            muxerClient.Setup(c => c.ReadPairingRecordAsync("abc", default)).Returns(Task.FromResult(record));

            var store = new MuxerPairingRecordStore(muxerClient.Object, NullLogger<MuxerPairingRecordStore>.Instance);

            var result = await store.ReadAsync("abc", default).ConfigureAwait(false);
            Assert.Same(record, result);

            muxerClient.Verify();
        }

        /// <summary>
        /// The <see cref="MuxerPairingRecordStore.WriteAsync(string, PairingRecord, CancellationToken)"/> forwards request
        /// to the muxer.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WriteAsync_Works_Async()
        {
            var record = new PairingRecord();
            var muxerClient = new Mock<MuxerClient>(MockBehavior.Strict);
            muxerClient.Setup(c => c.SavePairingRecordAsync("abc", record, default)).Returns(Task.FromResult(record));

            var store = new MuxerPairingRecordStore(muxerClient.Object, NullLogger<MuxerPairingRecordStore>.Instance);

            await store.WriteAsync("abc", record, default).ConfigureAwait(false);

            muxerClient.Verify();
        }
    }
}
