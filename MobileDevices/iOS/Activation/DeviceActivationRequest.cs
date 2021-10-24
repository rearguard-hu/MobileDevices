using Claunia.PropertyList;

namespace MobileDevices.iOS.Activation
{
    public class DeviceActivationRequest
    {
        public ActivationClientType ClientType { get; set; }

        public ActivationContentType ContentType { get; set; }

        public string Url { get; set; }

        public NSDictionary Fields { get; set; }

        public bool UseMobileActivation { get; set; }

        public bool SessionMode { get; set; }

        public DeviceActivationRequest()
        {

        }

        public DeviceActivationRequest(ActivationClientType clientType)
        {
            ClientType = clientType;
            ContentType = ActivationContentType.DeviceActivationContentTypeUrlEncoded;
            Url = DeviceActivation.DeviceActivationDefaultUrl;
            Fields = new NSDictionary();
        }

        public DeviceActivationRequest(
            ActivationClientType clientType,
            ActivationContentType contentType,
            NSDictionary fields, string url)
        {
            ClientType = clientType;
            ContentType = contentType;
            Url = url;
            Fields = fields;
        }


    }
}