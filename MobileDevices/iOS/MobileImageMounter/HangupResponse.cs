using Claunia.PropertyList;

namespace MobileDevices.iOS.MobileImageMounter
{
    /// <summary>
    /// A response from the remote end, confirming the connection has been terminated.
    /// </summary>
    public class HangupResponse : MobileImageMounterResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the remote end has acknowledged the terminate request.
        /// </summary>
        public bool Goodbye { get; set; }

        /// <inheritdoc/>
        public override void FromDictionary(NSDictionary dictionary)
        {
            base.FromDictionary(dictionary);
            this.Goodbye = dictionary.GetBoolean(nameof(this.Goodbye));
        }
    }
}
