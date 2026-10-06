namespace SeoLeadPlatform.Application.Dtos;

public class AgencyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? WebsiteUrl { get; set; }
    public string? Description { get; set; }
    public string? TargetIndustries { get; set; }
    public string? TargetLocations { get; set; }
    public string? TargetServices { get; set; }
    public decimal? MinimumProjectValue { get; set; }
    public string? PreferredProjectTypes { get; set; }
    public List<ServiceOfferingDto> ServiceOfferings { get; set; } = new();
}

public class UpdateAgencyRequest
{
    public string Name { get; set; } = string.Empty;
    public string? WebsiteUrl { get; set; }
    public string? Description { get; set; }
    public string? TargetIndustries { get; set; }
    public string? TargetLocations { get; set; }
    public string? TargetServices { get; set; }
    public decimal? MinimumProjectValue { get; set; }
    public string? PreferredProjectTypes { get; set; }
}

public class ServiceOfferingDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? TypicalProjectValue { get; set; }
    public decimal? TypicalMonthlyRetainer { get; set; }
    public bool IsActive { get; set; }
}

public class CreateServiceOfferingRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? TypicalProjectValue { get; set; }
    public decimal? TypicalMonthlyRetainer { get; set; }
}
