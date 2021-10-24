using MobileDevices.iOS.Lockdown;
using MobileDevices.iOS.MobileImageMounter;
using MobileDevices.iOS.Muxer;
using MobileDevices.iOS.NotificationProxy;
using MobileDevices.iOS.PropertyLists;
using MobileDevices.iOS.Workers;
using Microsoft.Extensions.DependencyInjection;
using MobileDevices.iOS.Activation;
using MobileDevices.iOS.AfcServices;
using MobileDevices.iOS.CrashReport;
using MobileDevices.iOS.DiagnosticsRelay;
using MobileDevices.iOS.Install;

namespace MobileDevices.iOS.DependencyInjection
{
    /// <summary>
    /// Provides extension methods for the <see cref="IServiceCollection"/> interface.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds iOS-related functionality to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">
        /// The <see cref="IServiceCollection"/> to add services to.
        /// </param>
        /// <returns>
        /// The <see cref="IServiceCollection"/> so that additional calls can be chained.
        /// </returns>
        public static IServiceCollection AddAppleServices(this IServiceCollection services)
        {
            services.AddTransient<PairingRecordProvisioner>();

            services.AddSingleton<MuxerSocketLocator>();
            services.AddSingleton<MuxerClient>();
            services.AddSingleton<PairingRecordGenerator>();
            services.AddSingleton<DeviceServiceProvider>();


            services.AddScoped<DeviceContext>();
            services.AddScoped<PairingWorker>();


            services.AddTransient<ClientFactory<LockdownClient>, LockdownClientFactory>();
            services.AddTransient<ClientFactory<NotificationProxyClient>, NotificationProxyClientFactory>();
            services.AddTransient<ClientFactory<MobileImageMounterClient>, MobileImageMounterClientFactory>();
            services.AddTransient<ClientFactory<AfcClient>, AfcClientFactory>();
            services.AddTransient<ClientFactory<CrashReportClient>, CrashReportClientFactory>();
            services.AddTransient<ClientFactory<CrashReportCopyClient>, CrashReportCopyClientFactory>();
            services.AddTransient<ClientFactory<InstallClient>, InstallClientFactory>();
            services.AddTransient<ClientFactory<DiagnosticsRelayClient>, DiagnosticsRelayClientFactory>();
            services.AddTransient<ClientFactory<MobileActivationClient>, MobileActivationClientFactory>();


            services.AddTransient<PropertyListProtocolFactory>();
            services.AddTransient<AfcProtocolFactory>();
            services.AddTransient<DeviceActivation>();
            return services;
        }


    }
}
