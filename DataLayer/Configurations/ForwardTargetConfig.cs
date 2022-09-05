using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.Configurations;

public class ForwardTargetConfig : IEntityTypeConfiguration<ForwardTarget>
{

    public void Configure(EntityTypeBuilder<ForwardTarget> builder)
    {
        builder.ToTable("forward_targets");

        builder.Property(f => f.Id)
            .IsRequired();

        builder.Property(f => f.ForwarderName)
            .IsRequired();

        builder.Property(f => f.ForwarderSettings)
            .IsRequired();

        builder.Property(f => f.Enabled)
            .IsRequired();

        builder.Property(f => f.LastUpdatedUtc)
            .IsRequired();
        builder.Property(f => f.CreatedUtc)
            .IsRequired();
        
        // keys and indexes
        builder.HasKey(f => f.Id);
        builder.HasIndex(f => f.Name);

        // relationships
        builder.HasOne(f => f.Owner)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired()
            .HasForeignKey(f => f.OwnerId);
    }
}