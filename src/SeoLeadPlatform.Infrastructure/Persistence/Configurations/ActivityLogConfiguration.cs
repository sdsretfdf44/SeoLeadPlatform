using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeoLeadPlatform.Domain.Entities;

namespace SeoLeadPlatform.Infrastructure.Persistence.Configurations;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("activity_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasConversion<int>();
        builder.Property(x => x.Description).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.Metadata).HasMaxLength(4000);
        builder.HasIndex(x => new { x.AgencyId, x.CreatedAt });
        builder.HasIndex(x => x.LeadId);
        builder.HasIndex(x => x.ProspectCompanyId);
    }
}
