using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeoLeadPlatform.Domain.Entities;

namespace SeoLeadPlatform.Infrastructure.Persistence.Configurations;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("leads");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.Priority).HasConversion<int>();
        builder.Property(x => x.EstimatedProjectValue).HasPrecision(18, 2);
        builder.Property(x => x.EstimatedMonthlyRetainer).HasPrecision(18, 2);
        builder.Property(x => x.Notes).HasMaxLength(4000);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.OpportunityScore);
        builder.HasIndex(x => x.NextFollowUpAt);
        builder.HasIndex(x => x.AgencyId);
    }
}
