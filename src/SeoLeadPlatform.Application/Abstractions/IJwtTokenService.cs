using SeoLeadPlatform.Domain.Entities;

namespace SeoLeadPlatform.Application.Abstractions;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    DateTime ExpiresAt { get; }
}
