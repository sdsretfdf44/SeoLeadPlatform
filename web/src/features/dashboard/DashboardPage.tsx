import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import {
  ArrowUpRight,
  Building2,
  CheckCircle2,
  DollarSign,
  ExternalLink,
  HandshakeIcon,
  MailCheck,
  MessageSquare,
  Plus,
  Search,
  Sparkles,
  Target,
  TrendingUp,
  Trophy
} from 'lucide-react';
import { dashboardApi } from '../../services/dashboardApi';
import { PageSpinner } from '../../components/ui/Loader';
import { AuditStatusBadge, PriorityBadge, ScoreBadge, StatusBadge } from '../../components/ui/Score';
import { CompanyAvatar } from '../../components/ui/CompanyAvatar';
import type { ComponentType } from 'react';

type Tone = 'brand' | 'emerald' | 'amber' | 'violet' | 'sky' | 'slate';

interface MetricProps {
  label: string;
  value: string | number;
  hint?: string;
  icon: ComponentType<{ size?: number; className?: string }>;
  tone?: Tone;
  emphasis?: boolean;
}

const TONE_MAP: Record<Tone, { bg: string; text: string; bar: string }> = {
  brand: { bg: 'bg-brand-50', text: 'text-brand-700', bar: 'bg-brand-500' },
  emerald: { bg: 'bg-emerald-50', text: 'text-emerald-700', bar: 'bg-emerald-500' },
  amber: { bg: 'bg-amber-50', text: 'text-amber-700', bar: 'bg-amber-500' },
  violet: { bg: 'bg-violet-50', text: 'text-violet-700', bar: 'bg-violet-500' },
  sky: { bg: 'bg-sky-50', text: 'text-sky-700', bar: 'bg-sky-500' },
  slate: { bg: 'bg-slate-100', text: 'text-slate-700', bar: 'bg-slate-400' }
};

function Metric({ label, value, hint, icon: Icon, tone = 'slate', emphasis }: MetricProps) {
  const t = TONE_MAP[tone];
  return (
    <div className="card card-body relative overflow-hidden">
      {emphasis && (
        <span className={`absolute inset-x-0 top-0 h-0.5 ${t.bar}`} />
      )}
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <div className="metric-label">{label}</div>
          <div className="metric-value mt-2">{value}</div>
          {hint && <div className="text-xs text-slate-500 mt-1">{hint}</div>}
        </div>
        <div className={`h-9 w-9 rounded-lg flex items-center justify-center shrink-0 ${t.bg} ${t.text}`}>
          <Icon size={18} />
        </div>
      </div>
    </div>
  );
}

interface FunnelStep {
  label: string;
  value: number;
  tone: Tone;
}

function FunnelChart({ steps }: { steps: FunnelStep[] }) {
  const max = Math.max(1, ...steps.map((s) => s.value));
  return (
    <div className="space-y-3">
      {steps.map((s, i) => {
        const pct = Math.max(4, Math.round((s.value / max) * 100));
        const t = TONE_MAP[s.tone];
        const dropOff = i > 0 ? steps[i - 1].value - s.value : 0;
        return (
          <div key={s.label} className="group">
            <div className="flex items-baseline justify-between text-xs mb-1.5">
              <span className="font-medium text-slate-700">{s.label}</span>
              <span className="flex items-center gap-2">
                {i > 0 && dropOff > 0 && (
                  <span className="text-[10px] text-slate-400">−{dropOff}</span>
                )}
                <span className="font-semibold text-slate-900 tabular-nums">{s.value}</span>
              </span>
            </div>
            <div className="progress">
              <div className={`progress-bar ${t.bar}`} style={{ width: `${pct}%` }} />
            </div>
          </div>
        );
      })}
    </div>
  );
}

