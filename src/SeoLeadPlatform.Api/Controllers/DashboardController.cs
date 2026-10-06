using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeoLeadPlatform.Application.Dashboard;

namespace SeoLeadPlatform.Api.Controllers;

[Authorize]
[Route("api/dashboard")]
public class DashboardController : ApiControllerBase
{
    private readonly DashboardService _service;

    public DashboardController(DashboardService service) => _service = service;

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken ct) => From(await _service.GetSummaryAsync(ct));
}
