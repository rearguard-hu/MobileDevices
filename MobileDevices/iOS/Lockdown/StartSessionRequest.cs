using Claunia.PropertyList;

namespace MobileDevices.iOS.Lockdown
{
    /// <summary>
    /// Represents a message which requests the lockdown service on the device to start a new session.
    /// </summary>
    public class StartSessionRequest : LockdownMessage
    {
        /// <summary>
        /// Gets or sets the Host ID of this computer, as stored in <see cref="PairingRecord.HostId"/>.
        /// </summary>
        public string HostID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the System BUID of this computer, as stored in <see cref="PairingRecord.SystemBUID"/>.
        /// </summary>
        public string SystemBUID
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public override NSDictionary ToDictionary()
        {
            var dict = base.ToDictionary();
            dict.Add(nameof(this.HostID), this.HostID);
            dict.Add(nameof(this.SystemBUID), this.SystemBUID);
            return dict;
        }
    }
}
