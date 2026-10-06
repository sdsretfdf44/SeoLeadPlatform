using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeoLeadPlatform.Application.Prospects;

namespace SeoLeadPlatform.Api.Controllers;

[Authorize]
[Route("api")]
public class ContactsController : ApiControllerBase
{
    private readonly ContactService _service;

    public ContactsController(ContactService service) => _service = service;

    [HttpPost("prospects/{prospectId:guid}/contacts")]
    public async Task<IActionResult> Add(Guid prospectId, [FromBody] UpsertContactRequest request, CancellationToken ct)
        => From(await _service.AddAsync(prospectId, request, ct), StatusCodes.Status201Created);

    [HttpPut("contacts/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertContactRequest request, CancellationToken ct)
        => From(await _service.UpdateAsync(id, request, ct));

    [HttpDelete("contacts/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => From(await _service.DeleteAsync(id, ct));
}