function Sparkline({ values, tone = 'brand' }: { values: number[]; tone?: Tone }) {
  if (!values.length) return null;
  const max = Math.max(...values, 1);
  const min = Math.min(...values, 0);
  const range = max - min || 1;
  const width = 80;
  const height = 24;
  const step = width / Math.max(values.length - 1, 1);
  const points = values.map((v, i) => `${i * step},${height - ((v - min) / range) * height}`).join(' ');
  const stroke = tone === 'emerald' ? '#10b981' : tone === 'amber' ? '#f59e0b' : '#2563eb';
  return (
    <svg width={width} height={height} className="text-brand-500">
      <polyline points={points} fill="none" stroke={stroke} strokeWidth={1.5} strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}

export function DashboardPage() {
  const { data, isLoading } = useQuery({ queryKey: ['dashboard'], queryFn: dashboardApi.summary });

  if (isLoading) return <PageSpinner />;
  if (!data) return <div className="text-slate-500">Unable to load dashboard.</div>;

  const currency = new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD', maximumFractionDigits: 0 });

  const funnelSteps: FunnelStep[] = [
    { label: 'Discovered', value: data.funnel.discovered, tone: 'slate' },
    { label: 'Qualified', value: data.funnel.qualified, tone: 'brand' },
    { label: 'Contacted', value: data.funnel.contacted, tone: 'sky' },
    { label: 'Replied', value: data.funnel.replied, tone: 'violet' },
    { label: 'Meeting', value: data.funnel.meeting, tone: 'violet' },
    { label: 'Proposal', value: data.funnel.proposal, tone: 'amber' },
    { label: 'Won', value: data.funnel.won, tone: 'emerald' }
  ];

  const conversion = data.funnel.discovered > 0
    ? Math.round((data.funnel.won / data.funnel.discovered) * 100)
    : 0;

  return (
    <div>
      {/* Hero header */}
      <div className="relative mb-8 rounded-2xl bg-white border border-slate-200/80 p-6 overflow-hidden">
        <div className="absolute inset-0 opacity-40 pointer-events-none bg-grid-fade" aria-hidden />
        <div className="relative flex items-start justify-between gap-4 flex-wrap">
          <div>
            <div className="flex items-center gap-2 text-xs font-semibold text-brand-700 mb-2">
              <Sparkles size={14} />
              <span>Sales dashboard</span>
            </div>
            <h1 className="text-3xl font-semibold text-slate-900 tracking-tight">Good progress today.</h1>
            <p className="text-sm text-slate-500 mt-1 max-w-xl">
              {data.newProspects > 0
                ? `${data.newProspects} new prospect${data.newProspects === 1 ? '' : 's'} in the last 30 days · ${conversion}% conversion rate`
                : 'Add your first prospects and run audits to start filling the funnel.'}
            </p>
          </div>
          <div className="flex items-center gap-2">
            <Link to="/prospects" className="btn btn-secondary">
              <Search size={16} /> Prospects
            </Link>
            <Link to="/prospects?new=1" className="btn btn-primary">
              <Plus size={16} /> Add prospect
            </Link>
          </div>
        </div>
      </div>

      {/* Primary KPIs — emphasized */}
      <section className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-4">
        <Metric emphasis label="New prospects (30d)" value={data.newProspects} icon={Building2} tone="brand" />
        <Metric emphasis label="Qualified leads" value={data.qualifiedLeads} icon={CheckCircle2} tone="emerald" />
        <Metric emphasis label="High priority" value={data.highPriorityOpportunities} icon={TrendingUp} tone="amber" />
        <Metric emphasis label="Pipeline value" value={currency.format(data.estimatedPipelineValue)} icon={DollarSign} tone="violet" />
      </section>

      {/* Secondary KPIs */}
      <section className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <Metric label="Outreach sent" value={data.outreachSent} icon={MailCheck} tone="sky" />
        <Metric label="Replies" value={data.replies} icon={MessageSquare} tone="brand" />
        <Metric label="Meetings" value={data.meetingsScheduled} icon={HandshakeIcon} tone="violet" />
        <Metric label="Won" value={data.wonProjects} icon={Trophy} tone="emerald" />
      </section>

      {/* Funnel + follow-ups */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mt-6">
        <div className="card lg:col-span-2">
          <div className="card-header">
            <div>
              <h2 className="section-title">Acquisition funnel</h2>
              <p className="text-xs text-slate-500 mt-0.5">Every stage from discovery to close.</p>
            </div>
            <span className="chip">
              <span className="badge-dot bg-emerald-500 animate-pulse-dot" />
              Live
            </span>
          </div>
          <div className="card-body">
            <FunnelChart steps={funnelSteps} />
          </div>
        </div>

        <div className="card">
          <div className="card-header">
            <div>
              <h2 className="section-title">Follow-ups due</h2>
              <p className="text-xs text-slate-500 mt-0.5">Next 48 hours.</p>
            </div>
            <Link to="/pipeline" className="text-xs text-brand-700 font-medium inline-flex items-center gap-1 hover:text-brand-800">
              View pipeline <ArrowUpRight size={12} />
            </Link>
          </div>
          <div className="card-body pt-3">
            {data.followUpsDue.length === 0 ? (
              <div className="text-center py-6">
                <div className="mx-auto h-11 w-11 rounded-full bg-emerald-50 flex items-center justify-center text-emerald-600">
                  <CheckCircle2 size={22} />
                </div>
                <p className="text-sm font-medium text-slate-700 mt-3">You're all caught up</p>
                <p className="text-xs text-slate-500 mt-1">No follow-ups scheduled in the next 48h.</p>
              </div>
            ) : (
              <ul className="divide-y divide-slate-100">
                {data.followUpsDue.map((f) => (
                  <li key={f.leadId} className="py-3 flex items-center justify-between gap-3">
                    <div className="flex items-center gap-3 min-w-0">
                      <CompanyAvatar name={f.companyName} size="sm" />
                      <div className="min-w-0">
                        <Link to={`/prospects/${f.prospectId}`} className="text-sm font-medium text-slate-900 hover:text-brand-700 truncate block">
                          {f.companyName}
                        </Link>
                        <div className="text-[11px] text-slate-500 mt-0.5">
                          Due {new Date(f.followUpAt).toLocaleString(undefined, { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' })}
                        </div>
                      </div>
                    </div>
                    <StatusBadge status={f.status} />
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      </div>

      {/* Top opportunities + recent prospects */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mt-6">
        <div className="card">
          <div className="card-header">
            <div>
              <h2 className="section-title">Top opportunities</h2>
              <p className="text-xs text-slate-500 mt-0.5">Ranked by opportunity score.</p>
            </div>
            <Link to="/prospects?sort=score" className="text-xs text-brand-700 font-medium inline-flex items-center gap-1 hover:text-brand-800">
              View all <ArrowUpRight size={12} />
            </Link>
          </div>
          <div className="card-body pt-3">
            {data.topOpportunities.length === 0 ? (
              <div className="text-center py-6">
                <div className="mx-auto h-11 w-11 rounded-full bg-brand-50 flex items-center justify-center text-brand-600">
                  <Target size={22} />
                </div>
                <p className="text-sm font-medium text-slate-700 mt-3">Run your first audit</p>
                <p className="text-xs text-slate-500 mt-1">Add a prospect and click Run audit to compute opportunity scores.</p>
              </div>
            ) : (
              <ul className="divide-y divide-slate-100">
                {data.topOpportunities.map((p, idx) => (
                  <li key={p.id} className="py-3 flex items-center justify-between gap-3">
                    <div className="flex items-center gap-3 min-w-0">
                      <div className="flex items-center gap-2">
                        <span className="text-xs font-semibold text-slate-400 tabular-nums w-4 text-right">{idx + 1}</span>
                        <CompanyAvatar name={p.companyName} size="sm" />
                      </div>
                      <div className="min-w-0">
                        <Link to={`/prospects/${p.id}`} className="font-medium text-sm text-slate-900 hover:text-brand-700 truncate block">
                          {p.companyName}
                        </Link>
                        <div className="text-[11px] text-slate-500 truncate">
                          {[p.industry, p.city, p.country].filter(Boolean).join(' · ') || p.websiteUrl}
                        </div>
                      </div>
                    </div>
                    <div className="flex items-center gap-2 shrink-0">
                      <PriorityBadge priority={p.leadPriority} />
                      <ScoreBadge score={p.opportunityScore} size="lg" />
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>

        <div className="card">
          <div className="card-header">
            <div>
              <h2 className="section-title">Recent prospects</h2>
              <p className="text-xs text-slate-500 mt-0.5">Newest additions to your pipeline.</p>
            </div>
            <Link to="/prospects" className="text-xs text-brand-700 font-medium inline-flex items-center gap-1 hover:text-brand-800">
              View all <ArrowUpRight size={12} />
            </Link>
          </div>
          <div className="card-body pt-3">
            {data.recentProspects.length === 0 ? (
              <div className="text-center py-6">
                <div className="mx-auto h-11 w-11 rounded-full bg-brand-50 flex items-center justify-center text-brand-600">
                  <Target size={22} />
                </div>
                <p className="text-sm font-medium text-slate-700 mt-3">Nothing here yet</p>
                <p className="text-xs text-slate-500 mt-1 max-w-xs mx-auto">Add your first prospect to start filling the funnel.</p>
                <Link to="/prospects?new=1" className="btn btn-primary btn-sm mt-4">
                  <Plus size={14} /> Add prospect
                </Link>
              </div>
            ) : (
              <ul className="divide-y divide-slate-100">
                {data.recentProspects.map((p) => (
                  <li key={p.id} className="py-3 flex items-center justify-between gap-3">
                    <div className="flex items-center gap-3 min-w-0">
                      <CompanyAvatar name={p.companyName} size="sm" />
                      <div className="min-w-0">
                        <Link to={`/prospects/${p.id}`} className="font-medium text-sm text-slate-900 hover:text-brand-700 truncate block">
                          {p.companyName}
                        </Link>
                        <div className="text-[11px] text-slate-500 truncate flex items-center gap-1">
                          <ExternalLink size={10} />
                          <span className="truncate">{p.websiteUrl.replace(/^https?:\/\//, '')}</span>
                        </div>
                      </div>
                    </div>
                    <div className="flex items-center gap-2 shrink-0">
                      <AuditStatusBadge status={p.latestAuditStatus} />
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      </div>
      {/* keep sparkline export used to avoid tree-shake warnings — placeholder for future trend */}
      <div className="hidden">
        <Sparkline values={[]} />
      </div>
    </div>
  );
}
