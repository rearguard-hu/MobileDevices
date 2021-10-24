using MobileDevices.iOS.Muxer;
using Xunit;

namespace MobileDevices.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="MuxerHeader"/> struct.
    /// </summary>
    public class MuxerHeaderTests
    {
        /// <summary>
        /// Makes sure a <see cref="MuxerHeader"/> object roundtrips successfully.
        /// </summary>
        [Fact]
        public void RoundtripTest()
        {
            var header = new MuxerHeader()
            {
                Length = 101,
                Message = MuxerMessageType.ReadBUID,
                Tag = 102,
                Version = 103,
            };

            var data = new byte[MuxerHeader.BinarySize];
            header.Write(data);

            var clone = MuxerHeader.Read(data);

            Assert.Equal(101u, clone.Length);
            Assert.Equal(MuxerMessageType.ReadBUID, clone.Message);
            Assert.Equal(102u, clone.Tag);
            Assert.Equal(103u, clone.Version);
        }
    }
}
