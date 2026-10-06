using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SeoLeadPlatform.Application.Agency;
using SeoLeadPlatform.Application.Auth;
using SeoLeadPlatform.Application.Dashboard;
using SeoLeadPlatform.Application.Leads;
using SeoLeadPlatform.Application.Outreach;
using SeoLeadPlatform.Application.Proposals;
using SeoLeadPlatform.Application.Prospects;
using SeoLeadPlatform.Application.Reports;
using SeoLeadPlatform.Application.SeoAudits;

namespace SeoLeadPlatform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<AuthenticationService>();
        services.AddScoped<AgencyService>();
        services.AddScoped<ProspectService>();
        services.AddScoped<SeoAuditService>();
        services.AddScoped<LeadService>();
        services.AddScoped<DashboardService>();
        services.AddScoped<OutreachService>();
        services.AddScoped<PublicReportService>();
        services.AddScoped<ContactService>();
        services.AddScoped<ActivityService>();
        services.AddScoped<ProspectImportService>();
        services.AddScoped<ProposalService>();

        return services;
    }
}
