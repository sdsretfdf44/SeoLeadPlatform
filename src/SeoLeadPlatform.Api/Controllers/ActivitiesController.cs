using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeoLeadPlatform.Application.Prospects;

namespace SeoLeadPlatform.Api.Controllers;

[Authorize]
[Route("api/prospects")]
public class ActivitiesController : ApiControllerBase
{
    private readonly ActivityService _service;

    public ActivitiesController(ActivityService service) => _service = service;

    [HttpGet("{prospectId:guid}/activities")]
    public async Task<IActionResult> List(Guid prospectId, CancellationToken ct)
        => From(await _service.ListForProspectAsync(prospectId, ct));
}
