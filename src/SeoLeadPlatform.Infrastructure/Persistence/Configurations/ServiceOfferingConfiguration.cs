using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeoLeadPlatform.Domain.Entities;

namespace SeoLeadPlatform.Infrastructure.Persistence.Configurations;

public class ServiceOfferingConfiguration : IEntityTypeConfiguration<ServiceOffering>
{
    public void Configure(EntityTypeBuilder<ServiceOffering> builder)
    {
        builder.ToTable("service_offerings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(160);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.TypicalProjectValue).HasPrecision(18, 2);
        builder.Property(x => x.TypicalMonthlyRetainer).HasPrecision(18, 2);
        builder.HasIndex(x => x.AgencyId);
    }
}
