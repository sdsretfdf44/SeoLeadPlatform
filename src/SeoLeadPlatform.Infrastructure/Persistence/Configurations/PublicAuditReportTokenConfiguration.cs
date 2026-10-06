using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeoLeadPlatform.Domain.Entities;

namespace SeoLeadPlatform.Infrastructure.Persistence.Configurations;

public class PublicAuditReportTokenConfiguration : IEntityTypeConfiguration<PublicAuditReportToken>
{
    public void Configure(EntityTypeBuilder<PublicAuditReportToken> builder)
    {
        builder.ToTable("public_audit_report_tokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Token).IsRequired().HasMaxLength(80);
        builder.HasIndex(x => x.Token).IsUnique();
        builder.HasOne(x => x.SeoAudit).WithMany().HasForeignKey(x => x.SeoAuditId).OnDelete(DeleteBehavior.Cascade);
    }
}
