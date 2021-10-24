using MobileDevices.iOS.Lockdown;
using Xunit;

namespace MobileDevices.Tests.Lockdown
{
    /// <summary>
    /// Tests the <see cref="LockdownException"/> class.
    /// </summary>
    public class LockdownExceptionTests
    {
        /// <summary>
        /// Tests the <see cref="LockdownException"/> constructor.
        /// </summary>
        [Fact]
        public void Constructor_Works()
        {
            var ex = new LockdownException();
            Assert.Equal("An unknown lockdown error occurred", ex.Message);

            ex = new LockdownException("Hello");
            Assert.Equal("Hello", ex.Message);
        }

        /// <summary>
        /// The <see cref="LockdownException(LockdownError)"/> constructor works correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithError_Works()
        {
            var ex = new LockdownException(LockdownError.GetProhibited);

            Assert.Equal(LockdownError.GetProhibited, ex.Error);
            Assert.Equal(LockdownError.GetProhibited, (LockdownError)ex.HResult);
        }
    }
}
