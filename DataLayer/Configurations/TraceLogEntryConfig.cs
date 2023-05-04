using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmtpForwarder.Domain;

namespace SmtpForwarder.DataLayer.Configurations;

public class TraceLogEntryConfig : IEntityTypeConfiguration<TraceLogEntry>
{

    public void Configure(EntityTypeBuilder<TraceLogEntry> builder)
    {
        builder.ToTable("trace_log_entries");

        builder.Property(t => t.Id)
            .IsRequired();
        
        builder.Property(t => t.TraceTime)
            .IsRequired();
        
        builder.Property(t => t.ProcessCode)
            .IsRequired();

        builder.Property(t => t.Step)
            .IsRequired();

        builder.Property(t => t.Message)
            .IsRequired();

        builder.Property(t => t.IsEnd)
            .HasDefaultValue(false)
            .IsRequired();
        
        builder.Property(t => t.LastUpdatedUtc)
            .IsRequired();
        builder.Property(t => t.CreatedUtc)
            .IsRequired();
        
        // keys and indexes
        builder.HasKey(t => t.Id);
        
        // relationships
        builder.HasOne(t => t.TraceLog)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired()
            .HasForeignKey(t => t.TraceLogId);
    }
}