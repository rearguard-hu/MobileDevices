using System;
using Claunia.PropertyList;

namespace MobileDevices.iOS.Activation
{
    public class MobileActivationResponse
    {
        public NSObject Value { get; set; }

        /// <summary>
        /// Reads a <see cref="MobileActivationResponse"/> from a <see cref="NSDictionary"/>.
        /// </summary>
        /// <param name="dict">
        /// The message data.
        /// </param>
        /// <returns>
        /// A <see cref="MobileActivationResponse"/> object.
        /// </returns>
        public static MobileActivationResponse Read(NSDictionary dict)
        {
            if (dict == null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            var value = new MobileActivationResponse();

            if (dict.TryGetValue(nameof(Value), out var sObject))
            {
                value.Value = sObject;
            }

            return value;
        }
    }
}