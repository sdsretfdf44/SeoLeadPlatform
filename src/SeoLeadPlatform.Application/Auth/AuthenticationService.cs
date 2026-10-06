using Microsoft.EntityFrameworkCore;
using SeoLeadPlatform.Application.Abstractions;
using SeoLeadPlatform.Application.Common;
using SeoLeadPlatform.Application.Dtos;
using SeoLeadPlatform.Domain.Entities;

namespace SeoLeadPlatform.Application.Auth;

public class AuthenticationService
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _tokens;

    public AuthenticationService(IApplicationDbContext db, IPasswordHasher hasher, IJwtTokenService tokens)
    {
        _db = db;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await _db.Users.AnyAsync(u => u.Email == normalizedEmail, ct);
        if (exists)
        {
            return Result.Failure<AuthResponse>("An account with this email already exists.", "auth.email_taken");
        }

        var agency = new Domain.Entities.Agency { Name = request.AgencyName.Trim() };
        _db.Agencies.Add(agency);

        var user = User.Create(agency.Id, normalizedEmail, _hasher.Hash(request.Password), request.FullName.Trim());
        _db.Users.Add(user);

        await _db.SaveChangesAsync(ct);

        var token = _tokens.GenerateAccessToken(user);
        return Result.Success(new AuthResponse
        {
            AccessToken = token,
            ExpiresAt = _tokens.ExpiresAt,
            User = new UserProfileDto
            {
                Id = user.Id,
                AgencyId = agency.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                AgencyName = agency.Name
            }
        });
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var normalized = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users
            .Include(u => u.Agency)
            .FirstOrDefaultAsync(u => u.Email == normalized, ct);

        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
        {
            return Result.Failure<AuthResponse>("Invalid credentials.", "auth.invalid_credentials");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var token = _tokens.GenerateAccessToken(user);
        return Result.Success(new AuthResponse
        {
            AccessToken = token,
            ExpiresAt = _tokens.ExpiresAt,
            User = new UserProfileDto
            {
                Id = user.Id,
                AgencyId = user.AgencyId,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                AgencyName = user.Agency?.Name ?? string.Empty
            }
        });
    }
}
