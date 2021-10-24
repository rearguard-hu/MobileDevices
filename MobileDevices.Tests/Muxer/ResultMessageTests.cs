using Claunia.PropertyList;
using MobileDevices.iOS.Muxer;
using System;
using Xunit;

namespace MobileDevices.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="ResultMessage"/> class.
    /// </summary>
    public class ResultMessageTests
    {
        /// <summary>
        /// <see cref="ResultMessage.Read(NSDictionary)"/> throws an error when passed
        /// <see langword="null"/> values.
        /// </summary>
        [Fact]
        public void Read_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("data", () => ResultMessage.Read(null));
        }

        /// <summary>
        /// <see cref="ResultMessage.Read(NSDictionary)"/> correctly parses a <see cref="NSDictionary"/>
        /// object.
        /// </summary>
        [Fact]
        public void Read_Works()
        {
            var result = ResultMessage.Read(
                (NSDictionary)PropertyListParser.Parse("Muxer/result.xml"));

            Assert.Equal(MuxerMessageType.Result, result.MessageType);
            Assert.Equal(MuxerError.Success, result.Number);
        }
    }
}
