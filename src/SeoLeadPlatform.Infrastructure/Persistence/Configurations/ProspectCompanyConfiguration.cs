using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeoLeadPlatform.Domain.Entities;

namespace SeoLeadPlatform.Infrastructure.Persistence.Configurations;

public class ProspectCompanyConfiguration : IEntityTypeConfiguration<ProspectCompany>
{
    public void Configure(EntityTypeBuilder<ProspectCompany> builder)
    {
        builder.ToTable("prospect_companies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CompanyName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.WebsiteUrl).IsRequired().HasMaxLength(400);
        builder.Property(x => x.Industry).HasMaxLength(120);
        builder.Property(x => x.Country).HasMaxLength(80);
        builder.Property(x => x.State).HasMaxLength(80);
        builder.Property(x => x.City).HasMaxLength(80);
        builder.Property(x => x.CompanySize).HasMaxLength(40);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.PhoneNumber).HasMaxLength(60);
        builder.Property(x => x.PrimaryEmail).HasMaxLength(200);
        builder.Property(x => x.LinkedInUrl).HasMaxLength(400);
        builder.Property(x => x.Source).HasConversion<int>();

        builder.HasIndex(x => new { x.AgencyId, x.WebsiteUrl }).IsUnique();
        builder.HasIndex(x => x.Industry);
        builder.HasIndex(x => x.Country);

        builder.HasMany(x => x.Contacts).WithOne(c => c.ProspectCompany).HasForeignKey(c => c.ProspectCompanyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Audits).WithOne(a => a.ProspectCompany).HasForeignKey(a => a.ProspectCompanyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Lead).WithOne(l => l.ProspectCompany!).HasForeignKey<Lead>(l => l.ProspectCompanyId).OnDelete(DeleteBehavior.Cascade);
    }
}
