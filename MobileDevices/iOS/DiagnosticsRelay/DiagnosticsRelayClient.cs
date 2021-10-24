using MobileDevices.iOS.PropertyLists;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Claunia.PropertyList;

namespace MobileDevices.iOS.DiagnosticsRelay
{

    /// <summary>
    /// A client which interacts with the diagnostics relay service running on an iOS device.
    /// </summary>
    public class DiagnosticsRelayClient : IAsyncDisposable
    {
        /// <summary>
        /// Gets the name of the diagnostics relay service running on the device.
        /// </summary>
        public const string ServiceName = "com.apple.mobile.diagnostics_relay";

        private readonly PropertyListProtocol protocol;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticsRelayClient"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents a connection to the diagnostics relay service running on the device.
        /// </param>
        /// <param name="logger">
        /// A logger which can be used when logging.
        /// </param>
        public DiagnosticsRelayClient(Stream stream, ILogger<DiagnosticsRelayClient> logger)
        {
            this.protocol = new PropertyListProtocol(stream, ownsStream: true, logger: logger);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticsRelayClient"/> class.
        /// </summary>
        /// <param name="protocol">
        /// A <see cref="PropertyListProtocol"/> which represents a connection to the diagnostics relay service running on the device.
        /// </param>
        public DiagnosticsRelayClient(PropertyListProtocol protocol)
        {
            this.protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
        }

        /// <summary>
        /// Restarts the device.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public virtual Task RestartAsync(CancellationToken cancellationToken)
        {
            return this.ExecuteRequestAsync(
                new DiagnosticsRelayRequest()
                {
                    Request = "Restart",
                    WaitForDisconnect = true,
                },
                cancellationToken);
        }

        /// <summary>
        /// Initiates a shut down of the device.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public virtual Task ShutdownAsync(CancellationToken cancellationToken)
        {
            return this.ExecuteRequestAsync(
                new DiagnosticsRelayRequest()
                {
                    Request = "Shutdown",
                    WaitForDisconnect = true,
                },
                cancellationToken);
        }

        /// <summary>
        /// Closes the connection with the diagnostics relay service.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public virtual Task GoodbyeAsync(CancellationToken cancellationToken)
        {
            return this.ExecuteRequestAsync(
                new DiagnosticsRelayRequest()
                {
                    Request = "Goodbye",
                },
                cancellationToken);
        }

        /// <summary>
        /// Queries the IO registry.
        /// </summary>
        /// <param name="entryName">
        /// The name of the entry to query.
        /// </param>
        /// <param name="entryClass">
        /// The class of the entry to query.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public virtual async Task<Dictionary<string, object>> QueryIoRegistryEntryAsync(string entryName, string entryClass, CancellationToken cancellationToken)
        {
            var response = await this.ExecuteRequestAsync(
                new DiagnosticsRelayRequest()
                {
                    Request = "IORegistry",
                    EntryName = entryName,
                    EntryClass = entryClass,
                },
                cancellationToken).ConfigureAwait(false);

            if (response == null)
            {
                return null;
            }

            return (Dictionary<string, object>)response.Diagnostics?["IORegistry"];
        }

        /// <summary>
        /// Queries the Mobile Gestalt
        /// </summary>
        /// <param name="entryArray"></param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public virtual async Task<Dictionary<string, object>> QueryMobileGestaltEntryAsync(object[] entryArray, CancellationToken cancellationToken)
        {
            var nSArray = NSObject.Wrap(entryArray);

            var response = await ExecuteRequestAsync(
                new DiagnosticsRelayRequest()
                {
                    Request = "MobileGestalt",
                    MobileGestaltKeys = nSArray
                },
                cancellationToken).ConfigureAwait(false);

            return (Dictionary<string, object>)response?.Diagnostics["MobileGestalt"];
        }


        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            return this.protocol.DisposeAsync();
        }

        private async Task<DiagnosticsRelayResponse> ExecuteRequestAsync(
            DiagnosticsRelayRequest request,
            CancellationToken cancellationToken)
        {
            await this.protocol.WriteMessageAsync(request.ToDictionary(), cancellationToken).ConfigureAwait(false);

            var response = await this.protocol.ReadMessageAsync(cancellationToken).ConfigureAwait(false);

            if (response == null)
            {
                return null;
            }

            var value = DiagnosticsRelayResponse.Read(response);

            if (value.Status != DiagnosticsRelayStatus.Success)
            {
                throw new DiagnosticsRelayException(value.Status);
            }

            return value;
        }
    }
}
