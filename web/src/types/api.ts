export type LeadStatus =
  | 'New' | 'Qualified' | 'Contacted' | 'Replied' | 'MeetingScheduled'
  | 'ProposalSent' | 'Negotiation' | 'Won' | 'Lost' | 'Disqualified';

export type LeadPriority = 'Low' | 'Medium' | 'High' | 'Critical';

export type SeoAuditStatus = 'Queued' | 'Running' | 'Completed' | 'Failed';

export type SeoIssueSeverity = 'Info' | 'Low' | 'Medium' | 'High' | 'Critical';

export type SeoIssueCategory =
  | 'TechnicalSeo' | 'OnPageSeo' | 'ContentSeo' | 'LocalSeo' | 'Performance'
  | 'Accessibility' | 'Indexability' | 'StructuredData' | 'Backlinks'
  | 'KeywordOpportunities' | 'CompetitorGap';

export type ProspectSource =
  | 'Manual' | 'PublicDirectory' | 'Referral' | 'WebsiteDiscovery'
  | 'Inbound' | 'Import' | 'Api';

export type OutreachMessageType = 'InitialEmail' | 'FollowUpEmail' | 'LinkedInMessage' | 'ContactFormMessage';
export type OutreachMessageStatus = 'Draft' | 'Approved' | 'Sent' | 'Opened' | 'Replied' | 'Bounced';

export interface UserProfile {
  id: string;
  agencyId: string;
  email: string;
  fullName: string;
  role: string;
  agencyName: string;
}

export interface AuthResponse {
  accessToken: string;
  expiresAt: string;
  user: UserProfile;
}

export interface Agency {
  id: string;
  name: string;
  websiteUrl?: string;
  description?: string;
  targetIndustries?: string;
  targetLocations?: string;
  targetServices?: string;
  minimumProjectValue?: number;
  preferredProjectTypes?: string;
  serviceOfferings: ServiceOffering[];
}

export interface ServiceOffering {
  id: string;
  name: string;
  description?: string;
  typicalProjectValue?: number;
  typicalMonthlyRetainer?: number;
  isActive: boolean;
}

export interface ProspectListItem {
  id: string;
  companyName: string;
  websiteUrl: string;
  industry?: string;
  country?: string;
  city?: string;
  opportunityScore?: number | null;
  leadStatus?: LeadStatus | null;
  leadPriority?: LeadPriority | null;
  latestAuditStatus?: SeoAuditStatus | null;
  overallSeoScore?: number | null;
  createdAt: string;
}

export interface Contact {
  id: string;
  fullName: string;
  jobTitle?: string;
  email?: string;
  phoneNumber?: string;
  linkedInUrl?: string;
  isDecisionMaker: boolean;
}

export interface Lead {
  id: string;
  status: LeadStatus;
  priority: LeadPriority;
  opportunityScore: number;
  estimatedProjectValue?: number | null;
  estimatedMonthlyRetainer?: number | null;
  probability: number;
  nextFollowUpAt?: string | null;
  notes?: string | null;
}

export interface SeoAuditSummary {
  id: string;
  status: SeoAuditStatus;
  overallScore: number;
  technicalScore: number;
  onPageScore: number;
  contentScore: number;
  performanceScore: number;
  localSeoScore: number;
  issueCount: number;
  criticalIssueCount: number;
  highIssueCount: number;
  completedAt?: string | null;
}

export interface OpportunityFactor {
  name: string;
  score: number;
  weight: number;
  reason: string;
}

export interface OpportunityScoreDto {
  score: number;
  priority: LeadPriority;
  reasoning: string;
  factors: OpportunityFactor[];
}

export interface SeoIssue {
  id: string;
  category: SeoIssueCategory;
  severity: SeoIssueSeverity;
  title: string;
  description: string;
  recommendation: string;
  evidence?: string;
  estimatedImpact?: string;
}

