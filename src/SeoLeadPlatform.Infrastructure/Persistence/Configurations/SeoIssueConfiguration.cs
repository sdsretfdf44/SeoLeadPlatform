using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeoLeadPlatform.Domain.Entities;

namespace SeoLeadPlatform.Infrastructure.Persistence.Configurations;

public class SeoIssueConfiguration : IEntityTypeConfiguration<SeoIssue>
{
    public void Configure(EntityTypeBuilder<SeoIssue> builder)
    {
        builder.ToTable("seo_issues");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Category).HasConversion<int>();
        builder.Property(x => x.Severity).HasConversion<int>();
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Recommendation).HasMaxLength(2000);
        builder.Property(x => x.Evidence).HasMaxLength(2000);
        builder.Property(x => x.EstimatedImpact).HasMaxLength(400);
    }
}
