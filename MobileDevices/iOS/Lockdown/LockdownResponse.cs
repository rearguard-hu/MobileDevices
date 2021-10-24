using MobileDevices.iOS.PropertyLists;

namespace MobileDevices.iOS.Lockdown
{
    /// <summary>
    /// Represents a response sent from lockdown running on the device to the PC. This class is generic, and not
    /// all properties may have values.
    /// </summary>
    public partial class LockdownResponse : IPropertyListDeserializable
    {
        /// <summary>
        /// Gets or sets the name of the request to which a value is being sent.
        /// </summary>
        public string Request
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a string which represents the result (success, failure,...) of the operation.
        /// </summary>
        public string Result
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public LockdownError? Error
        {
            get;
            set;
        }
    }
}
