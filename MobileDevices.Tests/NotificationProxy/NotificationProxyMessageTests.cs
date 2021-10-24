using Claunia.PropertyList;
using MobileDevices.iOS.NotificationProxy;
using System;
using Xunit;

namespace MobileDevices.Tests.NotificationProxy
{
    /// <summary>
    /// Tests the <see cref="NotificationProxyMessage"/> class.
    /// </summary>
    public class NotificationProxyMessageTests
    {
        /// <summary>
        /// <see cref="NotificationProxyMessage.Read(NSDictionary)"/> validates its arguments.
        /// </summary>
        [Fact]
        public void ReadAsync_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => NotificationProxyMessage.Read(null));
        }

        /// <summary>
        /// <see cref="NotificationProxyMessage.Read(NSDictionary)"/> works.
        /// </summary>
        [Fact]
        public void Read_Works()
        {
            var dict = new NSDictionary();
            dict.Add("Command", "RelayNotification");
            dict.Add("Name", "com.apple.mobile.application_installed");

            var value = NotificationProxyMessage.Read(dict);

            Assert.Equal("RelayNotification", value.Command);
            Assert.Equal("com.apple.mobile.application_installed", value.Name);
        }

        /// <summary>
        /// <see cref="NotificationProxyMessage.ToPropertyList"/> works.
        /// </summary>
        [Fact]
        public void ToPropertyList_Works()
        {
            var value = new NotificationProxyMessage()
            {
                Command = "RelayNotification",
                Name = "com.apple.mobile.application_installed",
            };

            var dict = value.ToPropertyList();

            Assert.Collection(
                dict,
                (v) =>
                {
                    Assert.Equal("Command", v.Key);
                    Assert.Equal("RelayNotification", v.Value.ToObject());
                },
                (v) =>
                {
                    Assert.Equal("Name", v.Key);
                    Assert.Equal("com.apple.mobile.application_installed", v.Value.ToObject());
                });
        }
    }
}
