using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using SmtpForwarder.Application.Interfaces.Repositories;
using SmtpForwarder.DataLayer.Repositories;

namespace SmtpForwarder.DataLayer.Extensions;

public static class AppContextService
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    public static IServiceCollection AddAppContext(this IServiceCollection services, string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("String could not be null or empty.", nameof(connectionString)); 
        
        services.AddDbContext<AppDbContext>(
            options => {
                options.UseNpgsql(connectionString, builder => 
                        builder.MigrationsHistoryTable("_EFMigrationsHistory"))
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
        services.AddTransient<ITraceLogRepository, TraceLogRepository>();
        services.AddTransient<ITraceLogEntryRepository, TraceLogEntryRepository>();
        return services;
    }

    public static void MigrateDatabase(this IServiceProvider serviceProvider)
    {
        // Migrate latest database changes during startup
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
        if (!pendingMigrations.Any())
            return;
        
        Log.Debug("Migrating database...");
        
        foreach (var pendingMigration in pendingMigrations)
            Log.Trace("PendingMigration: {}", pendingMigration);
        
        dbContext.Database.Migrate();
        Log.Debug("Finished database migration.");
    }
}