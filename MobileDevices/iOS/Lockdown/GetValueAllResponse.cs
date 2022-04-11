using Claunia.PropertyList;

namespace MobileDevices.iOS.Lockdown
{
    public class GetValueAllResponse : LockdownResponse
    {
        public NSDictionary Value;

        public override void FromDictionary(NSDictionary data)
        {
            base.FromDictionary(data);

            Request = data.GetString(nameof(Request));

            Value = data.GetDict(nameof(Value));
        }
    }
}