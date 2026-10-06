using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using SeoLeadPlatform.Application.Dtos;

namespace SeoLeadPlatform.IntegrationTests.Api;

public class AuthAndProspectFlowTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AuthAndProspectFlowTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Register_login_and_create_prospect_succeed()
    {
        var client = _factory.CreateClient();

        var email = $"user-{Guid.NewGuid():N}@example.com";
        var register = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            AgencyName = "Test Agency",
            FullName = "Test User",
            Email = email,
            Password = "test-password-123"
        });
        register.EnsureSuccessStatusCode();
        var authResponse = await register.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse!.AccessToken.Should().NotBeNullOrEmpty();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);

        var create = await client.PostAsJsonAsync("/api/prospects", new CreateProspectRequest
        {
            CompanyName = "Acme Dental",
            WebsiteUrl = "https://acme-dental.example",
            Industry = "Dental",
            City = "Austin",
            Country = "US"
        });
        create.EnsureSuccessStatusCode();
        var prospect = await create.Content.ReadFromJsonAsync<ProspectDetailDto>();
        prospect!.CompanyName.Should().Be("Acme Dental");

        var listResponse = await client.GetAsync("/api/prospects");
        listResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Login_with_invalid_credentials_returns_401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "does-not-exist@example.com",
            Password = "invalid"
        });
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized, body);
    }

    [Fact]
    public async Task Anonymous_prospects_request_returns_401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/prospects");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }
}

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public TestWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("USE_INMEMORY_DB", "true");
        Environment.SetEnvironmentVariable("Jwt__SigningKey", "test-only-super-secret-signing-key-32-characters-min-length");
        Environment.SetEnvironmentVariable("Database__AutoMigrate", "false");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
    }
}
