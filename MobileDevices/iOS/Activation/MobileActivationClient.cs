using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Claunia.PropertyList;
using Microsoft.Extensions.Logging;
using MobileDevices.iOS.DiagnosticsRelay;
using MobileDevices.iOS.PropertyLists;

namespace MobileDevices.iOS.Activation
{
    public class MobileActivationClient : IAsyncDisposable
    {
        /// <summary>
        /// Gets the name of the mobile activation service running on the device.
        /// </summary>
        public const string ServiceName = "com.apple.mobileactivationd";

        private readonly PropertyListProtocol protocol;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticsRelayClient"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents a connection to the mobile activation service running on the device.
        /// </param>
        /// <param name="logger">
        /// A logger which can be used when logging.
        /// </param>
        public MobileActivationClient(Stream stream, ILogger<MobileActivationClient> logger)
        {
            this.protocol = new PropertyListProtocol(stream, ownsStream: true, logger: logger);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MobileActivationClient"/> class.
        /// </summary>
        /// <param name="protocol">
        /// A <see cref="PropertyListProtocol"/> which represents a connection to the mobile activation service running on the device.
        /// </param>
        public MobileActivationClient(PropertyListProtocol protocol)
        {
            this.protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            return this.protocol.DisposeAsync();
        }

        public async Task<NSObject> GetActivationStateAsync(CancellationToken cancellationToken)
        {
            var response = await this.ExecuteRequestAsync(
                new MobileActivationRequest
                {
                    Command = "GetActivationStateRequest"
                },
                cancellationToken);

            return response?.Value;
        }

        public async Task<NSObject> CreateActivationSessionInfoAsync(CancellationToken cancellationToken)
        {
            var response = await this.ExecuteRequestAsync(
                new MobileActivationRequest
                {
                    Command = "CreateTunnel1SessionInfoRequest"
                },
                cancellationToken);

            return response?.Value;
        }

        public async Task<NSObject> CreateActivationInfoAsync(CancellationToken cancellationToken)
        {
            var response = await this.ExecuteRequestAsync(
                new MobileActivationRequest
                {
                    Command = "CreateActivationInfoRequest"
                },
                cancellationToken);

            return response?.Value;
        }

        public async Task<NSObject> CreateActivationInfoWithSessionAsync(NSObject handshakeResponse, CancellationToken cancellationToken)
        {
            var dataValue = PlistDataFromPlist(handshakeResponse);
            var response = await this.ExecuteRequestAsync(
                new MobileActivationRequest
                {
                    Command = "CreateTunnel1ActivationInfoRequest",
                    Value = dataValue
                },
                cancellationToken);

            return response?.Value;
        }

        public async Task<NSObject> ActivateAsync(NSObject activationRecord, CancellationToken cancellationToken)
        {
            var response = await this.ExecuteRequestAsync(
                new MobileActivationRequest
                {
                    Command = "HandleActivationInfoRequest",
                    Value = activationRecord
                },
                cancellationToken);

            return response?.Value;
        }

        public async Task<NSObject> DeactivateAsync(CancellationToken cancellationToken)
        {
            var response = await this.ExecuteRequestAsync(
                new MobileActivationRequest
                {
                    Command = "DeactivateRequest"
                },
                cancellationToken);

            return response?.Value;
        }

        public async Task<NSObject> ActivateWithSessionAsync(NSObject activationRecord, NSObject headers, CancellationToken cancellationToken)
        {
            var dataValue = PlistDataFromPlist(activationRecord);
            var response = await this.ExecuteRequestAsync(
                new MobileActivationRequest
                {
                    Command = "HandleActivationInfoWithSessionRequest",
                    Value = dataValue,
                    ActivationResponseHeaders = headers
                },
                cancellationToken);

            return response?.Value;
        }

        public NSObject PlistDataFromPlist(NSObject plist)
        {
            if (plist is NSData)
            {
                return plist;
            }

            var data = new NSData(Encoding.Default.GetBytes(plist.ToXmlPropertyList()));
            return data;
        }



        public async Task<MobileActivationResponse> ExecuteRequestAsync(MobileActivationRequest request, CancellationToken cancellationToken)
        {
            await this.protocol.WriteMessageAsync(request.ToDictionary(), cancellationToken).ConfigureAwait(false);

            var response = await this.protocol.ReadMessageAsync(cancellationToken).ConfigureAwait(false);

            if (response == null)
            {
                return null;
            }

            var value = MobileActivationResponse.Read(response);

            return value;

        }


    }
}