using Claunia.PropertyList;

namespace MobileDevices.iOS.Lockdown
{
    /// <summary>
    /// Represents a request to start a service.
    /// </summary>
    public class StartServiceRequest : LockdownMessage
    {
        /// <summary>
        /// Gets or sets the name of the service to start.
        /// </summary>
        public string Service
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public override NSDictionary ToDictionary()
        {
            var dict = base.ToDictionary();
            dict.Add(nameof(this.Service), this.Service);
            return dict;
        }
    }
}
