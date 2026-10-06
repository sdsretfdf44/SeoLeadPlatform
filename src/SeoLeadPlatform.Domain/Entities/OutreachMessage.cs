using SeoLeadPlatform.Domain.Common;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Domain.Entities;

public class OutreachMessage : Entity
{
    public Guid? OutreachCampaignId { get; set; }
    public Guid ProspectCompanyId { get; set; }
    public Guid? ContactId { get; set; }

    public OutreachMessageType Type { get; set; } = OutreachMessageType.InitialEmail;
    public OutreachMessageStatus Status { get; set; } = OutreachMessageStatus.Draft;

    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? PersonalizationData { get; set; }
    public bool GeneratedByAi { get; set; }

    public DateTime? ApprovedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? OpenedAt { get; set; }
    public DateTime? RepliedAt { get; set; }

    public OutreachCampaign? OutreachCampaign { get; set; }
    public ProspectCompany? ProspectCompany { get; set; }
    public Contact? Contact { get; set; }
}
