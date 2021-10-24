using Microsoft.Extensions.Logging;
using MobileDevices.iOS.Muxer;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS
{
    /// <summary>
    /// The base class for a factory which can create a client for a service running on an iOS
    /// device, which is accessible via the USB interface.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the client being accessed.
    /// </typeparam>
    public abstract class ClientFactory<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientFactory{T}"/> class.
        /// </summary>
        /// <param name="muxer">
        /// The <see cref="MuxerClient"/> which represents the connection to the iOS USB Multiplexor.
        /// </param>
        /// <param name="context">
        /// The <see cref="DeviceContext"/> which contains information about the device with which
        /// we are interacting.
        /// </param>
        /// <param name="logger">
        /// A <see cref="ILogger"/> which can be used when logging.
        /// </param>
        protected ClientFactory(MuxerClient muxer, DeviceContext context, ILogger<T> logger)
        {
            this.Muxer = muxer ?? throw new ArgumentNullException(nameof(muxer));
            this.Context = context ?? throw new ArgumentNullException(nameof(context));
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientFactory{T}"/> class. Intended
        /// for mocking purposes only.
        /// </summary>
        protected ClientFactory()
        {
        }

        /// <summary>
        /// Gets the <see cref="MuxerClient"/> which represents the connection to the iOS USB Multiplexor.
        /// </summary>
        public MuxerClient Muxer { get; }

        /// <summary>
        /// Gets the <see cref="DeviceContext"/> which contains information about the device with which
        /// we are interacting.
        /// </summary>
        public DeviceContext Context { get; }

        /// <summary>
        /// Gets a <see cref="ILogger"/> which can be used when logging.
        /// </summary>
        public ILogger<T> Logger { get; }

        /// <summary>
        /// Asynchronously creates a new instance of the <typeparamref name="T"/> client.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous
        /// operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and returns the
        /// newly created <typeparamref name="T"/> service client when completed.
        /// </returns>
        public abstract Task<T> CreateAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously creates a new instance of the <typeparamref name="T"/> client.
        /// </summary>
        /// <param name="serviceName">
        /// The name of the service to which to connect.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous
        /// operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and returns the
        /// newly created <typeparamref name="T"/> service client when completed.
        /// </returns>
        public abstract Task<T> CreateAsync(string serviceName, CancellationToken cancellationToken);

    }
}
