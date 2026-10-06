using SeoLeadPlatform.Domain.Common;

namespace SeoLeadPlatform.Domain.Entities;

public class User : AgencyOwnedEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Owner";
    public DateTime? LastLoginAt { get; set; }

    public Agency? Agency { get; set; }

    public static User Create(Guid agencyId, string email, string passwordHash, string fullName, string role = "Owner")
    {
        return new User
        {
            AgencyId = agencyId,
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            FullName = fullName,
            Role = role
        };
    }
}
