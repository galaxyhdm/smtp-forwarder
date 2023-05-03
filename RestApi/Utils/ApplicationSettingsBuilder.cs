using SmtpForwarder.Application.Utils;

namespace SmtpForwarder.RestApi.Utils;

public static class ApplicationSettingsBuilder
{

    public static WebApplicationBuilder ConfigureAppSettings(this WebApplicationBuilder host)
    {
        var environment = host.Environment.EnvironmentName;

        host.Configuration
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"appsettings.{environment}.json", true, true)
            .AddJsonFile($"appsettings.{Environment.MachineName}.json", true, true)
            .AddEnvironmentVariables("APP_");
        
        return host;
    }
    
}