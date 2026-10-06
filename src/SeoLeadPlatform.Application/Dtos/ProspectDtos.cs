using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Application.Dtos;

public class ProspectListItemDto
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public int? OpportunityScore { get; set; }
    public LeadStatus? LeadStatus { get; set; }
    public LeadPriority? LeadPriority { get; set; }
    public SeoAuditStatus? LatestAuditStatus { get; set; }
    public int? OverallSeoScore { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProspectDetailDto
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? CompanySize { get; set; }
    public string? Description { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? LinkedInUrl { get; set; }
    public ProspectSource Source { get; set; }

    public LeadDto? Lead { get; set; }
    public SeoAuditSummaryDto? LatestAudit { get; set; }
    public List<ContactDto> Contacts { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class ContactDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? LinkedInUrl { get; set; }
    public bool IsDecisionMaker { get; set; }
}

public class LeadDto
{
    public Guid Id { get; set; }
    public LeadStatus Status { get; set; }
    public LeadPriority Priority { get; set; }
    public int OpportunityScore { get; set; }
    public decimal? EstimatedProjectValue { get; set; }
    public decimal? EstimatedMonthlyRetainer { get; set; }
    public int Probability { get; set; }
    public DateTime? NextFollowUpAt { get; set; }
    public string? Notes { get; set; }
}

public class SeoAuditSummaryDto
{
    public Guid Id { get; set; }
    public SeoAuditStatus Status { get; set; }
    public int OverallScore { get; set; }
    public int TechnicalScore { get; set; }
    public int OnPageScore { get; set; }
    public int ContentScore { get; set; }
    public int PerformanceScore { get; set; }
    public int LocalSeoScore { get; set; }
    public int IssueCount { get; set; }
    public int CriticalIssueCount { get; set; }
    public int HighIssueCount { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class CreateProspectRequest
{
    public string CompanyName { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? CompanySize { get; set; }
    public string? Description { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? LinkedInUrl { get; set; }
    public ProspectSource Source { get; set; } = ProspectSource.Manual;
}

public class UpdateProspectRequest
{
    public string CompanyName { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? CompanySize { get; set; }
    public string? Description { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? LinkedInUrl { get; set; }
}

public class ProspectSearchQuery
{
    public string? Search { get; set; }
    public string? Industry { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public LeadStatus? LeadStatus { get; set; }
    public LeadPriority? Priority { get; set; }
    public int? MinOpportunityScore { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}
