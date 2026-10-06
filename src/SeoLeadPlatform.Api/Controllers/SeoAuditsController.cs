using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeoLeadPlatform.Application.SeoAudits;

namespace SeoLeadPlatform.Api.Controllers;

[Authorize]
[Route("api/seo-audits")]
public class SeoAuditsController : ApiControllerBase
{
    private readonly SeoAuditService _service;

    public SeoAuditsController(SeoAuditService service) => _service = service;

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct) => From(await _service.GetAuditAsync(id, ct));
}
