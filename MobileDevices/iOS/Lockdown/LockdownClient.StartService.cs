using System;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.Lockdown
{
    /// <content>
    /// Methods to start services on iOS devices.
    /// </content>
    public partial class LockdownClient
    {
        /// <summary>
        /// Starts a service on the device.
        /// </summary>
        /// <param name="serviceName">
        /// The name of the service to start.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous request.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous response. The return value is the port
        /// at which the service is listening.
        /// </returns>
        public virtual async Task<ServiceDescriptor> StartServiceAsync(string serviceName, CancellationToken cancellationToken)
        {
            if (serviceName == null)
            {
                throw new ArgumentNullException(nameof(serviceName));
            }

            await this.protocol.WriteMessageAsync(
                new StartServiceRequest()
                {
                    Label = this.Label,
                    Request = "StartService",
                    Service = serviceName,
                },
                cancellationToken).ConfigureAwait(false);

            var response = await this.protocol.ReadMessageAsync<StartServiceResponse>(cancellationToken).ConfigureAwait(false);

            if (response == null)
            {
                return null;
            }

            this.EnsureSuccess(response);

            return new ServiceDescriptor()
            {
                Port = unchecked((ushort)response.Port),
                EnableServiceSSL = response.EnableServiceSSL,
                ServiceName = serviceName,
            };
        }

    }
}