export interface SeoAuditDetail {
  id: string;
  prospectCompanyId: string;
  websiteUrl: string;
  status: SeoAuditStatus;
  overallScore: number;
  technicalScore: number;
  onPageScore: number;
  contentScore: number;
  performanceScore: number;
  localSeoScore: number;
  startedAt?: string | null;
  completedAt?: string | null;
  failureReason?: string | null;
  hasHttps: boolean;
  hasRobotsTxt: boolean;
  hasSitemap: boolean;
  hasCanonicalTag: boolean;
  hasStructuredData: boolean;
  hasMetaDescription: boolean;
  hasTitle: boolean;
  hasH1: boolean;
  isMobileFriendlyViewport: boolean;
  wordCount: number;
  internalLinkCount: number;
  imageCount: number;
  imagesMissingAlt: number;
  httpStatus?: number | null;
  responseTimeMs?: number | null;
  pageSizeBytes?: number | null;
  analysisSummary?: string | null;
  issues: SeoIssue[];
  opportunityScore?: OpportunityScoreDto | null;
}

export interface ProspectDetail {
  id: string;
  companyName: string;
  websiteUrl: string;
  industry?: string;
  country?: string;
  state?: string;
  city?: string;
  companySize?: string;
  description?: string;
  phoneNumber?: string;
  primaryEmail?: string;
  linkedInUrl?: string;
  source: ProspectSource;
  createdAt: string;
  contacts: Contact[];
  lead?: Lead | null;
  latestAudit?: SeoAuditSummary | null;
}

export interface Paged<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface FollowUp {
  leadId: string;
  prospectId: string;
  companyName: string;
  followUpAt: string;
  status: string;
}

export interface DashboardSummary {
  newProspects: number;
  qualifiedLeads: number;
  highPriorityOpportunities: number;
  outreachSent: number;
  replies: number;
  meetingsScheduled: number;
  proposalsSent: number;
  wonProjects: number;
  estimatedPipelineValue: number;
  funnel: {
    discovered: number;
    qualified: number;
    contacted: number;
    replied: number;
    meeting: number;
    proposal: number;
    won: number;
  };
  topOpportunities: ProspectListItem[];
  recentProspects: ProspectListItem[];
  followUpsDue: FollowUp[];
}

export interface OutreachMessage {
  id: string;
  prospectCompanyId: string;
  contactId?: string | null;
  type: OutreachMessageType;
  status: OutreachMessageStatus;
  subject: string;
  body: string;
  generatedByAi: boolean;
  approvedAt?: string | null;
  sentAt?: string | null;
  createdAt: string;
}

export interface PublicReport {
  companyName: string;
  websiteUrl: string;
  agencyName?: string;
  audit: SeoAuditDetail;
}

export type ActivityType =
  | 'ProspectDiscovered' | 'AuditCompleted' | 'LeadQualified' | 'OutreachGenerated'
  | 'OutreachApproved' | 'OutreachSent' | 'ReplyReceived' | 'MeetingScheduled'
  | 'ProposalCreated' | 'ProposalSent' | 'LeadWon' | 'LeadLost' | 'NoteAdded' | 'StatusChanged';

export interface Activity {
  id: string;
  type: ActivityType;
  description: string;
  createdAt: string;
}

export type ProposalStatus = 'Draft' | 'Sent' | 'Viewed' | 'Accepted' | 'Rejected';

export interface Proposal {
  id: string;
  prospectCompanyId: string;
  companyName: string;
  title: string;
  executiveSummary: string;
  problemsIdentified: string;
  recommendedServices: string;
  deliverables: string;
  timeline: string;
  pricing: number;
  monthlyRetainer?: number | null;
  terms: string;
  status: ProposalStatus;
  publicToken?: string | null;
  sentAt?: string | null;
  createdAt: string;
}

export interface UpdateProposalRequest {
  title: string;
  executiveSummary: string;
  problemsIdentified: string;
  recommendedServices: string;
  deliverables: string;
  timeline: string;
  pricing: number;
  monthlyRetainer?: number | null;
  terms: string;
}

export interface UpsertContactRequest {
  fullName: string;
  jobTitle?: string;
  email?: string;
  phoneNumber?: string;
  linkedInUrl?: string;
  isDecisionMaker: boolean;
}

export interface ImportProspectRow {
  companyName: string;
  websiteUrl: string;
  industry?: string;
  country?: string;
  state?: string;
  city?: string;
  primaryEmail?: string;
  phoneNumber?: string;
}

export interface ImportProspectsResult {
  imported: number;
  skipped: number;
  errors: string[];
}
