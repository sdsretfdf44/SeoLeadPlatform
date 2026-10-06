using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeoLeadPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TargetIndustries = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TargetLocations = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TargetServices = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MinimumProjectValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PreferredProjectTypes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "outreach_campaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outreach_campaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prospect_companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Industry = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    State = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    City = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    CompanySize = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    PrimaryEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LinkedInUrl = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Source = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prospect_companies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prospect_companies_agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_offerings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TypicalProjectValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TypicalMonthlyRetainer = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_offerings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_service_offerings_agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProspectCompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    LinkedInUrl = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    IsDecisionMaker = table.Column<bool>(type: "bit", nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_contacts_prospect_companies_ProspectCompanyId",
                        column: x => x.ProspectCompanyId,
                        principalTable: "prospect_companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "leads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProspectCompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    OpportunityScore = table.Column<int>(type: "int", nullable: false),
                    EstimatedProjectValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    EstimatedMonthlyRetainer = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Probability = table.Column<int>(type: "int", nullable: false),
                    NextFollowUpAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_leads_prospect_companies_ProspectCompanyId",
                        column: x => x.ProspectCompanyId,
                        principalTable: "prospect_companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "seo_audits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProspectCompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OverallScore = table.Column<int>(type: "int", nullable: false),
                    TechnicalScore = table.Column<int>(type: "int", nullable: false),
                    OnPageScore = table.Column<int>(type: "int", nullable: false),
                    ContentScore = table.Column<int>(type: "int", nullable: false),
                    PerformanceScore = table.Column<int>(type: "int", nullable: false),
                    LocalSeoScore = table.Column<int>(type: "int", nullable: false),
                    HasHttps = table.Column<bool>(type: "bit", nullable: false),
                    HasRobotsTxt = table.Column<bool>(type: "bit", nullable: false),
                    HasSitemap = table.Column<bool>(type: "bit", nullable: false),
                    HasCanonicalTag = table.Column<bool>(type: "bit", nullable: false),
                    HasStructuredData = table.Column<bool>(type: "bit", nullable: false),
                    HasMetaDescription = table.Column<bool>(type: "bit", nullable: false),
                    HasTitle = table.Column<bool>(type: "bit", nullable: false),
                    HasH1 = table.Column<bool>(type: "bit", nullable: false),
                    IsMobileFriendlyViewport = table.Column<bool>(type: "bit", nullable: false),
                    WordCount = table.Column<int>(type: "int", nullable: false),
                    InternalLinkCount = table.Column<int>(type: "int", nullable: false),
                    ImageCount = table.Column<int>(type: "int", nullable: false),
                    ImagesMissingAlt = table.Column<int>(type: "int", nullable: false),
                    HttpStatus = table.Column<int>(type: "int", nullable: true),
                    ResponseTimeMs = table.Column<long>(type: "bigint", nullable: true),
                    PageSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    AnalysisSummary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seo_audits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_seo_audits_prospect_companies_ProspectCompanyId",
                        column: x => x.ProspectCompanyId,
                        principalTable: "prospect_companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "outreach_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OutreachCampaignId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProspectCompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PersonalizationData = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    GeneratedByAi = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OpenedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RepliedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outreach_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_outreach_messages_contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "contacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_outreach_messages_outreach_campaigns_OutreachCampaignId",
                        column: x => x.OutreachCampaignId,
                        principalTable: "outreach_campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_outreach_messages_prospect_companies_ProspectCompanyId",
                        column: x => x.ProspectCompanyId,
                        principalTable: "prospect_companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "activity_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeadId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProspectCompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_activity_logs_leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "leads",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "proposals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProspectCompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeadId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ExecutiveSummary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ProblemsIdentified = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    RecommendedServices = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Deliverables = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Timeline = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Pricing = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MonthlyRetainer = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Terms = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublicToken = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proposals_leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_proposals_prospect_companies_ProspectCompanyId",
                        column: x => x.ProspectCompanyId,
                        principalTable: "prospect_companies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "opportunity_scores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeoAuditId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProspectCompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    SeoNeedScore = table.Column<int>(type: "int", nullable: false),
                    BusinessPotentialScore = table.Column<int>(type: "int", nullable: false),
                    ServiceFitScore = table.Column<int>(type: "int", nullable: false),
                    CommercialPotentialScore = table.Column<int>(type: "int", nullable: false),
                    WebsiteOpportunityScore = table.Column<int>(type: "int", nullable: false),
                    CompetitiveGapScore = table.Column<int>(type: "int", nullable: false),
                    ContactabilityScore = table.Column<int>(type: "int", nullable: false),
                    BuyingSignalScore = table.Column<int>(type: "int", nullable: false),
                    Reasoning = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    FactorsJson = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opportunity_scores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_opportunity_scores_seo_audits_SeoAuditId",
                        column: x => x.SeoAuditId,
                        principalTable: "seo_audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "public_audit_report_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeoAuditId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_public_audit_report_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_public_audit_report_tokens_seo_audits_SeoAuditId",
                        column: x => x.SeoAuditId,
                        principalTable: "seo_audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "seo_issues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeoAuditId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Recommendation = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Evidence = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    EstimatedImpact = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seo_issues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_seo_issues_seo_audits_SeoAuditId",
                        column: x => x.SeoAuditId,
                        principalTable: "seo_audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_activity_logs_AgencyId_CreatedAt",
                table: "activity_logs",
                columns: new[] { "AgencyId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_activity_logs_LeadId",
                table: "activity_logs",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_activity_logs_ProspectCompanyId",
                table: "activity_logs",
                column: "ProspectCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_contacts_ProspectCompanyId",
                table: "contacts",
                column: "ProspectCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_leads_AgencyId",
                table: "leads",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_leads_NextFollowUpAt",
                table: "leads",
                column: "NextFollowUpAt");

            migrationBuilder.CreateIndex(
                name: "IX_leads_OpportunityScore",
                table: "leads",
                column: "OpportunityScore");

            migrationBuilder.CreateIndex(
                name: "IX_leads_ProspectCompanyId",
                table: "leads",
                column: "ProspectCompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_leads_Status",
                table: "leads",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_opportunity_scores_SeoAuditId",
                table: "opportunity_scores",
                column: "SeoAuditId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_outreach_messages_ContactId",
                table: "outreach_messages",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_outreach_messages_OutreachCampaignId",
                table: "outreach_messages",
                column: "OutreachCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_outreach_messages_ProspectCompanyId",
                table: "outreach_messages",
                column: "ProspectCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_outreach_messages_Status",
                table: "outreach_messages",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_proposals_LeadId",
                table: "proposals",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_proposals_ProspectCompanyId",
                table: "proposals",
                column: "ProspectCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_proposals_PublicToken",
                table: "proposals",
                column: "PublicToken",
                unique: true,
                filter: "[PublicToken] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_prospect_companies_AgencyId_WebsiteUrl",
                table: "prospect_companies",
                columns: new[] { "AgencyId", "WebsiteUrl" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prospect_companies_Country",
                table: "prospect_companies",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_prospect_companies_Industry",
                table: "prospect_companies",
                column: "Industry");

            migrationBuilder.CreateIndex(
                name: "IX_public_audit_report_tokens_SeoAuditId",
                table: "public_audit_report_tokens",
                column: "SeoAuditId");

            migrationBuilder.CreateIndex(
                name: "IX_public_audit_report_tokens_Token",
                table: "public_audit_report_tokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_seo_audits_CreatedAt",
                table: "seo_audits",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_seo_audits_ProspectCompanyId",
                table: "seo_audits",
                column: "ProspectCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_seo_audits_Status",
                table: "seo_audits",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_seo_issues_SeoAuditId",
                table: "seo_issues",
                column: "SeoAuditId");

            migrationBuilder.CreateIndex(
                name: "IX_service_offerings_AgencyId",
                table: "service_offerings",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_users_AgencyId",
                table: "users",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_logs");

            migrationBuilder.DropTable(
                name: "opportunity_scores");

            migrationBuilder.DropTable(
                name: "outreach_messages");

            migrationBuilder.DropTable(
                name: "proposals");

            migrationBuilder.DropTable(
                name: "public_audit_report_tokens");

            migrationBuilder.DropTable(
                name: "seo_issues");

            migrationBuilder.DropTable(
                name: "service_offerings");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "contacts");

            migrationBuilder.DropTable(
                name: "outreach_campaigns");

            migrationBuilder.DropTable(
                name: "leads");

            migrationBuilder.DropTable(
                name: "seo_audits");

            migrationBuilder.DropTable(
                name: "prospect_companies");

            migrationBuilder.DropTable(
                name: "agencies");
        }
    }
}
