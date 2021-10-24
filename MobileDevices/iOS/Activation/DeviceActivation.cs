using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Claunia.PropertyList;
using MobileDevices.iOS.Lockdown;

namespace MobileDevices.iOS.Activation
{
    public class DeviceActivation
    {
        public const string DeviceActivationUserAgentIos = "iOS Device Activator (MobileActivation-20 built on Jan 15 2012 at 19:07:28)";

        public const string DeviceActivationUserAgentItunes = "iTunes/11.1.4 (Macintosh; OS X 10.9.1) AppleWebKit/537.73.11";

        public const string DeviceActivationDefaultUrl = "https://albert.apple.com/deviceservices/deviceActivation";

        public const string DeviceActivationDrmHandshakeDefaultUrl = "https://albert.apple.com/deviceservices/drmHandshake";

        private readonly ClientFactory<LockdownClient> factory;

        private readonly HttpClient httpClient;

        public DeviceContext Context;

        public DeviceActivation(ClientFactory<LockdownClient> lockDownClientFactory, DeviceContext context)
        {
            this.factory = lockDownClientFactory ?? throw new ArgumentNullException(nameof(lockDownClientFactory));
            this.Context = context;
            httpClient = new HttpClient();
        }

        public void ActivationParseRawResponse(DeviceActivationResponse response)
        {
            switch (response.ContentType)
            {
                case ActivationContentType.DeviceActivationContentTypeHtml:
                    ActivationParseHtmlResponse(response);
                    break;
                case ActivationContentType.DeviceActivationContentTypeBuddyml:
                    ActivationParseBuddymlResponse(response);
                    break;
                case ActivationContentType.DeviceActivationContentTypePlist:
                    ActivationParsePlistResponse(response);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool ActivationParsePlistResponse(DeviceActivationResponse response)
        {
            var plist = XmlPropertyListParser.ParseString(response.RawContent);
            var result = false;
            if (plist is null)
                return result;

            var dic = (NSDictionary)plist;
            
            if (dic.TryGetValue("HandshakeResponseMessage", out var handshakeMessage))
            {
                result = true;
            }
            else
            {
                result = ActivationRecordFromPlist(response, dic);
            }
            response.Fields = dic;

            return result;

        }

        private void ParsePlistResponse(DeviceActivationResponse response)
        {
            var plist = (NSDictionary)XmlPropertyListParser.ParseString(response.RawContent);

            if (plist.ContainsKey("HandshakeResponseMessage"))
            {
                ActivationRecordFromPlist(response, plist);
            }

            response.Fields = plist;
        }

        /// <summary>
        /// 从响应加载激活记录
        /// </summary>
        /// <param name="response"></param>
        /// <param name="plist"></param>
        /// <returns></returns>
        public bool ActivationRecordFromPlist(DeviceActivationResponse response, NSDictionary plist)
        {
            var record = plist.GetDict("ActivationRecord");
            if (record != null)
            {
                var ackReceived = record.GetNullableBoolean("ack-received") ?? false;
                if (ackReceived)
                    response.IsActivationAck = 1;

                response.ActivationRecord = new NSData(Encoding.UTF8.GetBytes(response.RawContent));
            }
            else
            {

                var activationNode = plist.GetDict("iphone-activation") ?? plist.GetDict("iphone-activation");
                if (activationNode is null)
                    return false;
                // activationNode ?? NULL

                var ackReceived = activationNode.GetNullableBoolean("ack-received") ?? false;
                if (ackReceived)
                    response.IsActivationAck = 1;

                if (activationNode.TryGetValue("activation-record", out var recordData))
                    response.ActivationRecord = recordData;

            }

            return true;
        }

        public bool ActivationParseHtmlResponse(DeviceActivationResponse response)
        {
            if (response.ContentType != ActivationContentType.DeviceActivationContentTypeHtml)
                return false;

            var xml = new XmlDocument();
            xml.LoadXml(response.RawContent);

            var xpathResult = xml.SelectNodes("//input[@name='isAuthRequired' and @value='true']");
            if (xpathResult is null)
                return false;

            if (xpathResult.Count>0)
                response.IsAuthRequired = 1;

            xpathResult = xml.SelectNodes("//script[@type='text/x-apple-plist']/plist");
            if (xpathResult is null)
                return false;
            if (xpathResult.Count <= 0) return false;

            var value=xpathResult[0]?.Value;
            if (string.IsNullOrEmpty(value)) return true;

            var dic = (NSDictionary)XmlPropertyListParser.ParseString(value);
            ActivationRecordFromPlist(response, dic);

            return true;
        }

        public bool ActivationParseBuddymlResponse(DeviceActivationResponse response)
        {
            if (response.ContentType != ActivationContentType.DeviceActivationContentTypeBuddyml)
                return false;

            var xml = new XmlDocument();
            xml.LoadXml(response.RawContent);

            var xpathResult = xml.SelectNodes("/xmlui/navigationBar/@title");

            if (xpathResult is null)
                return false;
            if (xpathResult.Count>0)
            {
                var title=xpathResult[0]?.Value;
                if (!string.IsNullOrEmpty(title))
                {
                    response.Title = title;
                    response.HasErrors = 1;
                }
            }

            xpathResult = xml.SelectNodes("/xmlui/clientInfo[@ack-received='true']");

            if (xpathResult is null)
                return false;

            if (xpathResult.Count > 0)
            {
                response.IsActivationAck = 1;
                return true;
            }

            xpathResult = xml.SelectNodes("/xmlui/alert/@title");
            if (xpathResult is null)
                return false;

            if (xpathResult.Count > 0)
            {
                var title = xpathResult[0]?.Value;

                if (!string.IsNullOrEmpty(title))
                    response.Title = title;
            }
            else
            {

                xpathResult = xml.SelectNodes("/xmlui/page/navigationBar/@title");
                if (xpathResult is null)
                    return false;

                var title = xpathResult[0]?.Value;

                if (!string.IsNullOrEmpty(title))
                    response.Title = title;

            }

            xpathResult = xml.SelectNodes("/xmlui/page/tableView/section/footer[not (@url)]");
            if (xpathResult is null)
                return false;

            if (xpathResult.Count<=0)
            {
                xpathResult = xml.SelectNodes("/xmlui/page/tableView/section[@footer and not(@footerLinkURL)]/@footer");
                return false;
            }

            if (xpathResult !=null)
            {
                var responseDescription = new StringBuilder();
                foreach (XmlElement item in xpathResult)
                {
                    responseDescription.Append(item.Value);
                }

                if (responseDescription.Length>0)
                    response.Description = responseDescription.ToString();
            }

            xpathResult = xml.SelectNodes("/xmlui/page//editableTextRow");
            if (xpathResult is null)
                return false;
            foreach (XmlElement node in xpathResult)
            {
                var id=node.GetAttribute("id");
                if (string.IsNullOrEmpty(id))
                    return false;

                var secure= node.GetAttribute("secure");

                var secureInput = secure.Equals("true", StringComparison.CurrentCultureIgnoreCase) ? 1 : 0;
                ResponseAddField(response, id, "", 1, secureInput);

                var label= node.GetAttribute("label");
                if (!string.IsNullOrEmpty(label))
                {
                    if (response.Labels.TryAdd(id, new NSString(label)))
                        response.Labels[id] = new NSString(label);
                }

                var placeholder = node.GetAttribute("placeholder");
                if (!string.IsNullOrEmpty(placeholder))
                {
                    if (response.LabelsPlaceholder.TryAdd(id, new NSString(placeholder)))
                        response.LabelsPlaceholder[id] = new NSString(placeholder);
                }

            }

            xpathResult = xml.SelectNodes("/xmlui/serverInfo/@*");
            if (xpathResult is null)
                return false;
            foreach (XmlElement node in xpathResult)
            {
                if (string.IsNullOrEmpty(node.Value)) continue;
                if (node.Name== "isAuthRequired")
                {
                    response.IsAuthRequired = 1;
                }

                if (response.Fields.TryAdd(node.Name, new NSString(node.Value)))
                    response.Fields[node.Name] = new NSString(node.Value);
            }

            return true;
        }

        public void ResponseAddField(DeviceActivationResponse response, string key, string value, int requiredInput,
            int secureInput)
        {
            if (response.Fields.TryAdd(key, new NSString(value)))
                response.Fields[key] = new NSString(value);
            if (requiredInput==1)
            {
                if (response.FieldsRequireInput.TryAdd(key, NSObject.Wrap(1)))
                    response.Fields[key] = NSObject.Wrap(1);
            }

            if (secureInput != 1) return;

            if (response.FieldsSecureInput.TryAdd(key, NSObject.Wrap(1)))
                response.Fields[key] = NSObject.Wrap(1);

        }

        public void ActivationRequestSetFieldsFromResponse(DeviceActivationRequest request, DeviceActivationResponse response)
        {
            if (request is null || response is null)
                return;
            if (response.Fields is null)
                return;

            ActivationRequestSetFields(request, response.Fields);
        }

        public void ActivationRequestSetFields(DeviceActivationRequest request, NSDictionary fields)
        {
            if (fields is null)
                return;

            if (request.ContentType == ActivationContentType.DeviceActivationContentTypeUrlEncoded)
            {
                var type = typeof(NSString);
                var count = fields.Values.Count(x => x?.GetType() != type);
                if (count > 0)
                    request.ContentType = ActivationContentType.DeviceActivationContentTypeMultipartFormData;
            }

            foreach (var (key, value) in fields)
            {
                if (request.Fields.ContainsKey(key))
                {
                    request.Fields[key] = value;
                }
                else
                {
                    request.Fields.Add(key, value);
                }
            }
        }

        public DeviceActivationRequest ActivationDrmHandshakeRequestNew(ActivationClientType clientType)
        {
            var request = new DeviceActivationRequest(
                clientType,
                ActivationContentType.DeviceActivationContentTypePlist,
                new NSDictionary(),
                DeviceActivationDrmHandshakeDefaultUrl);

            return request;
        }

        public async Task<DeviceActivationRequest> ActivationRequestNewFromLockDownAsync(ActivationClientType clientType, CancellationToken cancellationToken)
        {
            await using var lockDown = await this.factory.CreateAsync(cancellationToken);
            var session = await lockDown.StartSessionAsync(Context.PairingRecord, cancellationToken).ConfigureAwait(false);

            var value = await lockDown.GetValueAsync<GetValueAll>(null, null, cancellationToken);
            var info = value.Value;
            var fields = new NSDictionary
            {
                {"InStoreActivation", false},
                {"AppleSerialNumber", info.GetString("SerialNumber")}
            };

            var telephonyCapability = info.GetNullableBoolean("TelephonyCapability") ?? false;

            if (telephonyCapability)
            {
                fields.Add("IMEI", info.GetString("InternationalMobileEquipmentIdentity"));
                fields.Add("MEID", info.GetString("MobileEquipmentIdentifier"));
                fields.Add("IMSI", info.GetString("InternationalMobileSubscriberIdentity"));
                fields.Add("ICCID", info.GetString("IntegratedCircuitCardIdentity"));

            }


            var activationInfo = await lockDown.GetValueAsync<GetValueAll>(null, "ActivationInfo", cancellationToken);

            var activationValue = activationInfo.Value;
            if (activationValue is null)
                return null;

            fields.Add("activation-info", activationValue);


            var request = new DeviceActivationRequest(
                clientType,
                ActivationContentType.DeviceActivationContentTypeMultipartFormData,
                fields,
                DeviceActivationDefaultUrl);

            if (session != null)
                await lockDown.StopSessionAsync(session.SessionID, cancellationToken).ConfigureAwait(false);

            return request;

        }

        public async Task<DeviceActivationResponse> ActivationSendRequestAsync(DeviceActivationRequest request, CancellationToken token)
        {
            var response = new DeviceActivationResponse();

            httpClient.DefaultRequestHeaders.UserAgent.Clear();
            switch (request.ClientType)
            {
                case ActivationClientType.DeviceActivationClientMobileActivation:
                    httpClient.DefaultRequestHeaders.Add("User-Agent", DeviceActivationUserAgentIos);
                    break;
                case ActivationClientType.DeviceActivationClientItunes:
                    httpClient.DefaultRequestHeaders.Add("User-Agent", DeviceActivationUserAgentItunes);
                    break;
            }

            HttpContent httpContent = null;
            if (request.ContentType== ActivationContentType.DeviceActivationContentTypeMultipartFormData)
            {
                var postContent = new MultipartFormDataContent();
                foreach (var (key,value) in request.Fields)
                {
                    if (value is NSString strValue)
                    {
                        postContent.Add(new StringContent(strValue.ToString()), key);
                    }
                    else
                    {
                        postContent.Add(new StringContent(value.ToXmlPropertyList()), key);
                    }
                }

                httpContent = postContent;
            }
            else if (request.ContentType == ActivationContentType.DeviceActivationContentTypeUrlEncoded)
            {
                var values = new Dictionary<string, string>();

                foreach (var (key, value) in request.Fields)
                {
                    if (value == null) continue;

                    if (value is NSString strValue)
                    {
                        values.Add(key, strValue.ToString());
                    }
                    else
                    {
                        //only strings supported
                    }
                }

                httpContent = new FormUrlEncodedContent(values);
            }
            else if (request.ContentType == ActivationContentType.DeviceActivationContentTypePlist)
            {
                var stringContent = new StringContent(request.Fields.ToXmlPropertyList());
                stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-apple-plist");
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                httpContent = stringContent;
            }

            if (httpContent==null)
                return response;

            var httpResponse = await httpClient.PostAsync(request.Url, httpContent, token);

            response = await ParseRawResponseAsync(httpResponse);
            ActivationParseRawResponse(response);

            return response;
        }

        public async Task<DeviceActivationResponse> ParseRawResponseAsync(HttpResponseMessage httpResponse)
        {
            var response = new DeviceActivationResponse();

            if (httpResponse.Content.Headers.TryGetValues("Content-Type",out var values))
            {
                var value=values.FirstOrDefault();
                if (value== "text/xml")
                    response.ContentType = ActivationContentType.DeviceActivationContentTypePlist;
                else if (value == "application/xml")
                    response.ContentType = ActivationContentType.DeviceActivationContentTypePlist;
                else if (value == "application/x-buddyml")
                    response.ContentType = ActivationContentType.DeviceActivationContentTypeBuddyml;
                else if (value == "text/html")
                    response.ContentType = ActivationContentType.DeviceActivationContentTypeHtml;
            }

            foreach (var (key,value) in httpResponse.Headers)
            {
                response.Headers.Add(key, value.FirstOrDefault());
            }

            response.RawContent = await httpResponse.Content.ReadAsStringAsync();
            response.RawContentSize = (ulong)response.RawContent.Length;

            return response;
        }
    }
}