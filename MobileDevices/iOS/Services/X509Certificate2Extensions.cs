using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace MobileDevices.iOS.Services
{
    /// <summary>
    /// Extension methods for <see cref="X509Certificate2"/> certificates.
    /// </summary>
    public static class X509Certificate2Extensions
    {

        /// <summary>
        /// Combines a private key with the public key of an <see cref="RSA"/> certificate to generate a new RSA certificate.
        /// </summary>
        /// <param name="certificate">
        /// The RSA certificate.
        /// </param>
        /// <param name="privateKey">
        /// The private RSA key.
        /// </param>
        /// <returns>
        /// A new RSA certificate with the <see cref="X509Certificate2.HasPrivateKey"/> property set to <see langword="true"/>.
        /// On Windows, the private key is guaranteed to not be ephemeral.
        /// The input RSA certificate object isn't modified.
        /// </returns>
        public static X509Certificate2 CopyWithPrivateKeyForSsl(this X509Certificate2 certificate, RSA privateKey)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var cert = certificate.CopyWithPrivateKey(privateKey);
                return new X509Certificate2(cert.Export(X509ContentType.Pkcs12));
            }
            else
            {
                return certificate.CopyWithPrivateKey(privateKey);
            }
        }
    }
}
