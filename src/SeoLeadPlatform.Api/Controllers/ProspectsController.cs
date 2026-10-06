using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeoLeadPlatform.Application.Dtos;
using SeoLeadPlatform.Application.Leads;
using SeoLeadPlatform.Application.Outreach;
using SeoLeadPlatform.Application.Prospects;
using SeoLeadPlatform.Application.SeoAudits;

namespace SeoLeadPlatform.Api.Controllers;

[Authorize]
[Route("api/prospects")]
public class ProspectsController : ApiControllerBase
{
    private readonly ProspectService _prospects;
    private readonly SeoAuditService _audits;
    private readonly LeadService _leads;
    private readonly OutreachService _outreach;

    public ProspectsController(
        ProspectService prospects,
        SeoAuditService audits,
        LeadService leads,
        OutreachService outreach)
    {
        _prospects = prospects;
        _audits = audits;
        _leads = leads;
        _outreach = outreach;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] ProspectSearchQuery query, CancellationToken ct)
        => From(await _prospects.SearchAsync(query, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        => From(await _prospects.GetAsync(id, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProspectRequest request, CancellationToken ct)
        => From(await _prospects.CreateAsync(request, ct), StatusCodes.Status201Created);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => From(await _prospects.DeleteAsync(id, ct));

    [HttpPost("{id:guid}/audit")]
    public async Task<IActionResult> StartAudit(Guid id, CancellationToken ct)
        => From(await _audits.StartAuditAsync(id, ct), StatusCodes.Status202Accepted);

    [HttpGet("{id:guid}/audit")]
    public async Task<IActionResult> GetLatestAudit(Guid id, CancellationToken ct)
        => From(await _audits.GetLatestForProspectAsync(id, ct));

    [HttpPost("{id:guid}/qualify")]
    public async Task<IActionResult> Qualify(Guid id, CancellationToken ct)
        => From(await _leads.QualifyProspectAsync(id, ct));

    [HttpPost("{id:guid}/outreach")]
    public async Task<IActionResult> GenerateOutreach(Guid id, [FromBody] GenerateOutreachOptions options, CancellationToken ct)
        => From(await _outreach.GenerateAsync(new GenerateOutreachRequest
        {
            ProspectId = id,
            ContactId = options.ContactId,
            Type = options.Type
        }, ct), StatusCodes.Status201Created);

    [HttpGet("{id:guid}/outreach")]
    public async Task<IActionResult> ListOutreach(Guid id, CancellationToken ct)
        => From(await _outreach.ListForProspectAsync(id, ct));
}

public class GenerateOutreachOptions
{
    public Guid? ContactId { get; set; }
    public Domain.Enums.OutreachMessageType Type { get; set; } = Domain.Enums.OutreachMessageType.InitialEmail;
}
