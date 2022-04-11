using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Claunia.PropertyList;
using Microsoft.Extensions.Logging.Abstractions;
using MobileDevices.iOS;
using MobileDevices.iOS.DiagnosticsRelay;
using MobileDevices.iOS.Install;
using MobileDevices.iOS.Lockdown;
using MobileDevices.iOS.Muxer;
using MobileDevices.iOS.PropertyLists;
using Moq;
using Xunit;

namespace MobileDevices.Tests.Install
{
    public class InstallClientTests
    {
        /// <summary>
        /// The <see cref="InstallClient"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new InstallClient(null, NullLogger<InstallClient>.Instance));

            Assert.Throws<ArgumentNullException>(() => new InstallClient(Stream.Null, null));

            Assert.Throws<ArgumentNullException>(() => new InstallClient(null));

        }

        /// <summary>
        /// Tests the <see cref="InstallClient.InstallAsync(string,InstallOption,CancellationToken)"/> method.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task InstallAsync_Works_Async()
        {
            var options = new InstallOption();
            var packagePath = "1234";

            var protocol = new Mock<PropertyListProtocol>();

            await using var client = new InstallClient(protocol.Object);

            protocol
                .Setup(c => c.WriteMessageAsync(It.IsAny<IPropertyList>(), default))
                .Callback((IPropertyList pl, CancellationToken ct) =>
                {
                    var request = Assert.IsType<InstallRequest>(pl);
                    Assert.Equal("Install", request.Command);
                    Assert.Equal(options, request.ClientOptions);
                    Assert.Equal(packagePath, request.PackagePath);

                })
                .Returns(Task.CompletedTask);

            await client.InstallAsync(packagePath, options, default).ConfigureAwait(false);

            protocol.Verify();
        }

        /// <summary>
        /// Tests the <see cref="InstallClient.UpgradeAsync(string,InstallOption,CancellationToken)"/> method.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task UpgradeAsync_Works_Async()
        {
            var options = new InstallOption();
            var packagePath = "1234";

            var protocol = new Mock<PropertyListProtocol>();

            await using var client = new InstallClient(protocol.Object);

            protocol
                .Setup(c => c.WriteMessageAsync(It.IsAny<IPropertyList>(), default))
                .Callback((IPropertyList pl, CancellationToken ct) =>
                {
                    var request = Assert.IsType<InstallRequest>(pl);
                    Assert.Equal("Upgrade", request.Command);
                    Assert.Equal(options, request.ClientOptions);
                    Assert.Equal(packagePath, request.PackagePath);

                })
                .Returns(Task.CompletedTask);

            await client.UpgradeAsync(packagePath, options, default).ConfigureAwait(false);

            protocol.Verify();
        }

        /// <summary>
        /// Tests the <see cref="InstallClient.UninstallAsync(string,InstallOption,CancellationToken)"/> method.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task UninstallAsync_Works_Async()
        {
            var options = new InstallOption();
            var applicationIdentifier = "1234.cn";

            var protocol = new Mock<PropertyListProtocol>();

            await using var client = new InstallClient(protocol.Object);

            protocol
                .Setup(c => c.WriteMessageAsync(It.IsAny<IPropertyList>(), default))
                .Callback((IPropertyList pl, CancellationToken ct) =>
                {
                    var request = Assert.IsType<InstallRequest>(pl);
                    Assert.Equal("Uninstall", request.Command);
                    Assert.Equal(options, request.ClientOptions);
                    Assert.Equal(applicationIdentifier, request.ApplicationIdentifier);

                })
                .Returns(Task.CompletedTask);

            await client.UninstallAsync(applicationIdentifier, options, default).ConfigureAwait(false);

            protocol.Verify();
        }

