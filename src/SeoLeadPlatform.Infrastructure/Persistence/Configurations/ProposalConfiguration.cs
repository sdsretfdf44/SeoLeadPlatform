using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeoLeadPlatform.Domain.Entities;

namespace SeoLeadPlatform.Infrastructure.Persistence.Configurations;

public class ProposalConfiguration : IEntityTypeConfiguration<Proposal>
{
    public void Configure(EntityTypeBuilder<Proposal> builder)
    {
        builder.ToTable("proposals");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ExecutiveSummary).HasMaxLength(4000);
        builder.Property(x => x.ProblemsIdentified).HasMaxLength(4000);
        builder.Property(x => x.RecommendedServices).HasMaxLength(4000);
        builder.Property(x => x.Deliverables).HasMaxLength(4000);
        builder.Property(x => x.Timeline).HasMaxLength(2000);
        builder.Property(x => x.Terms).HasMaxLength(4000);
        builder.Property(x => x.Pricing).HasPrecision(18, 2);
        builder.Property(x => x.MonthlyRetainer).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.PublicToken).HasMaxLength(80);
        builder.HasIndex(x => x.PublicToken).IsUnique().HasFilter("[PublicToken] IS NOT NULL");

        builder.HasOne(x => x.ProspectCompany).WithMany().HasForeignKey(x => x.ProspectCompanyId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Lead).WithMany().HasForeignKey(x => x.LeadId).OnDelete(DeleteBehavior.SetNull);
    }
}
