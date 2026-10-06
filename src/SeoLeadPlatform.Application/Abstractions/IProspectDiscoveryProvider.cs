namespace SeoLeadPlatform.Application.Abstractions;

public record ProspectDiscoveryRequest(
    string? Industry,
    string? Country,
    string? State,
    string? City,
    string? Keywords,
    int MaxResults = 25);

public record ProspectCandidate(
    string CompanyName,
    string WebsiteUrl,
    string? Industry,
    string? Country,
    string? State,
    string? City,
    string? Description,
    string SourceName);

public interface IProspectDiscoveryProvider
{
    string Name { get; }
    Task<IReadOnlyCollection<ProspectCandidate>> DiscoverAsync(
        ProspectDiscoveryRequest request,
        CancellationToken cancellationToken);
}
