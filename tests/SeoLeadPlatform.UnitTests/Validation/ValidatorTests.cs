using FluentAssertions;
using SeoLeadPlatform.Application.Dtos;
using SeoLeadPlatform.Application.Validation;

namespace SeoLeadPlatform.UnitTests.Validation;

public class ValidatorTests
{
    [Fact]
    public void Register_requires_valid_email_and_password()
    {
        var validator = new RegisterRequestValidator();

        var result = validator.Validate(new RegisterRequest
        {
            AgencyName = "Acme",
            FullName = "Jane",
            Email = "not-an-email",
            Password = "short"
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Email));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public void CreateProspect_requires_valid_url()
    {
        var validator = new CreateProspectRequestValidator();

        var invalid = validator.Validate(new CreateProspectRequest
        {
            CompanyName = "Acme",
            WebsiteUrl = "not a url"
        });

        var valid = validator.Validate(new CreateProspectRequest
        {
            CompanyName = "Acme",
            WebsiteUrl = "https://acme.com"
        });

        invalid.IsValid.Should().BeFalse();
        valid.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateProspect_accepts_bare_domain()
    {
        var validator = new CreateProspectRequestValidator();
        var result = validator.Validate(new CreateProspectRequest
        {
            CompanyName = "Acme",
            WebsiteUrl = "acme.com"
        });
        result.IsValid.Should().BeTrue();
    }
}
