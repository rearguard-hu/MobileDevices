using System;
using System.Diagnostics;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft;
using MobileDevices.iOS.Lockdown;

namespace MobileDevices.iOS.Services
{
    /// <content>
    /// SSL-related methods for the <see cref="ServiceProtocol"/> class.
    /// </content>
    public partial class ServiceProtocol
    {

        /// <summary>
        /// Gets a value indicating whether the communication with the device is secured using SSL.
        /// </summary>
        public virtual bool SslEnabled => this.stream != this.rawStream;

        /// <summary>
        /// Asynchronously enables SSL communications with the device.
        /// </summary>
        /// <param name="pairingRecord">
        /// A <see cref="PairingRecord"/> which contains the certificates used to authenticate the host and the device.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public virtual async Task EnableSslAsync(PairingRecord pairingRecord, CancellationToken cancellationToken)
        {
            Verify.NotDisposed(this);

            if (pairingRecord == null)
            {
                throw new ArgumentNullException(nameof(pairingRecord));
            }

            if (this.stream is SslStream)
            {
                throw new InvalidOperationException("This connection is already using SSL");
            }

            // This block of code constructs a TLS ("SSL") stream which enables you to connect with lockdown over an encrypted
            // connection. It uses the built-in SslStream class.

            // AllowNoEncryption will use OpenSSL security policy/level 0, see
            // https://github.com/dotnet/runtime/blob/master/src/libraries/Common/src/Interop/Unix/System.Security.Cryptography.Native/Interop.OpenSsl.cs
            // https://github.com/dotnet/runtime/blob/master/src/libraries/Native/Unix/System.Security.Cryptography.Native/pal_ssl.c
            // When using security policy/level 1, the root CA may be rejected on Ubuntu 20.04 (which uses OpenSSL 1.1)
            var encryptionPolicy = EncryptionPolicy.AllowNoEncryption;

            var sslStream = new SslStream(
                innerStream: this.stream,
                leaveInnerStreamOpen: true,
                userCertificateSelectionCallback: (object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers) =>
                {
                    return localCertificates[0];
                },
                userCertificateValidationCallback: (sender, certificate, chain, sslPolicyErrors) =>
                {
                    var expectedDeviceCertHash = pairingRecord.DeviceCertificate.GetCertHashString();
                    var actualDeviceCertHash = certificate.GetCertHashString();

                    return string.Equals(expectedDeviceCertHash, actualDeviceCertHash, StringComparison.OrdinalIgnoreCase);
                },
                encryptionPolicy: encryptionPolicy);

            var clientCertificates = new X509CertificateCollection();
            clientCertificates.Add(pairingRecord.HostCertificate.CopyWithPrivateKeyForSsl(pairingRecord.HostPrivateKey));

            await sslStream.AuthenticateAsClientAsync(
                pairingRecord.DeviceCertificate.Subject,
                clientCertificates,
                SslProtocols.Tls12,
                checkCertificateRevocation: false).ConfigureAwait(false);

            this.Stream = sslStream;
        }

        /// <summary>
        /// Asynchronously terminates the secure connection between the device and the host. When this method has completes,
        /// communication with the device will continue using plain text.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public virtual async Task DisableSslAsync(CancellationToken cancellationToken)
        {
            Verify.NotDisposed(this);

            var sslStream = this.stream as SslStream;

            if (sslStream == null)
            {
                throw new InvalidOperationException("The session is not using SSL");
            }

            // Initiate a shutdown
            await sslStream.ShutdownAsync().ConfigureAwait(false);

            // The remote end (the device) will send an alert acknowledging the shutdown. We need to manually
            // read data to force the SslStream class to process that alert. The SslStream will process the alert
            // and let us know that 0 bytes of data have been read.
            var readBuffer = new byte[128];
            var bytesRead = await sslStream.ReadAsync(readBuffer, 0, readBuffer.Length, cancellationToken).ConfigureAwait(false);

            Debug.Assert(bytesRead == 0, "A SSL stream should not return data after a shutdown command.");

            await sslStream.DisposeAsync();

            this.Stream = this.rawStream;
        }

    }
}
