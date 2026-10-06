using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeoLeadPlatform.Application.Leads;

namespace SeoLeadPlatform.Api.Controllers;

[Authorize]
[Route("api/leads")]
public class LeadsController : ApiControllerBase
{
    private readonly LeadService _leads;

    public LeadsController(LeadService leads) => _leads = leads;

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateLeadStatusRequest request, CancellationToken ct)
        => From(await _leads.UpdateStatusAsync(id, request, ct));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeadRequest request, CancellationToken ct)
        => From(await _leads.UpdateAsync(id, request, ct));
}
