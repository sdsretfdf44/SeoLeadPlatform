using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeoLeadPlatform.Application.Prospects;

namespace SeoLeadPlatform.Api.Controllers;

[Authorize]
[Route("api/prospects/import")]
public class ImportsController : ApiControllerBase
{
    private readonly ProspectImportService _service;

    public ImportsController(ProspectImportService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Import([FromBody] ImportProspectsRequest request, CancellationToken ct)
        => From(await _service.ImportAsync(request, ct));
}
