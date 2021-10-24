using MobileDevices.iOS.DeveloperProfiles;
using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MobileDevices.Tests.DeveloperProfiles
{
    /// <summary>
    /// Tests the <see cref="DeveloperProfilePackage"/> class.
    /// </summary>
    public class DeveloperProfilePackageTests
    {
        /// <summary>
        /// The <see cref="DeveloperProfilePackage.ReadAsync(Stream, string, CancellationToken)"/> method validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReadAsync_ValidatesArguments_Async()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => DeveloperProfilePackage.ReadAsync(null, string.Empty, default));
            await Assert.ThrowsAsync<ArgumentNullException>(() => DeveloperProfilePackage.ReadAsync(Stream.Null, null, default));
        }

        /// <summary>
        /// The <see cref="DeveloperProfilePackage.ReadAsync(Stream, string, CancellationToken)"/> method works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReadAsync_Works_Async()
        {
            using (Stream stream = File.OpenRead("DeveloperProfiles/developer.zip"))
            {
                var package = await DeveloperProfilePackage.ReadAsync(stream, string.Empty, default).ConfigureAwait(false);
                Assert.Single(package.Identities);
                Assert.Single(package.ProvisioningProfiles);
            }
        }

        /// <summary>
        /// The <see cref="DeveloperProfilePackage.WriteAsync(Stream, string, CancellationToken)"/> method works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WriteAsync_ValidatesArguments_Async()
        {
            using (var profile = new DeveloperProfilePackage())
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => profile.WriteAsync(null, string.Empty, default));
                await Assert.ThrowsAsync<ArgumentNullException>(() => profile.WriteAsync(Stream.Null, null, default));

                profile.Dispose();

                await Assert.ThrowsAsync<ObjectDisposedException>(() => profile.WriteAsync(null, null, default));
            }
        }

        /// <summary>
        /// <see cref="DeveloperProfilePackage.WriteAsync(Stream, string, CancellationToken)"/> creates a well-formed archive.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WriteAsync_Works_Async()
        {
            using (var profile = new DeveloperProfilePackage())
            using (var stream = new MemoryStream())
            {
                profile.Identities.Add(new X509Certificate2("DeveloperProfiles/E7P4EE896K.cer"));

                var provisioningProfile = new SignedCms();
                provisioningProfile.Decode(File.ReadAllBytes("DeveloperProfiles/test.mobileprovision"));
                profile.ProvisioningProfiles.Add(provisioningProfile);

                await profile.WriteAsync(stream, "test", default).ConfigureAwait(false);

                stream.Seek(0, SeekOrigin.Begin);

                using (var archive = new ZipArchive(stream))
                {
                    Assert.Collection(
                        archive.Entries,
                        e =>
                        {
                            Assert.Equal("developer/profiles/98264c6b-5151-4349-8d0f-66691e48ae35.mobileprovision", e.FullName);
                            Assert.NotEqual(0, e.Length);
                        },
                        e =>
                        {
                            Assert.Equal("developer/identities/ef4751ca452094e26a79d6f8bfdc08413ce6c90d.cer.p12", e.FullName);
                            Assert.NotEqual(0, e.Length);
                        });
                }
            }
        }
    }
}