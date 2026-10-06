using Microsoft.EntityFrameworkCore;
using SeoLeadPlatform.Domain.Entities;

namespace SeoLeadPlatform.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Domain.Entities.Agency> Agencies { get; }
    DbSet<User> Users { get; }
    DbSet<ServiceOffering> ServiceOfferings { get; }
    DbSet<ProspectCompany> Prospects { get; }
    DbSet<Contact> Contacts { get; }
    DbSet<SeoAudit> SeoAudits { get; }
    DbSet<SeoIssue> SeoIssues { get; }
    DbSet<OpportunityScore> OpportunityScores { get; }
    DbSet<Lead> Leads { get; }
    DbSet<OutreachCampaign> OutreachCampaigns { get; }
    DbSet<OutreachMessage> OutreachMessages { get; }
    DbSet<Proposal> Proposals { get; }
    DbSet<ActivityLog> ActivityLogs { get; }
    DbSet<PublicAuditReportToken> PublicAuditReportTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
