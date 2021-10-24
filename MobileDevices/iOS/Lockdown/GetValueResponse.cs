namespace MobileDevices.iOS.Lockdown
{
    /// <summary>
    /// A response to a <see cref="GetValueRequest"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value.
    /// </typeparam>
    public partial class GetValueResponse<T> : LockdownResponse
    {
        /// <summary>
        /// Gets or sets the name of the domain of the value which is being returned.
        /// </summary>
        public string Domain
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the value which being returned.
        /// </summary>
        public string Key
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the response.
        /// </summary>
        public string Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the response value.
        /// </summary>
        public T Value
        {
            get;
            set;
        }
    }
}
