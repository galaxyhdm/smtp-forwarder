using Application.Interfaces.Repositories;
using DataLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataLayer.Extensions;

public static class AppContextService
{
    public static IServiceCollection AddAppContext(this IServiceCollection services, string connectionString) {
        services.AddDbContext<AppDbContext>(
            options => {
                options.UseNpgsql(connectionString)
                    .UseSnakeCaseNamingConvention()
                    .EnableSensitiveDataLogging(true)
                    .EnableDetailedErrors(true);
            }, ServiceLifetime.Transient, ServiceLifetime.Transient);
        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<IForwardingAddressRepository, ForwardingAddressRepository>();
        services.AddTransient<IForwardTargetRepository, ForwardTargetRepository>();
        services.AddTransient<IMailBoxRepository, MailBoxRepository>();
        services.AddTransient<IUserRepository, UserRepository>();
        return services;
    }
}