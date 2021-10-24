using Claunia.PropertyList;
using System;

namespace MobileDevices.iOS.NotificationProxy
{
    /// <summary>
    /// Provides serialization methods.
    /// </summary>
    public partial class NotificationProxyMessage
    {
        /// <summary>
        /// Reads a <see cref="NotificationProxyMessage"/> from a <see cref="NSDictionary"/>.
        /// </summary>
        /// <param name="data">
        /// The message data.
        /// </param>
        /// <returns>
        /// A <see cref="NotificationProxyMessage"/> object.
        /// </returns>
        public static NotificationProxyMessage Read(NSDictionary data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return new NotificationProxyMessage()
            {
                Command = data.GetString(nameof(Command)),
                Name = data.GetString(nameof(Name)),
            };
        }
    }
}
