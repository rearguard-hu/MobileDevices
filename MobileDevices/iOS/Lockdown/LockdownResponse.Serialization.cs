using Claunia.PropertyList;
using Microsoft;
using System;

namespace MobileDevices.iOS.Lockdown
{
    /// <content>
    /// Serialization methods for <see cref="LockdownResponse"/>.
    /// </content>
    public partial class LockdownResponse
    {
        /// <summary>
        /// Reads a <see cref="LockdownResponse"/> from a <see cref="NSDictionary"/>.
        /// </summary>
        /// <param name="data">
        /// The message data.
        /// </param>
        public virtual void FromDictionary(NSDictionary data)
        {
            Requires.NotNull(data, nameof(data));

            this.Request = data.GetString(nameof(this.Request));
            this.Result = data.GetString(nameof(this.Result));

            var errorValue = data.GetString(nameof(this.Error));
            if (errorValue is null)
            {
                this.Error = null;
                return;
            }

            this.Error = Enum.TryParse<LockdownError>(errorValue, out var error) ? error : LockdownError.Unknown;

        }
    }
}
