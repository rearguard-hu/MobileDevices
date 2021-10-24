using MobileDevices.iOS.DeveloperProfiles;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace MobileDevices.Tests.DeveloperProfiles
{
    /// <summary>
    /// Tests the <see cref="ProvisioningProfile"/> class.
    /// </summary>
    public class ProvisioningProfileTests
    {
        /// <summary>
        /// <see cref="ProvisioningProfile.Read(SignedCms)"/> throws when passed a <see langword="null"/>
        /// value.
        /// </summary>
        [Fact]
        public void Read_ThrowsOnNull()
        {
            Assert.Throws<ArgumentNullException>("signedData", () => ProvisioningProfile.Read(null));
        }

        /// <summary>
        /// <see cref="ProvisioningProfile.ReadAsync(Stream, CancellationToken)"/> throws when passed
        /// a <see langword="null"/> value.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ReadAsync_ThrowsOnNull_Async()
        {
            await Assert.ThrowsAsync<ArgumentNullException>("stream", () => ProvisioningProfile.ReadAsync(null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="ProvisioningProfile.ReadAsync(Stream, CancellationToken)"/> reads the correct
        /// values.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ReadAsync_MobileProvision_Works_Async()
        {
            using (Stream stream = File.OpenRead("DeveloperProfiles/test.mobileprovision"))
            {
                var profile = await ProvisioningProfile.ReadAsync(stream, default).ConfigureAwait(false);

                Assert.Equal("fastlane test app", profile.AppIdName);
                Assert.Equal("439BBM9367", Assert.Single(profile.ApplicationIdentifierPrefix));
                Assert.Equal("iOS", Assert.Single(profile.Platform));
                Assert.Equal(new DateTimeOffset(2015, 11, 8, 21, 9, 3, TimeSpan.Zero), profile.CreationDate);
                Assert.Single(profile.DeveloperCertificates);

                Assert.Null(profile.Entitlements.AddressBook);
                Assert.Null(profile.Entitlements.ApplicationGroups);
                Assert.Equal("439BBM9367.tools.fastlane.app", profile.Entitlements.ApplicationIdentifier);
                Assert.Null(profile.Entitlements.AppSandbox);
                Assert.Null(profile.Entitlements.ApsEnvironment);
                Assert.Null(profile.Entitlements.AssociatedDomains);
                Assert.Null(profile.Entitlements.AudioVideoBridging);
                Assert.True(profile.Entitlements.BetaReportsActive);
                Assert.Null(profile.Entitlements.Bluetooth);
                Assert.Null(profile.Entitlements.BookmarksAppScope);
                Assert.Null(profile.Entitlements.BookmarksDocumentScope);
                Assert.Null(profile.Entitlements.Calendars);
                Assert.Null(profile.Entitlements.Camera);
                Assert.Null(profile.Entitlements.DefaultDataProtection);
                Assert.Null(profile.Entitlements.DownloadsReadWrite);
                Assert.Null(profile.Entitlements.Firewire);
                Assert.False(profile.Entitlements.GetTaskAllow);
                Assert.Null(profile.Entitlements.HealthKit);
                Assert.Null(profile.Entitlements.HomeKit);
                Assert.Null(profile.Entitlements.iCloudDocumentStore);
                Assert.Null(profile.Entitlements.iCloudKeyValueStore);
                Assert.Null(profile.Entitlements.InAppPayments);
                Assert.Null(profile.Entitlements.InheritSecurity);
                Assert.Null(profile.Entitlements.InterAppAudio);
                Assert.Equal("439BBM9367.*", Assert.Single(profile.Entitlements.KeychainAccessGroups));
                Assert.Null(profile.Entitlements.Location);
                Assert.Null(profile.Entitlements.Microphone);
                Assert.Null(profile.Entitlements.MoviesReadOnly);
                Assert.Null(profile.Entitlements.MoviesReadWrite);
                Assert.Null(profile.Entitlements.MusicReadOnly);
                Assert.Null(profile.Entitlements.MusicReadWrite);
                Assert.Null(profile.Entitlements.NetworkClient);
                Assert.Null(profile.Entitlements.NetworkExtensions);
                Assert.Null(profile.Entitlements.NetworkServer);
                Assert.Null(profile.Entitlements.PicturesReadOnly);
                Assert.Null(profile.Entitlements.PicturesReadWrite);
                Assert.Null(profile.Entitlements.Print);
                Assert.Null(profile.Entitlements.ScriptingTargets);
                Assert.Null(profile.Entitlements.Serial);
                Assert.Null(profile.Entitlements.SiriKit);
                Assert.Equal("439BBM9367", profile.Entitlements.TeamIdentifier);
                Assert.Null(profile.Entitlements.Usb);
                Assert.Null(profile.Entitlements.UserSelectedFilesExecutable);
                Assert.Null(profile.Entitlements.UserSelectedFilesReadOnly);
                Assert.Null(profile.Entitlements.UserSelectedFilesReadWrite);
                Assert.Null(profile.Entitlements.VpnApi);

                Assert.Equal(new DateTimeOffset(2016, 11, 7, 20, 58, 52, TimeSpan.Zero), profile.ExpirationDate);
                Assert.Equal("tools.fastlane.app AppStore", profile.Name);
                Assert.Null(profile.ProvisionsAllDevices);
                Assert.Null(profile.ProvisionedDevices);
                Assert.Equal("439BBM9367", Assert.Single(profile.TeamIdentifier));
                Assert.Equal("Felix Krause", profile.TeamName);
                Assert.Equal(364, profile.TimeToLive);
                Assert.Equal(new Guid("98264c6b-5151-4349-8d0f-66691e48ae35"), profile.Uuid);
                Assert.Equal(1, profile.Version);

                Assert.Equal("tools.fastlane.app AppStore", profile.ToString());
            }
        }
    }
}
