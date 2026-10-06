using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeoLeadPlatform.Domain.Entities;

namespace SeoLeadPlatform.Infrastructure.Persistence.Configurations;

public class AgencyConfiguration : IEntityTypeConfiguration<Agency>
{
    public void Configure(EntityTypeBuilder<Agency> builder)
    {
        builder.ToTable("agencies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(160);
        builder.Property(x => x.WebsiteUrl).HasMaxLength(400);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.TargetIndustries).HasMaxLength(1000);
        builder.Property(x => x.TargetLocations).HasMaxLength(1000);
        builder.Property(x => x.TargetServices).HasMaxLength(1000);
        builder.Property(x => x.PreferredProjectTypes).HasMaxLength(1000);
        builder.Property(x => x.MinimumProjectValue).HasPrecision(18, 2);

        builder.HasMany(x => x.Users).WithOne(u => u.Agency).HasForeignKey(u => u.AgencyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.ServiceOfferings).WithOne().HasForeignKey(o => o.AgencyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Prospects).WithOne(p => p.Agency!).HasForeignKey(p => p.AgencyId).OnDelete(DeleteBehavior.Cascade);
    }
}
