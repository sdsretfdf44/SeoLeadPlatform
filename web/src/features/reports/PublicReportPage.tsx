import { useQuery } from '@tanstack/react-query';
import { useParams } from 'react-router-dom';
import { reportsApi } from '../../services/reportsApi';
import { PageSpinner } from '../../components/ui/Loader';
import { ScoreBadge, SeverityBadge } from '../../components/ui/Score';

export function PublicReportPage() {
  const { token } = useParams<{ token: string }>();
  const { data, isLoading, isError } = useQuery({
    queryKey: ['public-report', token],
    queryFn: () => reportsApi.getPublic(token!),
    enabled: !!token
  });

  if (isLoading) return <PageSpinner />;
  if (isError || !data) {
    return (
      <div className="min-h-screen flex items-center justify-center p-8">
        <div className="text-center">
          <h1 className="text-2xl font-semibold text-slate-900">Report not available</h1>
          <p className="text-sm text-slate-500 mt-2">This link may have expired or been revoked.</p>
        </div>
      </div>
    );
  }

  const audit = data.audit;

  return (
    <div className="min-h-screen bg-slate-50">
      <div className="max-w-3xl mx-auto px-6 py-12 space-y-6">
        <header className="text-center">
          <div className="text-xs uppercase tracking-widest text-brand-700">SEO opportunity report</div>
          <h1 className="text-3xl font-semibold text-slate-900 mt-2">{data.companyName}</h1>
          <div className="text-sm text-slate-500 mt-1">{data.websiteUrl}</div>
          {data.agencyName && <div className="text-xs text-slate-500 mt-3">Prepared by {data.agencyName}</div>}
        </header>

        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <ReportMetric label="Overall SEO" score={audit.overallScore} />
          <ReportMetric label="Opportunity" score={audit.opportunityScore?.score ?? null} />
          <ReportMetric label="Technical" score={audit.technicalScore} />
          <ReportMetric label="On-page" score={audit.onPageScore} />
          <ReportMetric label="Content" score={audit.contentScore} />
          <ReportMetric label="Performance" score={audit.performanceScore} />
          <ReportMetric label="Local" score={audit.localSeoScore} />
        </div>

        {audit.analysisSummary && (
          <div className="card">
            <div className="card-body">
              <h2 className="text-sm font-semibold text-slate-900">Summary</h2>
              <p className="text-sm text-slate-700 mt-2">{audit.analysisSummary}</p>
            </div>
          </div>
        )}

        {audit.opportunityScore && (
          <div className="card">
            <div className="card-body">
              <h2 className="text-sm font-semibold text-slate-900">Why we're reaching out</h2>
              <p className="text-sm text-slate-700 mt-2">{audit.opportunityScore.reasoning}</p>
            </div>
          </div>
        )}

        <div className="card">
          <div className="card-body">
            <h2 className="text-sm font-semibold text-slate-900">Top opportunities</h2>
            <ul className="mt-3 divide-y divide-slate-100">
              {audit.issues.slice(0, 8).map((issue) => (
                <li key={issue.id} className="py-3">
                  <div className="flex items-center justify-between">
                    <div className="font-medium text-slate-900">{issue.title}</div>
                    <SeverityBadge severity={issue.severity} />
                  </div>
                  <p className="text-sm text-slate-600 mt-1">{issue.description}</p>
                  <p className="text-sm text-slate-500 mt-1"><span className="font-medium text-slate-700">What to do:</span> {issue.recommendation}</p>
                </li>
              ))}
            </ul>
          </div>
        </div>

        <div className="card bg-brand-600 text-white">
          <div className="card-body text-center py-8">
            <div className="text-lg font-semibold">Want a walkthrough?</div>
            <p className="text-sm text-brand-100 mt-2">We'll spend 15 minutes reviewing the opportunities above and share a plan tailored to your business.</p>
            <a href="mailto:hello@example.com" className="inline-block mt-4 rounded-lg bg-white text-brand-700 px-4 py-2 text-sm font-semibold">
              Book a free strategy call
            </a>
          </div>
        </div>

        <div className="text-center text-xs text-slate-400">
          Analysis based on publicly available data as of {audit.completedAt ? new Date(audit.completedAt).toLocaleDateString() : '—'}.
        </div>
      </div>
    </div>
  );
}

function ReportMetric({ label, score }: { label: string; score: number | null | undefined }) {
  return (
    <div className="card card-body text-center">
      <div className="metric-label">{label}</div>
      <div className="mt-2 flex justify-center">
        <ScoreBadge score={score ?? null} size="lg" />
      </div>
    </div>
  );
}
