using Claunia.PropertyList;
using MobileDevices.iOS.Lockdown;
using System;
using Xunit;

namespace MobileDevices.Tests.Lockdown
{
    /// <summary>
    /// Tests the <see cref="StartSessionResponse"/> class.
    /// </summary>
    public class StartSessionResponseTests
    {
        /// <summary>
        /// <see cref="StartSessionResponse.FromDictionary(NSDictionary)"/> validates its arguments.
        /// </summary>
        [Fact]
        public void FromDictionary_ValidatesArguments()
        {
            var response = new StartSessionResponse();
            Assert.Throws<ArgumentNullException>(() => response.FromDictionary(null));
        }

        /// <summary>
        /// Tests the <see cref="StartSessionResponse.FromDictionary(NSDictionary)"/> method.
        /// </summary>
        [Fact]
        public void FromDictionary_Works()
        {
            var dict = new NSDictionary();
            dict.Add("Request", "StartSession");
            dict.Add("SessionID", "abc");

            var response = new StartSessionResponse();
            response.FromDictionary(dict);

            Assert.Equal("StartSession", response.Request);
            Assert.False(response.EnableSessionSSL);
            Assert.Equal("abc", response.SessionID);
            Assert.Null(response.Error);
        }

        /// <summary>
        /// Tests the <see cref="StartSessionResponse.FromDictionary(NSDictionary)"/> method.
        /// </summary>
        [Fact]
        public void FromDictionary_WithSSL_Works()
        {
            var dict = new NSDictionary();
            dict.Add("Request", "StartSession");
            dict.Add("EnableSessionSSL", true);
            dict.Add("SessionID", "abc");

            var response = new StartSessionResponse();
            response.FromDictionary(dict);

            Assert.Equal("StartSession", response.Request);
            Assert.True(response.EnableSessionSSL);
            Assert.Equal("abc", response.SessionID);
            Assert.Null(response.Error);
        }

        /// <summary>
        /// Tests the <see cref="StartSessionResponse.FromDictionary(NSDictionary)"/> method.
        /// </summary>
        [Fact]
        public void FromDictionary_WithError_Works()
        {
            var dict = new NSDictionary();
            dict.Add("Request", "StartSession");
            dict.Add("Error", "SessionInactive");

            var response = new StartSessionResponse();
            response.FromDictionary(dict);

            Assert.Equal("StartSession", response.Request);
            Assert.Equal(LockdownError.SessionInactive, response.Error);
        }
    }
}
