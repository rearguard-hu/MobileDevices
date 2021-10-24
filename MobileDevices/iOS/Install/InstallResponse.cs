using System;
using Claunia.PropertyList;
using MobileDevices.iOS.PropertyLists;

namespace MobileDevices.iOS.Install
{
    public class InstallResponse : IPropertyListDeserializable
    {
        public int PercentComplete { get; set; }

        public string Status { get; set; }

        public string Error { get; set; }

        public string ErrorDescription { get; set; }

        public void FromDictionary(NSDictionary data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            Status = data.GetString(nameof(Status));
            Error = data.GetString(nameof(Error));
            ErrorDescription = data.GetString(nameof(ErrorDescription));

            PercentComplete = data.GetNullableInt32(nameof(PercentComplete)) ?? 0;
        }
    }
}