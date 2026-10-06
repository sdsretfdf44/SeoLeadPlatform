using SeoLeadPlatform.Domain.Common;

namespace SeoLeadPlatform.Domain.Entities;

public class ServiceOffering : AgencyOwnedEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? TypicalProjectValue { get; set; }
    public decimal? TypicalMonthlyRetainer { get; set; }
    public bool IsActive { get; set; } = true;
}
