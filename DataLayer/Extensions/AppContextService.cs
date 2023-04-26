using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmtpForwarder.Application.Interfaces.Repositories;
using SmtpForwarder.DataLayer.Repositories;

namespace SmtpForwarder.DataLayer.Extensions;

public static class AppContextService
{
    public static IServiceCollection AddAppContext(this IServiceCollection services, string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("String could not be null or empty.", nameof(connectionString)); 
        
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