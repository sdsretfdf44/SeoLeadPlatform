namespace SeoLeadPlatform.Infrastructure.Auth;

public class JwtOptions
{
    public string Issuer { get; set; } = "SeoLeadPlatform";
    public string Audience { get; set; } = "SeoLeadPlatform.Client";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenLifetimeMinutes { get; set; } = 480;
}
