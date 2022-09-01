using Application.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DataLayer;

internal class SqlDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{

    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Env.GetStringRequired("SQL_CONNECTION");
        var options = new DbContextOptionsBuilder()
            .UseNpgsql(connectionString, builder =>
                builder.MigrationsHistoryTable("_EFMigrationsHistory"))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new AppDbContext(options);
    }
}