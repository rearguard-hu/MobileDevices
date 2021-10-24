using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.DependencyInjection
{
    /// <summary>
    /// A <see cref="IServiceScope"/> which can be used to start and interact with lockdown services running on iOS devices.
    /// </summary>
    public class DeviceServiceScope : IServiceScope
    {
        private readonly IServiceScope scope;
        private readonly DeviceContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceServiceScope"/> class.
        /// </summary>
        /// <param name="scope">
        /// The underlying service scope.
        /// </param>1
        public DeviceServiceScope(IServiceScope scope)
        {
            this.scope = scope ?? throw new ArgumentNullException(nameof(scope));
            this.context = scope.ServiceProvider.GetRequiredService<DeviceContext>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceServiceScope"/> class.
        /// </summary>
        /// <remarks>
        /// Intended for mocking purposes only.
        /// </remarks>
        protected DeviceServiceScope()
        {
        }

        /// <summary>
        /// Gets the <see cref="DeviceContext"/> for the device to which this scope relates.
        /// </summary>
        public DeviceContext Context => this.context;

        /// <inheritdoc/>
        public IServiceProvider ServiceProvider => this.scope.ServiceProvider;

        /// <summary>
        /// Asynchronously starts a service on the device.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the service to start.
        /// </typeparam>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation, which returns a <typeparamref name="T"/>
        /// which represents a client for the service running on the device.
        /// </returns>
        public virtual Task<T> StartServiceAsync<T>(CancellationToken cancellationToken)
        {
            var factory = this.scope.ServiceProvider.GetRequiredService<ClientFactory<T>>();
            return factory.CreateAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.scope?.Dispose();
        }
    }
}
