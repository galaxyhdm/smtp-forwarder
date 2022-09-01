using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.Configurations;

public class UserConfig : IEntityTypeConfiguration<User>
{

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.Property(u => u.UserId)
            .IsRequired();

        builder.Property(u => u.Username)
            .IsRequired();

        builder.Property(u => u.DisplayName);

        builder.Property(u => u.IsAdmin)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .IsRequired();
        
        // keys and indexes
        builder.HasKey(u => u.UserId);
        builder.HasIndex(u => u.Username).IsUnique();

    }
}