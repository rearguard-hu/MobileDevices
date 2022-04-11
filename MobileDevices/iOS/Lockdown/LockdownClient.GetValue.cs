using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.Lockdown
{
    /// <content>
    /// GetValue-related methods.
    /// </content>
    public partial class LockdownClient
    {
        /// <summary>
        /// Issues a GetValue request.
        /// </summary>
        /// <param name="key">
        /// The key for the value to get.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// The requested value.
        /// </returns>
        public virtual Task<string> GetValueAsync(string key, CancellationToken cancellationToken)
        {
            return this.GetValueAsync<string>(null, key, cancellationToken);
        }

        /// <summary>
        /// Issues a GetValue request.
        /// </summary>
        /// <param name="domain">
        /// The domain of the value to get.
        /// </param>
        /// <param name="key">
        /// The key for the value to get.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <typeparam name="T">
        /// The type of the value.
        /// </typeparam>
        /// <returns>
        /// The requested value.
        /// </returns>
        public async Task<T> GetValueAsync<T>(string domain, string key, CancellationToken cancellationToken)
        {
            await this.protocol.WriteMessageAsync(
                new GetValueRequest()
                {
                    Label = this.Label,
                    Domain = domain,
                    Key = key,
                    Request = "GetValue",
                },
                cancellationToken).ConfigureAwait(false);

            var response = await this.protocol.ReadMessageAsync<GetValueResponse<T>>(cancellationToken).ConfigureAwait(false);

            this.EnsureSuccess(response);

            return response == null ? default : response.Value;
        }

        /// <summary>
        /// Asynchronously gets the public key of a device.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual Task<byte[]> GetPublicKeyAsync(CancellationToken cancellationToken)
        {
            return this.GetValueAsync<byte[]>(null, "DevicePublicKey", cancellationToken);
        }

        /// <summary>
        /// Asynchronously gets the WiFi address of a device.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual Task<string> GetWifiAddressAsync(CancellationToken cancellationToken)
        {
            return this.GetValueAsync("WiFiAddress", cancellationToken);
        }
    }
}
