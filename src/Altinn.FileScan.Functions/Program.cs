using Altinn.Common.AccessTokenClient.Services;
using Altinn.FileScan.Functions.Clients.Interfaces;
using Altinn.FileScan.Functions.Clients;
using Altinn.FileScan.Functions.Configuration;
using Altinn.FileScan.Functions.Services.Interfaces;
using Altinn.FileScan.Functions.Services;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altinn.FileScan.Functions
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(s =>
                {
                    s.AddOptions<PlatformSettings>()
                        .Configure<IConfiguration>((settings, configuration) =>
                    {
                        configuration.GetSection("Platform").Bind(settings);
                    });
                    s.AddOptions<KeyVaultSettings>()
                    .Configure<IConfiguration>((settings, configuration) =>
                    {
                        configuration.GetSection("KeyVault").Bind(settings);
                    });
                    s.AddSingleton<IKeyVaultService, KeyVaultService>();
                    s.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
                    s.AddSingleton<IAccessTokenGenerator, AccessTokenGenerator>();
                    s.AddHttpClient<IFileScanClient, FileScanClient>();
                })
                .Build();

            host.Run();
        }
    }
}