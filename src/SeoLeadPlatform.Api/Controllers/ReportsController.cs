using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeoLeadPlatform.Application.Reports;

namespace SeoLeadPlatform.Api.Controllers;

[Route("api/reports")]
public class ReportsController : ApiControllerBase
{
    private readonly PublicReportService _reports;

    public ReportsController(PublicReportService reports) => _reports = reports;

    [Authorize]
    [HttpPost("audits/{auditId:guid}/token")]
    public async Task<IActionResult> CreateToken(Guid auditId, CancellationToken ct)
        => From(await _reports.CreateTokenAsync(auditId, ct), StatusCodes.Status201Created);

    [AllowAnonymous]
    [HttpGet("public/{token}")]
    public async Task<IActionResult> GetPublic(string token, CancellationToken ct)
        => From(await _reports.GetByTokenAsync(token, ct));
}
