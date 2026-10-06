using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeoLeadPlatform.Domain.Entities;

namespace SeoLeadPlatform.Infrastructure.Persistence.Configurations;

public class SeoAuditConfiguration : IEntityTypeConfiguration<SeoAudit>
{
    public void Configure(EntityTypeBuilder<SeoAudit> builder)
    {
        builder.ToTable("seo_audits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.WebsiteUrl).IsRequired().HasMaxLength(400);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.FailureReason).HasMaxLength(1000);
        builder.Property(x => x.AnalysisSummary).HasMaxLength(4000);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.ProspectCompanyId);

        builder.HasMany(x => x.Issues).WithOne(i => i.SeoAudit).HasForeignKey(i => i.SeoAuditId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.OpportunityScore).WithOne(o => o.SeoAudit!).HasForeignKey<OpportunityScore>(o => o.SeoAuditId).OnDelete(DeleteBehavior.Cascade);
    }
}
