using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmtpForwarder.Domain;

namespace SmtpForwarder.DataLayer.Configurations;

public class ForwardingAddressConfig : IEntityTypeConfiguration<ForwardingAddress>
{

    public void Configure(EntityTypeBuilder<ForwardingAddress> builder)
    {
        builder.ToTable("forwarding_addresses");

        builder.Property(f => f.Id)
            .IsRequired();

        builder.Property(f => f.LocalAddressPart)
            .IsRequired();
        
        builder.Property(f => f.Enabled)
            .IsRequired();

        builder.Property(f => f.DeleteTimeUtc);
        
        builder.Property(f => f.LastUpdatedUtc)
            .IsRequired();
        builder.Property(f => f.CreatedUtc)
            .IsRequired();

        // keys and indexes
        builder.HasKey(f => f.Id);
        builder.HasIndex(f => f.LocalAddressPart).IsUnique();

        // relationships
        builder.HasOne(f => f.Owner)
            .WithMany()
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false)
            .HasForeignKey(f => f.OwnerId);

        builder.HasOne(f => f.ForwardTarget)
            .WithMany(t => t.ForwardingAddresses)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false)
            .HasForeignKey(f => f.ForwardTargetId);
    }
}