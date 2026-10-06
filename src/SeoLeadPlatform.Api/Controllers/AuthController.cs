using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeoLeadPlatform.Application.Abstractions;
using SeoLeadPlatform.Application.Auth;
using SeoLeadPlatform.Application.Common;
using SeoLeadPlatform.Application.Dtos;

namespace SeoLeadPlatform.Api.Controllers;

[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly AuthenticationService _auth;
    private readonly ICurrentUserService _current;
    private readonly IApplicationDbContext _db;

    public AuthController(AuthenticationService auth, ICurrentUserService current, IApplicationDbContext db)
    {
        _auth = auth;
        _current = current;
        _db = db;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
        => From(await _auth.RegisterAsync(request, ct), StatusCodes.Status201Created);

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
        => From(await _auth.LoginAsync(request, ct));

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        if (_current.UserId is null) return Unauthorized();
        var user = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .FirstOrDefaultAsync(_db.Users, u => u.Id == _current.UserId, ct);
        if (user is null) return Unauthorized();
        var agency = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .FirstOrDefaultAsync(_db.Agencies, a => a.Id == user.AgencyId, ct);
        return Ok(new UserProfileDto
        {
            Id = user.Id,
            AgencyId = user.AgencyId,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            AgencyName = agency?.Name ?? string.Empty
        });
    }
}
