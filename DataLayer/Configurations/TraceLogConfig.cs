using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmtpForwarder.Domain;

namespace SmtpForwarder.DataLayer.Configurations;

public class TraceLogConfig : IEntityTypeConfiguration<TraceLog>
{

    public void Configure(EntityTypeBuilder<TraceLog> builder)
    {
        builder.ToTable("trace_logs");

        builder.Property(t => t.Id)
            .IsRequired();

        builder.Property(t => t.ProcessIdentifier)
            .IsRequired();

        builder.Property(t => t.ApplicationVersion)
            .IsRequired();

        builder.Property(t => t.StartTime)
            .IsRequired();
        builder.Property(t => t.EndTime)
            .IsRequired();

        builder.Property(t => t.Ended)
            .HasDefaultValue(false)
            .IsRequired();
        
        builder.Property(t => t.LastUpdatedUtc)
            .IsRequired();
        builder.Property(t => t.CreatedUtc)
            .IsRequired();
        
        // keys and indexes
        builder.HasKey(t => t.Id);
        builder.HasIndex(t => t.ProcessIdentifier).IsUnique();
        
        // relationships
        builder.HasOne(t => t.MailBox)
            .WithMany()
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false)
            .HasForeignKey(t => t.MailBoxId);

    }
}