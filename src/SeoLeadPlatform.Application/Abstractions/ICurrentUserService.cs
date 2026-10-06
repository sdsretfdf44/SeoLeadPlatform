namespace SeoLeadPlatform.Application.Abstractions;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? AgencyId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}
