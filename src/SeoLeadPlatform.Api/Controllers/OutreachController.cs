using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeoLeadPlatform.Application.Outreach;

namespace SeoLeadPlatform.Api.Controllers;

[Authorize]
[Route("api/outreach")]
public class OutreachController : ApiControllerBase
{
    private readonly OutreachService _service;

    public OutreachController(OutreachService service) => _service = service;

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOutreachRequest request, CancellationToken ct)
        => From(await _service.UpdateAsync(id, request, ct));

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
        => From(await _service.ApproveAsync(id, ct));

    [HttpPost("{id:guid}/sent")]
    public async Task<IActionResult> MarkSent(Guid id, CancellationToken ct)
        => From(await _service.MarkSentAsync(id, ct));
}
