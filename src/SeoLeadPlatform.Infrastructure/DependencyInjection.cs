using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SeoLeadPlatform.Application.Abstractions;
using SeoLeadPlatform.Infrastructure.Auth;
using SeoLeadPlatform.Infrastructure.BackgroundJobs;
using SeoLeadPlatform.Infrastructure.Http;
using SeoLeadPlatform.Infrastructure.Persistence;
using SeoLeadPlatform.Infrastructure.Seo;
using SeoLeadPlatform.Infrastructure.Seo.Analyzers;
using SeoLeadPlatform.Infrastructure.Services;

namespace SeoLeadPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddInfrastructureCore(services, configuration);

        var connectionString = configuration.GetConnectionString("SqlServer")
            ?? configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:SqlServer is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    public static IServiceCollection AddInfrastructureCore(this IServiceCollection services, IConfiguration configuration)
    {

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddHttpClient("seo-fetch", client =>
        {
            client.MaxResponseContentBufferSize = 4 * 1024 * 1024;
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 5,
            AutomaticDecompression = System.Net.DecompressionMethods.All
        });
        services.AddSingleton<SafeHttpClient>();

        services.AddScoped<ISeoAnalyzer, TechnicalSeoAnalyzer>();
        services.AddScoped<ISeoAnalyzer, OnPageSeoAnalyzer>();
        services.AddScoped<ISeoAnalyzer, ContentAnalyzer>();
        services.AddScoped<ISeoAnalyzer, PerformanceAnalyzer>();
        services.AddScoped<ISeoAnalyzer, LocalSeoAnalyzer>();
        services.AddScoped<ISeoAuditProvider, WebsiteSeoAuditProvider>();

        services.AddSingleton<IOpportunityScoringService, OpportunityScoringService>();
        services.AddScoped<ILeadIntelligenceService, HeuristicLeadIntelligenceService>();

        services.AddSingleton<ISeoAuditQueue, SeoAuditQueue>();
        services.AddHostedService<SeoAuditWorker>();

        return services;
    }
}

