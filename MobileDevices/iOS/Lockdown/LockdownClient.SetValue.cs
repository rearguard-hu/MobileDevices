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
        /// Issues a SetValue request.
        /// </summary>
        /// <param name="domain">
        /// The domain of the key to set.
        /// </param>
        /// <param name="key">
        /// The key for the value to set.
        /// </param>
        /// <param name="value">
        /// The value to set the key to.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// The requested value.
        /// </returns>
        public virtual async Task SetValueAsync(string domain, string key, object value, CancellationToken cancellationToken)
        {
            await this.protocol.WriteMessageAsync(
                new SetValueRequest()
                {
                    Label = this.Label,
                    Domain = domain,
                    Key = key,
                    Value = value,
                },
                cancellationToken).ConfigureAwait(false);

            var response = await this.protocol.ReadMessageAsync<GetValueResponse<object>>(cancellationToken).ConfigureAwait(false);

            this.EnsureSuccess(response);
        }
    }
}
