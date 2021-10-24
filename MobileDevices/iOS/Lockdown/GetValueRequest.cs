using Claunia.PropertyList;

namespace MobileDevices.iOS.Lockdown
{
    /// <summary>
    /// Represents a request for a value, sent from the client to lockdown.
    /// </summary>
    public class GetValueRequest : LockdownMessage
    {
        /// <summary>
        /// Gets or sets the domain of the value to get.
        /// </summary>
        public string Domain
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the value to get.
        /// </summary>
        public string Key
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public override NSDictionary ToDictionary()
        {
            var dictionary = base.ToDictionary();

            if (this.Domain != null)
            {
                dictionary.Add(nameof(this.Domain), this.Domain);
            }

            if (Key != null)
            {
                dictionary.Add(nameof(Key), Key);
            }

            return dictionary;
        }
    }
}
