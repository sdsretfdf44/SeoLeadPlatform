using FluentValidation;
using SeoLeadPlatform.Application.Dtos;

namespace SeoLeadPlatform.Application.Validation;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.AgencyName).NotEmpty().MaximumLength(160);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(200);
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class CreateProspectRequestValidator : AbstractValidator<CreateProspectRequest>
{
    public CreateProspectRequestValidator()
    {
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.WebsiteUrl).NotEmpty().Must(BeValidHttpUrl)
            .WithMessage("Website URL must be a valid http(s) URL.");
        RuleFor(x => x.Industry).MaximumLength(120);
        RuleFor(x => x.Country).MaximumLength(80);
        RuleFor(x => x.State).MaximumLength(80);
        RuleFor(x => x.City).MaximumLength(80);
        RuleFor(x => x.PrimaryEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.PrimaryEmail));
    }

    private static bool BeValidHttpUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        var candidate = url.Trim();
        if (!candidate.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !candidate.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            candidate = "https://" + candidate;
        }
        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri)) return false;
        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
    }
}

public class UpdateAgencyRequestValidator : AbstractValidator<UpdateAgencyRequest>
{
    public UpdateAgencyRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(160);
        RuleFor(x => x.WebsiteUrl).MaximumLength(400);
        RuleFor(x => x.MinimumProjectValue).GreaterThanOrEqualTo(0).When(x => x.MinimumProjectValue.HasValue);
    }
}

public class CreateServiceOfferingRequestValidator : AbstractValidator<CreateServiceOfferingRequest>
{
    public CreateServiceOfferingRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(160);
        RuleFor(x => x.TypicalProjectValue).GreaterThanOrEqualTo(0).When(x => x.TypicalProjectValue.HasValue);
        RuleFor(x => x.TypicalMonthlyRetainer).GreaterThanOrEqualTo(0).When(x => x.TypicalMonthlyRetainer.HasValue);
    }
}
