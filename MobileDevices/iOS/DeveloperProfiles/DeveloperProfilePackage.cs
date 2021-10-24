using Microsoft;
using Nerdbank.Streams;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace MobileDevices.iOS.DeveloperProfiles
{
    /// <summary>
    /// A <see cref="DeveloperProfilePackage"/> represents a ZIP archive which contains provisioning profiles and developer
    /// identities (X.509 certificates).
    /// </summary>
    public class DeveloperProfilePackage : IDisposableObservable
    {
        /// <summary>
        /// Gets a list of provisioning profiles embedded in this package.
        /// </summary>
        public IList<SignedCms> ProvisioningProfiles { get; } = new List<SignedCms>();

        /// <summary>
        /// Gets a list of developer identities, stored as certificates, embedded in this package.
        /// </summary>
        public IList<X509Certificate2> Identities { get; } = new List<X509Certificate2>();

        /// <inheritdoc/>
        public bool IsDisposed
        { get; private set; }

        /// <summary>
        /// Asynchronously reads the contents of a developer profile package.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> from which to read the developer profile package.
        /// This stream must allow for synchronous I/O.
        /// </param>
        /// <param name="password">
        /// The password to use when reading the private keys of the certificates embedded
        /// in this developer profile package.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous
        /// operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and return a
        /// <see cref="DeveloperProfilePackage"/> when completed.
        /// </returns>
        public static async Task<DeveloperProfilePackage> ReadAsync(Stream stream, string password, CancellationToken cancellationToken)
        {
            Requires.NotNull(stream, nameof(stream));
            Requires.NotNull(password, nameof(password));

            var developerProfile = new DeveloperProfilePackage();

            using (ZipArchive archive = new ZipArchive(stream))
            {
                // Regarding the check for IsDirectory:
                // This doesn't happen with the Apple files, but if you
                // manually unzip & rezip the .developerprofile file, the zip tool
                // may create directory entries for you.
                var profileEntries =
                    from entry
                    in archive.Entries
                    let entryName = entry.FullName
                    where entryName != null
                        && entryName.StartsWith("developer/profiles", StringComparison.OrdinalIgnoreCase)
                        && !IsDirectory(entry)
                    select entry;

                foreach (var profileEntry in profileEntries)
                {
                    using (Stream profileStream = profileEntry.Open())
                    using (var content = MemoryPool<byte>.Shared.Rent((int)profileEntry.Length))
                    {
                        var memory = content.Memory.Slice(0, (int)profileEntry.Length);

                        SignedCms cms = new SignedCms();
                        await profileStream.ReadBlockOrThrowAsync(memory, cancellationToken).ConfigureAwait(false);
                        cms.Decode(memory.Span);

                        developerProfile.ProvisioningProfiles.Add(cms);
                    }
                }

                var identityEntries =
                    from entry
                    in archive.Entries
                    let entryName = entry.FullName
                    where entryName.StartsWith("developer/identities", StringComparison.OrdinalIgnoreCase)
                    && !IsDirectory(entry)
                    select entry;

                foreach (var identityEntry in identityEntries)
                {
                    X509Certificate2 certificate = null;

                    using (Stream identityStream = identityEntry.Open())
                    using (var content = MemoryPool<byte>.Shared.Rent((int)identityEntry.Length))
                    {
                        var memory = content.Memory.Slice(0, (int)identityEntry.Length);

                        await identityStream.ReadBlockAsync(memory, cancellationToken).ConfigureAwait(false);

                        certificate = new X509Certificate2(memory.Span, password, X509KeyStorageFlags.Exportable);
                    }

                    developerProfile.Identities.Add(certificate);
                }
            }

            return developerProfile;
        }

        /// <summary>
        /// Asynchronously writes the identities and provisioning profiles embedded in this
        /// <see cref="DeveloperProfilePackage"/> to a file.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents the file to which to write the developer profile.
        /// </param>
        /// <param name="password">
        /// The password to use when encrypting the private keys of the developer profiles.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public async Task WriteAsync(Stream stream, string password, CancellationToken cancellationToken)
        {
            Verify.NotDisposed(this);

            Requires.NotNull(stream, nameof(stream));
            Requires.NotNull(password, nameof(password));

            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var profile in this.ProvisioningProfiles)
                {
                    var profileContents = ProvisioningProfile.Read(profile);

                    var entry = archive.CreateEntry($"developer/profiles/{profileContents.Uuid}.mobileprovision");

                    using (var entryStream = entry.Open())
                    {
                        await entryStream.WriteAsync(profile.Encode(), cancellationToken).ConfigureAwait(false);
                        await entryStream.FlushAsync(cancellationToken).ConfigureAwait(false);
                    }
                }

                foreach (var identity in this.Identities)
                {
                    var entry = archive.CreateEntry($"developer/identities/{identity.Thumbprint.ToLower()}.cer.p12");

                    using (var entryStream = entry.Open())
                    {
                        byte[] data = identity.Export(X509ContentType.Pkcs12, password);
                        await entryStream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
                        await entryStream.FlushAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            foreach (var identity in this.Identities)
            {
                identity.Dispose();
            }

            this.Identities.Clear();
            this.ProvisioningProfiles.Clear();

            this.IsDisposed = true;
        }

        private static bool IsDirectory(ZipArchiveEntry entry)
        {
            return entry.Name.EndsWith("/", StringComparison.OrdinalIgnoreCase) || entry.Length == 0;
        }
    }
}
