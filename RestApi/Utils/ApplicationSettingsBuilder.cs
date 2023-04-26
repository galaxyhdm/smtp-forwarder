using SmtpForwarder.Application.Utils;

namespace SmtpForwarder.RestApi.Utils;

public static class ApplicationSettingsBuilder
{

    public static IHostBuilder ConfigureAppSettings(this IHostBuilder host)
    {
        var environment = Env.GetStringDefault("ASPNETCORE_ENVIRONMENT");

        host.ConfigureAppConfiguration((ctx, builder) =>
        {
            builder.AddJsonFile("appsettings.json", false, true);
            builder.AddJsonFile($"appsettings.{environment}.json", true, true);
            builder.AddJsonFile($"appsettings.{Environment.MachineName}.json", true, true);
            
            builder.AddEnvironmentVariables("APP_");
        });
        
        return host;
    }
    
}