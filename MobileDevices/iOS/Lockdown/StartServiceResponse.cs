namespace MobileDevices.iOS.Lockdown
{
    /// <summary>
    /// Represents a response to the <see cref="StartServiceRequest"/> request.
    /// </summary>
    public partial class StartServiceResponse : LockdownResponse
    {
        /// <summary>
        /// Gets or sets the port at which the service is listening.
        /// </summary>
        public int Port
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the service which was started.
        /// </summary>
        public string Service
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether SSL should be enabled or not.
        /// </summary>
        public bool EnableServiceSSL
        {
            get;
            set;
        }
    }
}
