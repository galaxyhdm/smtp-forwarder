using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmtpForwarder.DataLayer.Interfaces;
using SmtpForwarder.Domain;

namespace SmtpForwarder.DataLayer;

public class AppDbContext : DbContext, IAppContext
{

    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<MailBox> MailBoxes { get; set; }
    public DbSet<ForwardTarget> ForwardTargets { get; set; }
    public DbSet<ForwardingAddress> ForwardingAddresses { get; set; }
    public DbSet<TraceLog> TraceLogs { get; set; }
    public DbSet<TraceLogEntry> TraceLogEntries { get; set; }

    public async Task<bool> CanConnectAsync() => await Database.CanConnectAsync();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Error);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

}