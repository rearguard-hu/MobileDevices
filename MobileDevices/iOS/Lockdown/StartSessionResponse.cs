using Claunia.PropertyList;

namespace MobileDevices.iOS.Lockdown
{
    /// <summary>
    /// Represents the resopnse to a <see cref="StartSessionRequest"/> request.
    /// </summary>
    public class StartSessionResponse : LockdownResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether SSL should be enabled.
        /// </summary>
        public bool EnableSessionSSL
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a ID which uniquely identifies this session.
        /// </summary>
        public string SessionID
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public override void FromDictionary(NSDictionary data)
        {
            base.FromDictionary(data);

            this.EnableSessionSSL = data.GetNullableBoolean(nameof(this.EnableSessionSSL)) ?? false;
            this.SessionID = data.GetString(nameof(this.SessionID));
        }
    }
}
