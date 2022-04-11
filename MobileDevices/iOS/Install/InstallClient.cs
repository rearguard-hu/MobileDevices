using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Claunia.PropertyList;
using Microsoft.Extensions.Logging;
using MobileDevices.iOS.DiagnosticsRelay;
using MobileDevices.iOS.PropertyLists;

namespace MobileDevices.iOS.Install
{
    /// <summary>
    /// A client which interacts with the installation proxy service running on an iOS device.
    /// </summary>
    public class InstallClient : IAsyncDisposable
    {
        /// <summary>
        /// Gets the name of the installation proxy service running on the device.
        /// </summary>
        public const string ServiceName = "com.apple.mobile.installation_proxy";

        private readonly PropertyListProtocol protocol;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallClient"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents a connection to the installation proxy service running on the device.
        /// </param>
        /// <param name="logger">
        /// A logger which can be used when logging.
        /// </param>
        public InstallClient(Stream stream, ILogger<InstallClient> logger)
        {
            this.protocol = new PropertyListProtocol(stream, ownsStream: true, logger: logger);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallClient"/> class.
        /// </summary>
        /// <param name="protocol">
        /// A <see cref="PropertyListProtocol"/> which represents a connection to the installation proxy service running on the device.
        /// </param>
        public InstallClient(PropertyListProtocol protocol)
        {
            this.protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
        }

        public Task InstallAsync(string packagePath, InstallOption options, CancellationToken token)
        {
            return this.protocol.WriteMessageAsync(
                new InstallRequest() {
                    Command = "Install",
                    ClientOptions = options,
                    PackagePath = packagePath
                },
                token);

        }

        public virtual Task UpgradeAsync(string packagePath, InstallOption options, CancellationToken token)
        {
            return this.protocol.WriteMessageAsync(
                new InstallRequest()
                {
                    Command = "Upgrade",
                    ClientOptions = options,
                    PackagePath = packagePath
                },
                token);
        }

        public virtual Task UninstallAsync(string appId, InstallOption options, CancellationToken token)
        {
            return this.protocol.WriteMessageAsync(
                new InstallRequest()
                {
                    Command = "Uninstall",
                    ClientOptions = options,
                    ApplicationIdentifier = appId
                },
                token);
        }

        public virtual Task<NSDictionary> LookUpAsync(CancellationToken token, InstallOption options, params string[] appIds)
        {
            options ??= new InstallOption();
            options.BundleIDs = appIds;

            return ExecuteRequestAsync(
                new InstallRequest()
                {
                    Command = "Lookup",
                    ClientOptions = options
                },
                token);
        }

        public async Task<bool> InstallCallbackAsync(Action<InstallResponse> action, CancellationToken token)
        {

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var dict = await protocol.ReadMessageAsync(token);

                    if (dict == null || dict.IsEmpty) return false;


                    var installProgress = new InstallResponse();
                    installProgress.FromDictionary(dict);

                    action?.Invoke(installProgress);

                    if (string.IsNullOrEmpty(installProgress.Status) ||
                        installProgress.Status.Equals("Complete"))
                    {
                        return true;
                    }

                    if (!string.IsNullOrEmpty(installProgress.Error))
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }


        private async Task<NSDictionary> ExecuteRequestAsync(
            IPropertyList request,
            CancellationToken cancellationToken)
        {
            await protocol.WriteMessageAsync(request, cancellationToken);

            var response = await this.protocol.ReadMessageAsync(cancellationToken).ConfigureAwait(false);

            return response;
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            return this.protocol.DisposeAsync();
        }

    }
}