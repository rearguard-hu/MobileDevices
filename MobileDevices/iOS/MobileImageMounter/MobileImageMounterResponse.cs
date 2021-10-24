using Claunia.PropertyList;
using MobileDevices.iOS.PropertyLists;
using Microsoft;
using System;

namespace MobileDevices.iOS.MobileImageMounter
{
    /// <summary>
    /// Respresents the response of a image mounter request.
    /// </summary>
    public class MobileImageMounterResponse : IPropertyListDeserializable
    {
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public MobileImageMounterStatus? Status
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the response error.
        /// </summary>
        public MobileImageMounterError? Error
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the detailed error.
        /// </summary>
        public string DetailedError
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public virtual void FromDictionary(NSDictionary dictionary)
        {
            Requires.NotNull(dictionary, nameof(dictionary));

            var statusValue = dictionary.GetString(nameof(this.Status));
            this.Status = statusValue == null ? null : Enum.Parse<MobileImageMounterStatus>(statusValue);
            var errorValue = dictionary.GetString(nameof(this.Error));
            this.Error = errorValue == null ? null : Enum.Parse<MobileImageMounterError>(errorValue);
            this.DetailedError = dictionary.GetString(nameof(this.DetailedError));
        }
    }
}
