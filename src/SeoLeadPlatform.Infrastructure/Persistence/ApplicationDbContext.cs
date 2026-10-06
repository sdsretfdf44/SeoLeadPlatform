using Microsoft.EntityFrameworkCore;
using SeoLeadPlatform.Application.Abstractions;
using SeoLeadPlatform.Domain.Entities;

namespace SeoLeadPlatform.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Agency> Agencies => Set<Agency>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ServiceOffering> ServiceOfferings => Set<ServiceOffering>();
    public DbSet<ProspectCompany> Prospects => Set<ProspectCompany>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<SeoAudit> SeoAudits => Set<SeoAudit>();
    public DbSet<SeoIssue> SeoIssues => Set<SeoIssue>();
    public DbSet<OpportunityScore> OpportunityScores => Set<OpportunityScore>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<OutreachCampaign> OutreachCampaigns => Set<OutreachCampaign>();
    public DbSet<OutreachMessage> OutreachMessages => Set<OutreachMessage>();
    public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<PublicAuditReportToken> PublicAuditReportTokens => Set<PublicAuditReportToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var idProperty = entity.FindProperty("Id");
            if (idProperty is not null && idProperty.ClrType == typeof(Guid))
            {
                idProperty.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never;
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}
