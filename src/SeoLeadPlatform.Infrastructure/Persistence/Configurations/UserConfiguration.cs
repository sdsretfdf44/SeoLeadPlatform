using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeoLeadPlatform.Domain.Entities;

namespace SeoLeadPlatform.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(200);
        builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(400);
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(160);
        builder.Property(x => x.Role).IsRequired().HasMaxLength(60);
        builder.HasIndex(x => x.Email).IsUnique();
    }
}
