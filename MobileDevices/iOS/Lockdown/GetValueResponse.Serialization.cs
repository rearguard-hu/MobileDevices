using Claunia.PropertyList;

namespace MobileDevices.iOS.Lockdown
{
    /// <content>
    /// Serialization methods for <see cref="GetValueResponse{T}"/>.
    /// </content>
    public partial class GetValueResponse<T>
    {
        /// <inheritdoc/>
        public override void FromDictionary(NSDictionary data)
        {
            base.FromDictionary(data);

            this.Domain = data.GetString(nameof(this.Domain));
            this.Key = data.GetString(nameof(this.Key));
            this.Type = data.GetString(nameof(this.Type));

            if (data.ContainsKey(nameof(this.Value)))
            {
                this.Value = (T)data.Get(nameof(this.Value))?.ToObject();
            }
        }
    }
}
