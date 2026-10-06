import { useQuery } from '@tanstack/react-query';
import { useParams } from 'react-router-dom';
import { proposalsApi } from '../../services/proposalsApi';
import { PageSpinner } from '../../components/ui/Loader';

export function PublicProposalPage() {
  const { token } = useParams<{ token: string }>();
  const { data, isLoading, isError } = useQuery({
    queryKey: ['public-proposal', token],
    queryFn: () => proposalsApi.getPublic(token!),
    enabled: !!token
  });

  if (isLoading) return <PageSpinner />;
  if (isError || !data) {
    return (
      <div className="min-h-screen flex items-center justify-center p-8">
        <div className="text-center">
          <h1 className="text-2xl font-semibold text-slate-900">Proposal not available</h1>
          <p className="text-sm text-slate-500 mt-2">This link may have expired or been revoked.</p>
        </div>
      </div>
    );
  }

  const currency = new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD', maximumFractionDigits: 0 });

  return (
    <div className="min-h-screen bg-slate-50">
      <div className="max-w-3xl mx-auto px-6 py-12 space-y-6">
        <header className="text-center">
          <div className="text-xs uppercase tracking-widest text-brand-700">Proposal</div>
          <h1 className="text-3xl font-semibold text-slate-900 mt-2">{data.title}</h1>
          <div className="text-sm text-slate-500 mt-1">Prepared for {data.companyName}</div>
        </header>

        <Section title="Executive summary">{data.executiveSummary}</Section>
        <Section title="Problems identified" mono>{data.problemsIdentified}</Section>
        <Section title="Recommended services" mono>{data.recommendedServices}</Section>
        <Section title="Deliverables" mono>{data.deliverables}</Section>
        <Section title="Timeline">{data.timeline}</Section>

        <div className="card">
          <div className="card-body">
            <h2 className="section-title">Investment</h2>
            <div className="mt-4 grid grid-cols-2 gap-4">
              <div>
                <div className="metric-label">Engagement</div>
                <div className="text-2xl font-semibold text-slate-900 mt-1">{currency.format(data.pricing)}</div>
              </div>
              {data.monthlyRetainer != null && (
                <div>
                  <div className="metric-label">Monthly retainer</div>
                  <div className="text-2xl font-semibold text-slate-900 mt-1">{currency.format(data.monthlyRetainer)}<span className="text-sm text-slate-500 font-normal">/mo</span></div>
                </div>
              )}
            </div>
          </div>
        </div>

        <Section title="Terms">{data.terms}</Section>

        <div className="card bg-brand-600 text-white">
          <div className="card-body text-center py-8">
            <div className="text-lg font-semibold">Ready to move forward?</div>
            <p className="text-sm text-brand-100 mt-2">Reply to confirm and we'll schedule kick-off within one business week.</p>
          </div>
        </div>

        <div className="text-center text-xs text-slate-400">
          Proposal generated {new Date(data.createdAt).toLocaleDateString()}
        </div>
      </div>
    </div>
  );
}

function Section({ title, children, mono = false }: { title: string; children: string; mono?: boolean }) {
  if (!children) return null;
  return (
    <div className="card">
      <div className="card-body">
        <h2 className="section-title">{title}</h2>
        <div className={`mt-3 text-sm text-slate-700 whitespace-pre-wrap ${mono ? 'font-mono text-xs leading-relaxed' : ''}`}>
          {children}
        </div>
      </div>
    </div>
  );
}
