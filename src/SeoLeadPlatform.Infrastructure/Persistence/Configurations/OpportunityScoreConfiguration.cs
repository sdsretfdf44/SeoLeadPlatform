using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeoLeadPlatform.Domain.Entities;

namespace SeoLeadPlatform.Infrastructure.Persistence.Configurations;

public class OpportunityScoreConfiguration : IEntityTypeConfiguration<OpportunityScore>
{
    public void Configure(EntityTypeBuilder<OpportunityScore> builder)
    {
        builder.ToTable("opportunity_scores");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Priority).HasConversion<int>();
        builder.Property(x => x.Reasoning).HasMaxLength(4000);
        builder.Property(x => x.FactorsJson).HasColumnType("nvarchar(max)").HasDefaultValue("[]");
    }
}
