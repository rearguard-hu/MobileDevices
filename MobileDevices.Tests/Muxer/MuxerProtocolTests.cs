using Claunia.PropertyList;
using Microsoft.Extensions.Logging.Abstractions;
using MobileDevices.iOS.Muxer;
using Moq;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MobileDevices.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="MuxerProtocol"/> class.
    /// </summary>
    public class MuxerProtocolTests
    {
        /// <summary>
        /// The <see cref="MuxerProtocol"/> class disposes of the underlying stream if it owns the stream.
        /// </summary>
        /// <param name="ownsStream">
        /// A value indicating whether the <see cref="MuxerProtocol"/> owns the stream.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DisposeAsync_DisposesStreamIfNeeded_Async(bool ownsStream)
        {
            var stream = new Mock<Stream>(MockBehavior.Strict);

            if (ownsStream)
            {
                stream.Setup(s => s.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable();
            }

            var protocol = new MuxerProtocol(stream.Object, ownsStream, NullLogger<MuxerProtocol>.Instance);
            await protocol.DisposeAsync();

            stream.Verify();
        }

        /// <summary>
        /// The <see cref="MuxerProtocol"/> constructor checks for <see langword="null"/> values.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("stream", () => new MuxerProtocol(null, ownsStream: true, NullLogger<MuxerProtocol>.Instance));
            Assert.Throws<ArgumentNullException>("logger", () => new MuxerProtocol(Stream.Null, ownsStream: true, null));
        }

        /// <summary>
        /// The <see cref="MuxerProtocol"/> constructor initializes the <see cref="MuxerProtocol.Stream"/> property.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Constructor_InitializesProperties_Async()
        {
            await using var protocol = new MuxerProtocol(Stream.Null, ownsStream: false, NullLogger<MuxerProtocol>.Instance);

            Assert.Equal(Stream.Null, protocol.Stream);
        }

        /// <summary>
        /// The <see cref="MuxerProtocol.WriteMessageAsync(MuxerMessage, CancellationToken)"/> method checks for <see langword="null"/>
        /// values.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task WriteMessageAsync_ValidatesNullArgument_Async()
        {
            await using (var protocol = new MuxerProtocol(Stream.Null, ownsStream: true, NullLogger<MuxerProtocol>.Instance))
            {
                await Assert.ThrowsAsync<ArgumentNullException>("message", () => protocol.WriteMessageAsync(null, default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The <see cref="MuxerProtocol.WriteMessageAsync(MuxerMessage, CancellationToken)"/> method correctly serializes simple messages.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task WriteMessageAsync_SerializesCorrectly_Async()
        {
            await using (MemoryStream stream = new MemoryStream())
            await using (var protocol = new MuxerProtocol(stream, ownsStream: true, NullLogger<MuxerProtocol>.Instance))
            {
                await protocol.WriteMessageAsync(
                    new RequestMessage()
                    {
                        MessageType = MuxerMessageType.ListDevices,
                        BundleID = "com.apple.iTunes",
                        ClientVersionString = "usbmuxd-374.70",
                        ProgName = "iTunes",
                    },
                    default).ConfigureAwait(false);

                var actual = stream.ToArray();
                var expected = File.ReadAllBytes("Muxer/list-request.bin");

                Assert.Equal(
                    expected,
                    actual);
            }
        }

        /// <summary>
        /// Tests the <see cref="MuxerProtocol.ReadMessageAsync(CancellationToken)"/> method for receiving
        /// property list messages.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ReadMessageAsync_CanReadPropertyList_Async()
        {
            await using (Stream stream = File.OpenRead("Muxer/list-response.bin"))
            await using (var protocol = new MuxerProtocol(stream, ownsStream: true, NullLogger<MuxerProtocol>.Instance))
            {
                var value = await protocol.ReadMessageAsync(
                    default).ConfigureAwait(false);

                Assert.IsType<DeviceListMessage>(value);
            }
        }

        /// <summary>
        /// Tests the <see cref="MuxerProtocol.ReadMessageAsync(CancellationToken)"/> method and makes sure
        /// it returns <see langword="null"/> when the header is truncated.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ReadMessageAsync_NoHeader_ReturnsNull_Async()
        {
            await using (Stream stream = new MemoryStream())
            await using (var protocol = new MuxerProtocol(stream, ownsStream: true, NullLogger<MuxerProtocol>.Instance))
            {
                var value = await protocol.ReadMessageAsync(
                    default).ConfigureAwait(false);

                Assert.Null(value);
            }
        }

        /// <summary>
        /// Tests the <see cref="MuxerProtocol.ReadMessageAsync(CancellationToken)"/> method and makes sure
        /// an exception is thrown when a message is received which is not a property list message.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ReadMessageAsync_NotAPropertyListMessage_Throws_Async()
        {
            await using (Stream stream = new MemoryStream())
            await using (var protocol = new MuxerProtocol(stream, ownsStream: true, NullLogger<MuxerProtocol>.Instance))
            {
                byte[] headerData = new byte[MuxerHeader.BinarySize];

                new MuxerHeader()
                {
                    Length = 0x100,
                    Message = MuxerMessageType.Attached,
                    Tag = 1,
                    Version = 1,
                }.Write(headerData);

                stream.Write(headerData);
                stream.Position = 0;

                await Assert.ThrowsAsync<NotSupportedException>(() => protocol.ReadMessageAsync(default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Tests the <see cref="MuxerProtocol.ReadMessageAsync(CancellationToken)"/> method and makes sure
        /// it returns <see langword="null"/> when the message truncated.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ReadMessageAsync_MessageTruncated_ReturnsNull_Async()
        {
            await using (Stream stream = new MemoryStream())
            await using (var protocol = new MuxerProtocol(stream, ownsStream: true, NullLogger<MuxerProtocol>.Instance))
            {
                byte[] headerData = new byte[MuxerHeader.BinarySize];

                new MuxerHeader()
                {
                    Length = 0x100,
                    Message = MuxerMessageType.Plist,
                    Tag = 1,
                    Version = 1,
                }.Write(headerData);

                stream.Write(headerData);
                stream.Position = 0;

                var value = await protocol.ReadMessageAsync(
                    default).ConfigureAwait(false);

                Assert.Null(value);
            }
        }

        /// <summary>
        /// Tests the <see cref="MuxerProtocol.ReadMessageAsync(CancellationToken)"/> method and makes sure
        /// it returns the serialized data.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ReadMessageAsync_Works_Async()
        {
            await using (Stream stream = new MemoryStream())
            await using (var protocol = new MuxerProtocol(stream, ownsStream: true, NullLogger<MuxerProtocol>.Instance))
            {
                var payload = new NSDictionary();
                payload.Add("MessageType", new NSString(nameof(MuxerMessageType.Result)));
                payload.Add("Number", new NSNumber((int)MuxerError.Success));

                byte[] payloadData = Encoding.UTF8.GetBytes(payload.ToXmlPropertyList());
                byte[] headerData = new byte[MuxerHeader.BinarySize];

                new MuxerHeader()
                {
                    Length = (uint)(MuxerHeader.BinarySize + payloadData.Length),
                    Message = MuxerMessageType.Plist,
                    Tag = 1,
                    Version = 1,
                }.Write(headerData);

                stream.Write(headerData);
                stream.Write(payloadData);
                stream.Position = 0;

                var value = await protocol.ReadMessageAsync(
                    default).ConfigureAwait(false);

                Assert.IsType<ResultMessage>(value);
            }
        }
    }
}
