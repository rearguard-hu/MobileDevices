using Claunia.PropertyList;

namespace MobileDevices.iOS.Lockdown
{
    /// <summary>
    /// Represents a request to stop a lockdown session.
    /// </summary>
    public class StopSessionRequest : LockdownMessage
    {
        /// <summary>
        /// Gets or sets the ID of the session to stop.
        /// </summary>
        public string SessionID
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public override NSDictionary ToDictionary()
        {
            var dict = base.ToDictionary();
            dict.Add(nameof(this.SessionID), this.SessionID);
            return dict;
        }
    }
}
