using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmtpForwarder.Domain;

namespace SmtpForwarder.DataLayer.Configurations;

public class MailBoxConfig : IEntityTypeConfiguration<MailBox>
{

    public void Configure(EntityTypeBuilder<MailBox> builder)
    {
        builder.ToTable("mail_boxes");

        builder.Property(m => m.MailBoxId)
            .IsRequired();

        builder.Property(m => m.LocalAddressPart)
            .IsRequired();

        builder.Property(m => m.AuthName)
            .IsRequired();

        builder.Property(m => m.PasswordHash)
            .IsRequired();

        builder.Property(m => m.Enabled)
            .IsRequired();

        builder.Property(m => m.DeleteTimeUtc);
        
        builder.Property(u => u.LastUpdatedUtc)
            .IsRequired();
        builder.Property(u => u.CreatedUtc)
            .IsRequired();
        
        // keys and indexes
        builder.HasKey(m => m.MailBoxId);
        builder.HasIndex(m => m.LocalAddressPart).IsUnique();
        builder.HasIndex(m => m.AuthName);

        // relationships
        builder.HasOne(m => m.Owner)
            .WithMany()
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false)
            .HasForeignKey(m => m.OwnerId);
    }
}