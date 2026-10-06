using FluentAssertions;
using SeoLeadPlatform.Infrastructure.Http;

namespace SeoLeadPlatform.UnitTests.Seo;

public class SafeHttpClientTests
{
    [Theory]
    [InlineData("http://127.0.0.1/path")]
    [InlineData("http://10.0.0.5/")]
    [InlineData("http://192.168.1.1/")]
    [InlineData("http://169.254.169.254/latest/")]
    public void Private_ip_urls_are_rejected(string url)
    {
        var ok = SafeHttpClient.IsPublicHttpUrl(url, out _, out var reason);
        ok.Should().BeFalse();
        reason.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("ftp://example.com")]
    [InlineData("javascript:alert(1)")]
    [InlineData("file:///etc/passwd")]
    public void Non_http_schemes_are_rejected(string url)
    {
        SafeHttpClient.IsPublicHttpUrl(url, out _, out _).Should().BeFalse();
    }

    [Fact]
    public void Public_https_url_is_accepted()
    {
        SafeHttpClient.IsPublicHttpUrl("https://example.com", out var uri, out _).Should().BeTrue();
        uri.Host.Should().Be("example.com");
    }
}
