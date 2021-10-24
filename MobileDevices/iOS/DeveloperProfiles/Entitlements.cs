using Claunia.PropertyList;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MobileDevices.iOS.DeveloperProfiles
{
    /// <summary>
    /// Entitlements confer specific capabilities or security permissions to your iOS or OS X app.
    /// </summary>
    /// <seealso href="https://developer.apple.com/library/mac/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/AboutEntitlements.html"/>
    public class Entitlements
    {
        /// <summary>
        /// Gets or sets the application identifier.
        /// </summary>
        /// <remarks>
        /// An application ID uniquely identifies your iOS application within Apple's application services.
        /// It has the format <c>&lt;Bundle Seed ID&gt;.&lt;Bundle  Identifier&gt;</c>.
        /// The bundle seed ID identifies the team, whereas the bundle identifier identifies the app.
        /// If you don't need to use any of Apple's services, you can use a wildcard identifier, which
        /// is written as <c>*</c>.
        /// </remarks>
        public string ApplicationIdentifier { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether other processes (like the debugger) can attach to the app.
        /// </summary>
        /// <remarks>
        /// <c>get-task-allow</c>, when signed into an application, allows other processes (like the debugger) to attach to
        /// your app. Distribution profiles require that this value be turned off, while development profiles require this
        /// value to be turned on.
        /// </remarks>
        public bool? GetTaskAllow { get; set; }

        /// <summary>
        /// Gets or sets the value of the push notification entitlement.
        /// </summary>
        /// <remarks>
        /// Push notifications let your app alert the user even when your iOS or OS X app is not executing.
        /// </remarks>
        public string ApsEnvironment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the App Store code signed builds are allowed
        /// to be tested using iTunes Connect.
        /// </summary>
        /// <seealso href="https://developer.apple.com/library/ios/qa/qa1830/_index.html"/>
        public bool? BetaReportsActive { get; set; }

        /// <summary>
        /// Gets or sets the list of keychain access groups. A keychain access group is a group of applications,
        /// identified by an ID, that can share data.
        /// </summary>
        public IList<string> KeychainAccessGroups { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the team.
        /// </summary>
        public string TeamIdentifier { get; set; }

        /// <summary>
        /// Gets or sets a list of domains with which the app is assocated, to access specific services—such
        /// as Safari saved passwords and activity continuation, or universal links.
        /// </summary>
        public IList<string> AssociatedDomains { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Enables App Sandbox is enabled for a target in an Xcode project.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? AppSandbox { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is allowed access to group containers that are shared among multiple apps produced by a single development team, and allows certain additional interprocess communication between the apps.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public IList<string> ApplicationGroups { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is granted read-only access to the user’s Movies folder and iTunes movies.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? MoviesReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is granted read/write access to the user’s Movies folder and iTunes movies.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? MoviesReadWrite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is granted read-only access to the user’s Music folder.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? MusicReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is granted read/write access to the user’s Music folder.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? MusicReadWrite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is granted read-only access to the user’s Pictures folder.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? PicturesReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is granted read/write access to the user’s Pictures folder.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? PicturesReadWrite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is allowed to communicate with AVB devices.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? AudioVideoBridging { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is allowed to interact with Bluetooth devices.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? Bluetooth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is allowed to capture of movies and still images using the built-in camera, if available.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? Camera { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is allowed to interact with FireWire devices (currently, does not support interaction with audio/video devices such as DV cameras).
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? Firewire { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is allowed to record audio using the built-in microphone, if available, along with access to audio input using any Core Audio API that supports audio input.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? Microphone { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is allowed to interact with serial devices.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? Serial { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is allowed to interact with USB devices, including HID devices such as joysticks.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? Usb { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is granted read/write access to the user’s Downloads folder.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? DownloadsReadWrite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is granted to make use of app-scoped bookmarks and URLs.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? BookmarksAppScope { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is allows to make use of document-scoped bookmarks and URLs.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? BookmarksDocumentScope { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is granted read-only access to files the user has selected using an Open or Save dialog.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? UserSelectedFilesReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is granted read/write access to files the user has selected using an Open or Save dialog.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? UserSelectedFilesReadWrite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is allowed to write executable files.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? UserSelectedFilesExecutable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a child process inherits the parent’s sandbox.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? InheritSecurity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is granted access to
        /// network sockets for connecting to other machines.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? NetworkClient { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is granted access to
        /// network sockets for listening for incoming connections initiated by other machines.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? NetworkServer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is granted read/write access to contacts in the user’s address book; allows apps to infer the default address book if more than one is present on a system.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? AddressBook { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is granted read/write access to the user’s calendars.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? Calendars { get; set; }

        /// <summary>
        /// Gets or sets a value which indicates whether the application is allowd to make
        /// use of the Core Location framework for determining the computer’s geographical location.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? Location { get; set; }

        /// <summary>
        /// Gets or sets a value which indicates whether the application is able to print.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? Print { get; set; }

        /// <summary>
        /// Gets or sets a value which indicates whether the application is able to use specific AppleScript scripting access groups within a specific scriptable app.
        /// </summary>
        /// <remarks>
        /// This entitlement is specific to the OS X implementation of App Sandbox.
        /// It is not available in iOS.
        /// </remarks>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingAppSandbox.html#//apple_ref/doc/uid/TP40011195-CH4-SW19"/>
        public bool? ScriptingTargets { get; set; }

        /// <summary>
        /// Gets or sets the merchant IDs to use for in-app payments.
        /// </summary>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/ApplePayandPassKitEntitlements/ApplePayandPassKitEntitlements.html"/>
        public IList<string> InAppPayments { get; set; }

        /// <summary>
        /// Gets or sets the list of network extensions which are enabled.
        /// </summary>
        public IList<string> NetworkExtensions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether inter-app audio is enabled for this app. Inter-app audio allows your app to export audio that other apps can use.
        /// </summary>
        public bool? InterAppAudio { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Health Kit is enabled for this app.
        /// </summary>
        public bool? HealthKit { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the app, as used to support iCloud storage of key-value information for your app.
        /// </summary>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingiCloud.html"/>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "Standard iOS naming.")]
        public string iCloudKeyValueStore { get; set; }

        /// <summary>
        /// Gets or sets the bundle identifiers of the apps whose iCloud Document Store this app can access.
        /// </summary>
        /// <seealso href="https://developer.apple.com/library/content/documentation/Miscellaneous/Reference/EntitlementKeyReference/Chapters/EnablingiCloud.html"/>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "Standard iOS naming.")]
        public IList<string> iCloudDocumentStore { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether HomeKit is enabled for this app.
        /// </summary>
        public bool? HomeKit { get; set; }

        /// <summary>
        /// Gets or sets the default data protection rule applied to this app.
        /// </summary>
        public string DefaultDataProtection { get; set; }

        /// <summary>
        /// Gets or sets the VPN settings applied to this app.
        /// </summary>
        public IList<string> VpnApi { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SiriKit is enabled for this app.
        /// </summary>
        public bool? SiriKit { get; set; }

        /// <summary>
        /// Asynchronously reads a <see cref="Entitlements"/> object off a <see cref="NSDictionary"/>.
        /// </summary>
        /// <param name="propertyList">
        /// The dictionary which represents the entitlements.
        /// </param>
        /// <returns>
        /// A <see cref="Entitlements"/> object.
        /// </returns>
        public static Entitlements Read(NSDictionary propertyList)
        {
            if (propertyList == null)
            {
                throw new ArgumentNullException(nameof(propertyList));
            }

            return new Entitlements()
            {
                ApplicationIdentifier = propertyList.GetString("application-identifier"),
                GetTaskAllow = propertyList.GetNullableBoolean("get-task-allow"),
                ApsEnvironment = propertyList.GetString("aps-environment"),
                BetaReportsActive = propertyList.GetNullableBoolean("beta-reports-active"),
                KeychainAccessGroups = propertyList.GetStringArray("keychain-access-groups"),
                TeamIdentifier = propertyList.GetString("com.apple.developer.team-identifier"),
                AssociatedDomains = propertyList.GetStringArray("com.apple.developer.associated-domains"),
                AppSandbox = propertyList.GetNullableBoolean("com.apple.security.app-sandbox"),
                ApplicationGroups = propertyList.GetStringArray("com.apple.security.application-groups"),
                MoviesReadOnly = propertyList.GetNullableBoolean("com.apple.security.assets.movies.read-only"),
                MoviesReadWrite = propertyList.GetNullableBoolean("com.apple.security.assets.movies.read-write"),
                MusicReadOnly = propertyList.GetNullableBoolean("com.apple.security.assets.music.read-only"),
                MusicReadWrite = propertyList.GetNullableBoolean("com.apple.security.assets.music.read-write"),
                PicturesReadOnly = propertyList.GetNullableBoolean("com.apple.security.assets.pictures.read-only"),
                PicturesReadWrite = propertyList.GetNullableBoolean("com.apple.security.assets.pictures.read-write"),
                AudioVideoBridging = propertyList.GetNullableBoolean("com.apple.security.device.audio-video-bridging"),
                Bluetooth = propertyList.GetNullableBoolean("com.apple.security.device.bluetooth"),
                Camera = propertyList.GetNullableBoolean("com.apple.security.device.camera"),
                Firewire = propertyList.GetNullableBoolean("com.apple.security.device.firewire"),
                Microphone = propertyList.GetNullableBoolean("com.apple.security.device.microphone"),
                Serial = propertyList.GetNullableBoolean("com.apple.security.device.serial"),
                Usb = propertyList.GetNullableBoolean("com.apple.security.device.usb"),
                DownloadsReadWrite = propertyList.GetNullableBoolean("com.apple.security.files.downloads.read-write"),
                BookmarksAppScope = propertyList.GetNullableBoolean("com.apple.security.files.bookmarks.app-scope"),
                BookmarksDocumentScope = propertyList.GetNullableBoolean("com.apple.security.files.bookmarks.document-scope"),
                UserSelectedFilesReadOnly = propertyList.GetNullableBoolean("com.apple.security.files.user-selected.read-only"),
                UserSelectedFilesReadWrite = propertyList.GetNullableBoolean("com.apple.security.files.user-selected.read-write"),
                UserSelectedFilesExecutable = propertyList.GetNullableBoolean("com.apple.security.files.user-selected.executable"),
                InheritSecurity = propertyList.GetNullableBoolean("com.apple.security.inherit"),
                NetworkClient = propertyList.GetNullableBoolean("com.apple.security.network.client"),
                NetworkServer = propertyList.GetNullableBoolean("com.apple.security.network.server"),
                AddressBook = propertyList.GetNullableBoolean("com.apple.security.personal-information.addressbook"),
                Calendars = propertyList.GetNullableBoolean("com.apple.security.personal-information.calendars"),
                Location = propertyList.GetNullableBoolean("com.apple.security.personal-information.location"),
                Print = propertyList.GetNullableBoolean("com.apple.security.print"),
                ScriptingTargets = propertyList.GetNullableBoolean("com.apple.security.scripting-targets"),
                InAppPayments = propertyList.GetStringArray("com.apple.developer.in-app-payments"),
                NetworkExtensions = propertyList.GetStringArray("com.apple.developer.networking.networkextension"),
                InterAppAudio = propertyList.GetNullableBoolean("inter-app-audio"),
                HealthKit = propertyList.GetNullableBoolean("com.apple.developer.healthkit"),
                iCloudKeyValueStore = propertyList.GetString("com.apple.developer.ubiquity-kvstore-identifier"),
                iCloudDocumentStore = propertyList.GetStringArray("com.apple.developer.ubiquity-kvstore-identifier"),
                HomeKit = propertyList.GetNullableBoolean("com.apple.developer.homekit"),
                DefaultDataProtection = propertyList.GetString("com.apple.developer.default-data-protection"),
                VpnApi = propertyList.GetStringArray("com.apple.developer.networking.vpn.api"),
                SiriKit = propertyList.GetNullableBoolean("com.apple.developer.siri"),
            };
        }
    }
}
