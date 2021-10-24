using Claunia.PropertyList;

namespace MobileDevices.iOS.Lockdown
{
    /// <summary>
    /// Represents a request to change a value, sent from the client to lockdown.
    /// </summary>
    public class SetValueRequest : LockdownMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetValueRequest"/> class.
        /// </summary>
        public SetValueRequest()
        {
            this.Request = "SetValue";
        }

        /// <summary>
        /// Gets or sets the domain of the value to set.
        /// </summary>
        public string Domain
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the value to set.
        /// </summary>
        public string Key
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value to set the key to.
        /// </summary>
        public object Value
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

            dictionary.Add(nameof(this.Key), this.Key);
            dictionary.Add(nameof(this.Value), this.Value);
            return dictionary;
        }
    }
}
