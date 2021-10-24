using Claunia.PropertyList;

namespace MobileDevices.iOS.Lockdown
{
    /// <content>
    /// Methods for deserializing the <see cref="StartServiceResponse"/>.
    /// </content>
    public partial class StartServiceResponse
    {
        /// <inheritdoc/>
        public override void FromDictionary(NSDictionary data)
        {
            base.FromDictionary(data);

            this.EnableServiceSSL = data.GetNullableBoolean(nameof(this.EnableServiceSSL)) ?? false;

            this.Port = data.GetNullableInt32(nameof(this.Port)) ?? 0;
            this.Service = data.GetString(nameof(this.Service));
        }
    }
}
