namespace SeoLeadPlatform.Application.Dtos;

public class DashboardSummaryDto
{
    public int NewProspects { get; set; }
    public int QualifiedLeads { get; set; }
    public int HighPriorityOpportunities { get; set; }
    public int OutreachSent { get; set; }
    public int Replies { get; set; }
    public int MeetingsScheduled { get; set; }
    public int ProposalsSent { get; set; }
    public int WonProjects { get; set; }
    public decimal EstimatedPipelineValue { get; set; }

    public FunnelDto Funnel { get; set; } = new();
    public List<ProspectListItemDto> TopOpportunities { get; set; } = new();
    public List<ProspectListItemDto> RecentProspects { get; set; } = new();
    public List<FollowUpDto> FollowUpsDue { get; set; } = new();
}

public class FunnelDto
{
    public int Discovered { get; set; }
    public int Qualified { get; set; }
    public int Contacted { get; set; }
    public int Replied { get; set; }
    public int Meeting { get; set; }
    public int Proposal { get; set; }
    public int Won { get; set; }
}

public class FollowUpDto
{
    public Guid LeadId { get; set; }
    public Guid ProspectId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime FollowUpAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
