using Claunia.PropertyList;

namespace MobileDevices.iOS.Activation
{
    public class DeviceActivationResponse
    {
        public string RawContent { get; set; }

        public ulong RawContentSize { get; set; }

        public ActivationContentType ContentType { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public NSObject ActivationRecord { get; set; }

        public NSDictionary Headers { get; set; }
        public NSDictionary Fields { get; set; }
        public NSDictionary FieldsRequireInput { get; set; }
        public NSDictionary FieldsSecureInput { get; set; }
        public NSDictionary Labels { get; set; }
        public NSDictionary LabelsPlaceholder { get; set; }
        public int IsActivationAck { get; set; }
        public int IsAuthRequired { get; set; }
        public int HasErrors { get; set; }

        public bool ActivationSuccess;

        public DeviceActivationResponse()
        {
            ContentType = ActivationContentType.DeviceActivationContentTypeUnknown;
            Headers = new NSDictionary();
            Fields = new NSDictionary();
            FieldsRequireInput = new NSDictionary();
            FieldsSecureInput = new NSDictionary();
            Labels = new NSDictionary();
            LabelsPlaceholder = new NSDictionary();
        }

    }
}