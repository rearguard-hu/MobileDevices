using MobileDevices.iOS.Muxer;
using System;
using Xunit;

namespace MobileDevices.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="MuxerExceptionTests"/> class.
    /// </summary>
    public class MuxerExceptionTests
    {
        /// <summary>
        /// The <see cref="MuxerException()"/> constructor works.
        /// </summary>
        [Fact]
        public void Constructor_Works()
        {
            var ex = new MuxerException();
            Assert.Equal("An unexpected usbmuxd exception occurred.", ex.Message);
        }

        /// <summary>
        /// The <see cref="MuxerException(string)"/> constructor works.
        /// </summary>
        [Fact]
        public void Constructor_WithMessage_Works()
        {
            var ex = new MuxerException("test.");
            Assert.Equal("test.", ex.Message);
        }

        /// <summary>
        /// The <see cref="MuxerException(string, MuxerError)"/> constructor works.
        /// </summary>
        [Fact]
        public void Constructor_WithMessageAndCode_Works()
        {
            var ex = new MuxerException("test.", MuxerError.BadDevice);
            Assert.Equal("test.", ex.Message);
            Assert.Equal((int)MuxerError.BadDevice, ex.HResult);
        }

        /// <summary>
        /// The <see cref="MuxerException(string, MuxerError)"/> constructor works.
        /// </summary>
        [Fact]
        public void Constructor_WithMessageAndInner_Works()
        {
            var inner = new Exception();
            var ex = new MuxerException("test.", inner);
            Assert.Equal("test.", ex.Message);
            Assert.Same(inner, ex.InnerException);
        }
    }
}
