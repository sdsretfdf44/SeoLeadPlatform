using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeoLeadPlatform.Application.Proposals;

namespace SeoLeadPlatform.Api.Controllers;

[Route("api/proposals")]
public class ProposalsController : ApiControllerBase
{
    private readonly ProposalService _service;

    public ProposalsController(ProposalService service) => _service = service;

    [Authorize]
    [HttpPost("generate/{prospectId:guid}")]
    public async Task<IActionResult> Generate(Guid prospectId, CancellationToken ct)
        => From(await _service.GenerateForProspectAsync(prospectId, ct), StatusCodes.Status201Created);

    [Authorize]
    [HttpGet("prospect/{prospectId:guid}")]
    public async Task<IActionResult> ListForProspect(Guid prospectId, CancellationToken ct)
        => From(await _service.ListForProspectAsync(prospectId, ct));

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProposalRequest request, CancellationToken ct)
        => From(await _service.UpdateAsync(id, request, ct));

    [Authorize]
    [HttpPost("{id:guid}/send")]
    public async Task<IActionResult> MarkSent(Guid id, CancellationToken ct)
        => From(await _service.MarkSentAsync(id, ct));

    [AllowAnonymous]
    [HttpGet("public/{token}")]
    public async Task<IActionResult> Public(string token, CancellationToken ct)
        => From(await _service.GetPublicAsync(token, ct));
}
