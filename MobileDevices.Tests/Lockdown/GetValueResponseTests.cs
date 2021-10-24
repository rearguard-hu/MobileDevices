using Claunia.PropertyList;
using MobileDevices.iOS.Lockdown;
using System;
using Xunit;

namespace MobileDevices.Tests.Lockdown
{
    /// <summary>
    /// Tests the <see cref="GetValueResponse{T}"/> class.
    /// </summary>
    public class GetValueResponseTests
    {
        /// <summary>
        /// <see cref="GetValueResponse{T}.FromDictionary(NSDictionary)"/> validates its arguments.
        /// </summary>
        [Fact]
        public void Read_ValidatesArguments()
        {
            var response = new GetValueResponse<string>();
            Assert.Throws<ArgumentNullException>(() => response.FromDictionary(null));
        }

        /// <summary>
        /// Tests the <see cref="GetValueResponse{T}.FromDictionary(NSDictionary)"/> method.
        /// </summary>
        [Fact]
        public void Read_Works()
        {
            var dict = new NSDictionary();
            dict.Add("Request", "QueryType");
            dict.Add("Result", "Success");
            dict.Add("Type", "com.apple.mobile.lockdown");

            var response = new GetValueResponse<string>();
            response.FromDictionary(dict);

            Assert.Equal("QueryType", response.Request);
            Assert.Equal("Success", response.Result);
            Assert.Equal("com.apple.mobile.lockdown", response.Type);
        }
    }
}
