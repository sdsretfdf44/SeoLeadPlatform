using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeoLeadPlatform.Domain.Entities;

namespace SeoLeadPlatform.Infrastructure.Persistence.Configurations;

public class OutreachCampaignConfiguration : IEntityTypeConfiguration<OutreachCampaign>
{
    public void Configure(EntityTypeBuilder<OutreachCampaign> builder)
    {
        builder.ToTable("outreach_campaigns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasMany(x => x.Messages).WithOne(m => m.OutreachCampaign).HasForeignKey(m => m.OutreachCampaignId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class OutreachMessageConfiguration : IEntityTypeConfiguration<OutreachMessage>
{
    public void Configure(EntityTypeBuilder<OutreachMessage> builder)
    {
        builder.ToTable("outreach_messages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.Subject).IsRequired().HasMaxLength(400);
        builder.Property(x => x.Body).IsRequired();
        builder.Property(x => x.PersonalizationData).HasMaxLength(4000);
        builder.HasIndex(x => x.ProspectCompanyId);
        builder.HasIndex(x => x.Status);
    }
}
