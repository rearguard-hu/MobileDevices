using MobileDevices.iOS.PropertyLists;
using Microsoft;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.MobileImageMounter
{
    /// <summary>
    /// The <see cref="MobileImageMounterClient"/> provides access to the mobile image mounter services running on the device.
    /// </summary>
    public class MobileImageMounterClient : IAsyncDisposable, IDisposableObservable
    {
        /// <summary>
        /// Gets the name of the mobile image mounter service on the device.
        /// </summary>
        public const string ServiceName = "com.apple.mobile.mobile_image_mounter";

        private readonly PropertyListProtocol protocol;

        /// <summary>
        /// Initializes a new instance of the <see cref="MobileImageMounterClient"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents a connection to the mobile image mounter services running on the device.
        /// </param>
        /// <param name="logger">
        /// A logger which can be used when logging.
        /// </param>
        public MobileImageMounterClient(Stream stream, ILogger logger)
        {
            this.protocol = new PropertyListProtocol(stream, ownsStream: true, logger);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MobileImageMounterClient"/> class.
        /// </summary>
        /// <param name="protocol">
        /// A <see cref="PropertyListProtocol"/> which represents a connection to the mobile image mounter services running on the device.
        /// </param>
        public MobileImageMounterClient(PropertyListProtocol protocol)
        {
            this.protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MobileImageMounterClient"/> class.
        /// </summary>
        /// <remarks>
        /// Intended for mocking purposes only.
        /// </remarks>
        protected MobileImageMounterClient()
        {
        }

        /// <inheritdoc/>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets the image status of the device.
        /// </summary>
        /// <param name="imageType">
        /// The image type to lookup.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A image status of the device.
        /// </returns>
        public virtual async Task<LookupImageResponse> LookupImageAsync(string imageType, CancellationToken cancellationToken)
        {
            Verify.NotDisposed(this);
            Requires.NotNull(imageType, nameof(imageType));

            await this.protocol.WriteMessageAsync(
                new LookupImageRequest()
                {
                    ImageType = imageType,
                },
                cancellationToken).ConfigureAwait(false);

            var response = await this.protocol.ReadMessageAsync<LookupImageResponse>(cancellationToken).ConfigureAwait(false);
            this.EnsureValidResponse(response);

            return response;
        }

        /// <summary>
        /// Uploads the image to the device.
        /// </summary>
        /// <param name="image">
        /// The image to be uploaded.
        /// </param>
        /// <param name="imageType">
        /// The type of the image. e.g. "Developer".
        /// </param>
        /// <param name="signature">
        /// The signature of the image.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public virtual async Task UploadImageAsync(Stream image, string imageType, byte[] signature, CancellationToken cancellationToken)
        {
            Verify.NotDisposed(this);

            Requires.NotNull(image, nameof(image));
            Requires.NotNull(imageType, nameof(imageType));
            Requires.NotNull(signature, nameof(signature));

            await this.protocol.WriteMessageAsync(
                new UploadImageRequest()
                {
                    ImageType = imageType,
                    ImageSignature = signature,
                    ImageSize = (int)image.Length,
                },
                cancellationToken).ConfigureAwait(false);

            var response = await this.protocol.ReadMessageAsync<MobileImageMounterResponse>(cancellationToken).ConfigureAwait(false);
            this.EnsureValidResponse(response, MobileImageMounterStatus.ReceiveBytesAck);

            image.Seek(0, SeekOrigin.Begin);
            await image.CopyToAsync(this.protocol.Stream, bufferSize: 0x10_000, cancellationToken).ConfigureAwait(false);

            response = await this.protocol.ReadMessageAsync<MobileImageMounterResponse>(cancellationToken).ConfigureAwait(false);
            this.EnsureValidResponse(response, MobileImageMounterStatus.Complete);
        }

        /// <summary>
        /// Mounts the uploaded image to the device.
        /// </summary>
        /// <param name="signature">
        /// The signature of the image.
        /// </param>
        /// <param name="imageType">
        /// The image type.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public virtual async Task MountImageAsync(byte[] signature, string imageType, CancellationToken cancellationToken)
        {
            Verify.NotDisposed(this);

            Requires.NotNull(signature, nameof(signature));
            Requires.NotNull(imageType, nameof(imageType));

            await this.protocol.WriteMessageAsync(
                new MountImageRequest()
                {
                    ImageType = imageType,
                    ImageSignature = signature,
                    ImagePath = "/private/var/mobile/Media/PublicStaging/staging.dimage",
                },
                cancellationToken).ConfigureAwait(false);

            var response = await this.protocol.ReadMessageAsync<MobileImageMounterResponse>(cancellationToken).ConfigureAwait(false);
            this.EnsureValidResponse(response, MobileImageMounterStatus.Complete);
        }

        /// <summary>
        /// Sends the hangup command.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public virtual async Task HangupAsync(CancellationToken cancellationToken)
        {
            Verify.NotDisposed(this);

            await this.protocol.WriteMessageAsync(
                new HangupRequest(),
                cancellationToken).ConfigureAwait(false);

            var response = this.protocol.ReadMessageAsync<HangupResponse>(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            this.IsDisposed = true;

            return this.protocol == null ? ValueTask.CompletedTask : this.protocol.DisposeAsync();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.IsDisposed = true;

            this.protocol?.Dispose();
        }

        private void EnsureValidResponse(MobileImageMounterResponse response)
        {
            if (response.Error != null)
            {
                throw new MobileImageMounterException($"Invalid image mounter response: {response.Status}: {response.Error}", response.Status, response.Error.Value, response.DetailedError);
            }
        }

        private void EnsureValidResponse(MobileImageMounterResponse response, MobileImageMounterStatus expectedStatus)
        {
            this.EnsureValidResponse(response);

            if (response.Status != expectedStatus)
            {
                throw new MobileImageMounterException($"Invalid image mounter response. Expected {expectedStatus} but received the {response.Status} status.", response.Status, response.Error, response.DetailedError);
            }
        }
    }
}
