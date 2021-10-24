using Claunia.PropertyList;

namespace MobileDevices.iOS.Lockdown
{




    /// <summary>
    /// Represents a request to change a value, sent from the client to lockdown.
    /// </summary>
    public class ActivateRequest : LockdownMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivateRequest"/> class.
        /// </summary>
        public ActivateRequest()
        {
            this.Request = "Activate";
        }


        /// <summary>
        /// Gets or sets the value to set the ActivationRecord to.
        /// </summary>
        public object ActivationRecord
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public override NSDictionary ToDictionary()
        {
            var dictionary = base.ToDictionary();

            if (this.ActivationRecord != null)
            {
                dictionary.Add(nameof(this.ActivationRecord), this.ActivationRecord);
            }

            return dictionary;
        }
    }
}