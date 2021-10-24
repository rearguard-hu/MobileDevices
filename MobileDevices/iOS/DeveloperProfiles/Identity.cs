using System;
using System.Security.Cryptography.X509Certificates;

namespace MobileDevices.iOS.DeveloperProfiles
{
    /// <summary>
    /// A POCO object which contains information about an X509 Certificate.
    /// </summary>
    public class Identity
    {
        /// <summary>
        /// Gets or sets the thumbprint of the certificate.
        /// </summary>
        public string Thumbprint
        { get; set; }

        /// <summary>
        /// Gets or sets the simple name of the subject of the certificate.
        /// </summary>
        public string CommonName
        { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the expiration date of this certificate.
        /// </summary>
        public DateTimeOffset NotAfter
        { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a private key for this certificate is available.
        /// </summary>
        public bool HasPrivateKey
        { get; set; }

        /// <summary>
        /// Gets or sets the type (development or production) of the certificate.
        /// </summary>
        public string Type
        { get; set; }

        /// <summary>
        /// Gets or sets the ID of the person for which the certificate was issued.
        /// </summary>
        public string PersonID
        { get; set; }

        /// <summary>
        /// Gets or sets the name of the person to which the certificate was issued.
        /// </summary>
        public string Name
        { get; set; }

        /// <summary>
        /// Converts a <see cref="X509Certificate2"/> object into a <see cref="Identity"/> object.
        /// </summary>
        /// <param name="certificate">
        /// The certificate to convert.
        /// </param>
        /// <returns>
        /// A <see cref="Identity"/> object which represnts <paramref name="certificate"/>.
        /// </returns>
        public static Identity FromX509Certificate(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException(nameof(certificate));
            }

            var commonName = certificate.GetNameInfo(X509NameType.SimpleName, false);

            // Sample: iPhone Developer: [First Name] [Last Name] (Theam Id)
            // Sample: iPhone Distribution: [Company Name]
            string name = null;
            string personId = null;
            string type = null;

            var firstColon = commonName.IndexOf(':');

            if (firstColon > 0)
            {
                type = commonName.Substring(0, firstColon);

                var lastBracket = commonName.LastIndexOf('(');

                if (lastBracket == -1)
                {
                    // Production certificates don't have this
                    name = commonName.Substring(firstColon + 2);
                    personId = null;
                }
                else
                {
                    name = commonName.Substring(firstColon + 2, lastBracket - firstColon - 3);
                    personId = commonName.Substring(lastBracket + 1, commonName.Length - lastBracket - 2);
                }
            }

            return new Identity()
            {
                Thumbprint = certificate.Thumbprint,
                CommonName = certificate.GetNameInfo(X509NameType.SimpleName, false),
                NotAfter = certificate.NotAfter,
                HasPrivateKey = certificate.HasPrivateKey,
                Type = type,
                Name = name,
                PersonID = personId,
            };
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{this.Name} ({this.Thumbprint})";
        }
    }
}
