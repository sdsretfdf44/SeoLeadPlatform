using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeoLeadPlatform.Application.Agency;
using SeoLeadPlatform.Application.Dtos;

namespace SeoLeadPlatform.Api.Controllers;

[Authorize]
[Route("api/agency")]
public class AgencyController : ApiControllerBase
{
    private readonly AgencyService _service;

    public AgencyController(AgencyService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => From(await _service.GetCurrentAsync(ct));

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateAgencyRequest request, CancellationToken ct)
        => From(await _service.UpdateAsync(request, ct));

    [HttpPost("services")]
    public async Task<IActionResult> AddService([FromBody] CreateServiceOfferingRequest request, CancellationToken ct)
        => From(await _service.AddServiceOfferingAsync(request, ct), StatusCodes.Status201Created);
}