        /// <summary>
        /// Tests the <see cref="InstallClient.LookUpAsync(CancellationToken,InstallOption,string[])"/> method.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task LookUpAsync_Works_Async()
        {
            var options = new InstallOption();
            var applicationIdentifier = "1234.cn";
            var result = new NSDictionary { { "Status", new NSString("Success") } };

            var protocol = new Mock<PropertyListProtocol>();

            await using var client = new InstallClient(protocol.Object);

            protocol
                .Setup(c => c.WriteMessageAsync(It.IsAny<IPropertyList>(), default))
                .Callback((IPropertyList pl, CancellationToken ct) =>
                {
                    var request = Assert.IsType<InstallRequest>(pl);
                    Assert.Equal("Lookup", request.Command);
                    Assert.Equal(options, request.ClientOptions);
                    Assert.Equal(request.ClientOptions.BundleIDs[0], applicationIdentifier);

                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(c => c.ReadMessageAsync(default))
                .ReturnsAsync(result);

            await client.LookUpAsync(default, options, applicationIdentifier).ConfigureAwait(false);

            protocol.Verify();
        }

        /// <summary>
        /// Tests the <see cref="InstallClient.LookUpAsync(CancellationToken,InstallOption,string[])"/> method.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task LookUpAsync_NoOptions_Works_Async()
        {
            var applicationIdentifier = "1234.cn";
            var result = new NSDictionary { { "Status", new NSString("Success") } };

            var protocol = new Mock<PropertyListProtocol>();

            await using var client = new InstallClient(protocol.Object);

            protocol
                .Setup(c => c.WriteMessageAsync(It.IsAny<IPropertyList>(), default))
                .Callback((IPropertyList pl, CancellationToken ct) =>
                {
                    var request = Assert.IsType<InstallRequest>(pl);
                    Assert.Equal("Lookup", request.Command);
                    Assert.True(request.ClientOptions.BundleIDs.Any());
                    Assert.Equal(request.ClientOptions.BundleIDs[0], applicationIdentifier);
                })
                .Returns(Task.CompletedTask);

            protocol
                .Setup(c => c.ReadMessageAsync(default))
                .ReturnsAsync(result);

            await client.LookUpAsync(default, null, applicationIdentifier).ConfigureAwait(false);

            protocol.Verify();
        }

        /// <summary>
        /// Tests the <see cref="InstallClient.InstallCallbackAsync(Action&lt;InstallResponse&gt;,CancellationToken)"/> method.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task InstallCallbackAsync_Works_Async()
        {
            var result = new NSDictionary { { "Status", new NSString("Complete") }, { "PercentComplete", 100 } };

            var protocol = new Mock<PropertyListProtocol>();

            await using var client = new InstallClient(protocol.Object);

            protocol
                .Setup(c => c.ReadMessageAsync(default))
                .ReturnsAsync(result);

            await client.InstallCallbackAsync(r =>
            {
                Assert.Equal(100, r.PercentComplete);
                Assert.Equal("Complete", r.Status);
            }, default).ConfigureAwait(false);

            protocol.Verify();
        }

        /// <summary>
        /// Tests the <see cref="InstallClient.InstallCallbackAsync(Action&lt;InstallResponse&gt;,CancellationToken)"/> method.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task InstallCallbackAsync_InstallError_Works_Async()
        {
            var result = new NSDictionary { { "Error", new NSString("Error") }, { "PercentComplete", 0 }, { "ErrorDescription", "test error message" } };

            var protocol = new Mock<PropertyListProtocol>();

            await using var client = new InstallClient(protocol.Object);

            protocol
                .Setup(c => c.ReadMessageAsync(default))
                .ReturnsAsync(result);

            await client.InstallCallbackAsync(r =>
            {
                Assert.Equal("Error", r.Error);
                Assert.Equal("test error message", r.ErrorDescription);

            }, default).ConfigureAwait(false);

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="InstallClient.InstallCallbackAsync(Action&lt;InstallResponse&gt;,CancellationToken)"/> returns <see message="null"/>
        /// when the remote end closes the connection unexpectedly or the message returns null
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task InstallCallbackAsync_MessageNull_Works_Async()
        {
            var result = new NSDictionary();
            var protocol = new Mock<PropertyListProtocol>();

            await using var client = new InstallClient(protocol.Object);

            protocol
                .Setup(c => c.ReadMessageAsync(default))
                .ReturnsAsync(result);

            await client.InstallCallbackAsync(null, default).ConfigureAwait(false);

            protocol.Verify();
        }

    }
}
