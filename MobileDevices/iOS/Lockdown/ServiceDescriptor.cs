namespace MobileDevices.iOS.Lockdown
{
    /// <summary>
    /// Represents a service running on an iOS device.
    /// </summary>
    public class ServiceDescriptor
    {
        /// <summary>
        /// Gets or sets the port on which the service is listening.
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SSL should be enabled for this service, or not.
        /// </summary>
        public bool EnableServiceSSL { get; set; }

        /// <summary>
        /// Gets or sets the service name. For informational purposes only.
        /// </summary>
        public string ServiceName { get; set; }
    }
}
