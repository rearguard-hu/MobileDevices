using Claunia.PropertyList;
using MobileDevices.iOS.Lockdown;
using System;
using Xunit;

namespace MobileDevices.Tests.Lockdown
{
    /// <summary>
    /// Tests the <see cref="PairResponse"/> class.
    /// </summary>
    public class PairResponseTests
    {
        /// <summary>
        /// <see cref="PairResponse.FromDictionary(NSDictionary)"/> validates its arguments.
        /// </summary>
        [Fact]
        public void FromDictionary_ValidatesArguments()
        {
            var response = new PairResponse();
            Assert.Throws<ArgumentNullException>(() => response.FromDictionary(null));
        }

        /// <summary>
        /// <see cref="PairResponse.FromDictionary(NSDictionary)"/> parses the escrow bag.
        /// </summary>
        [Fact]
        public void FromDictionary_WithEscrowBag_Works()
        {
            var dict = new NSDictionary();
            byte[] data = new byte[] { 1, 2, 3, 4 };

            dict.Add("EscrowBag", data);

            var value = new PairResponse();
            value.FromDictionary(dict);

            Assert.Null(value.Error);
            Assert.Equal(data, value.EscrowBag);
        }

        /// <summary>
        /// <see cref="PairResponse.FromDictionary(NSDictionary)"/> parses the error data.
        /// </summary>
        [Fact]
        public void FromDictionary_WithError_Works()
        {
            var dict = new NSDictionary();
            dict.Add("Error", "PairingDialogResponsePending");

            var value = new PairResponse();
            value.FromDictionary(dict);

            Assert.Equal(LockdownError.PairingDialogResponsePending, value.Error);
            Assert.Null(value.EscrowBag);
        }
    }
}
