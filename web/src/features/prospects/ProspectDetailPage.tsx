import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Link, useParams } from 'react-router-dom';
import { ArrowLeft, ExternalLink, MapPin, Send, Share2, Sparkles, Zap } from 'lucide-react';
import { prospectsApi } from '../../services/prospectsApi';
import { leadsApi } from '../../services/leadsApi';
import { outreachApi } from '../../services/outreachApi';
import { reportsApi } from '../../services/reportsApi';
import { PriorityBadge, ScoreBadge, SeverityBadge, StatusBadge } from '../../components/ui/Score';
import { InlineSpinner, PageSpinner } from '../../components/ui/Loader';
import type { LeadStatus, OutreachMessage } from '../../types/api';
import { apiErrorMessage } from '../../services/api';
import { useToast } from '../../components/ui/Toast';
import { ContactsCard } from './components/ContactCard';
import { ActivityTimeline } from './components/ActivityTimeline';
import { LeadDetailsCard } from './components/LeadDetailsCard';
import { ProposalsCard } from './components/ProposalsCard';

const PIPELINE: LeadStatus[] = ['New', 'Qualified', 'Contacted', 'Replied', 'MeetingScheduled', 'ProposalSent', 'Won'];

export function ProspectDetailPage() {
  const params = useParams<{ id: string }>();
  const id = params.id!;
  const queryClient = useQueryClient();
  const toast = useToast();
  const [outreachEditing, setOutreachEditing] = useState<{ id: string; subject: string; body: string } | null>(null);
  const [shareLink, setShareLink] = useState<string | null>(null);

  const prospectQuery = useQuery({
    queryKey: ['prospect', id],
    queryFn: () => prospectsApi.get(id),
    refetchInterval: (q) => q.state.data?.latestAudit?.status === 'Running' || q.state.data?.latestAudit?.status === 'Queued' ? 3000 : false
  });
  const auditQuery = useQuery({
    queryKey: ['prospect-audit', id],
    queryFn: () => prospectsApi.getLatestAudit(id),
    enabled: !!prospectQuery.data?.latestAudit,
    refetchInterval: (q) => q.state.data?.status === 'Running' || q.state.data?.status === 'Queued' ? 3000 : false
  });
  const outreachQuery = useQuery({ queryKey: ['prospect-outreach', id], queryFn: () => prospectsApi.listOutreach(id) });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['prospect', id] });
    queryClient.invalidateQueries({ queryKey: ['prospect-audit', id] });
    queryClient.invalidateQueries({ queryKey: ['prospect-outreach', id] });
    queryClient.invalidateQueries({ queryKey: ['activities', id] });
    queryClient.invalidateQueries({ queryKey: ['dashboard'] });
  };

  const startAudit = useMutation({
    mutationFn: () => prospectsApi.startAudit(id),
    onSuccess: () => {
      toast.success('SEO audit queued.');
      invalidate();
    },
    onError: (e) => toast.error(apiErrorMessage(e))
  });

  const qualify = useMutation({
    mutationFn: () => prospectsApi.qualify(id),
    onSuccess: () => {
      toast.success('Prospect qualified as a lead.');
      invalidate();
    },
    onError: (e) => toast.error(apiErrorMessage(e))
  });

  const generateOutreach = useMutation({
    mutationFn: () => prospectsApi.generateOutreach(id),
    onSuccess: () => {
      toast.success('AI outreach draft generated.');
      invalidate();
    },
    onError: (e) => toast.error(apiErrorMessage(e))
  });

  const updateOutreach = useMutation({
    mutationFn: (payload: { id: string; subject: string; body: string }) =>
      outreachApi.update(payload.id, { subject: payload.subject, body: payload.body }),
    onSuccess: () => {
      toast.success('Outreach updated.');
      queryClient.invalidateQueries({ queryKey: ['prospect-outreach', id] });
      setOutreachEditing(null);
    },
    onError: (e) => toast.error(apiErrorMessage(e))
  });

  const approveOutreach = useMutation({
    mutationFn: (mid: string) => outreachApi.approve(mid),
    onSuccess: () => {
      toast.success('Outreach approved.');
      queryClient.invalidateQueries({ queryKey: ['prospect-outreach', id] });
      queryClient.invalidateQueries({ queryKey: ['activities', id] });
    },
    onError: (e) => toast.error(apiErrorMessage(e))
  });

  const markOutreachSent = useMutation({
    mutationFn: (mid: string) => outreachApi.markSent(mid),
    onSuccess: () => {
      toast.success('Outreach marked as sent.');
      invalidate();
    },
    onError: (e) => toast.error(apiErrorMessage(e))
  });

  const updateStatus = useMutation({
    mutationFn: (status: LeadStatus) => {
      if (!prospectQuery.data?.lead) throw new Error('No lead');
      return leadsApi.updateStatus(prospectQuery.data.lead.id, status);
    },
    onSuccess: () => {
      toast.success('Pipeline updated.');
      invalidate();
    },
    onError: (e) => toast.error(apiErrorMessage(e))
  });

  const createShare = useMutation({
    mutationFn: (auditId: string) => reportsApi.createToken(auditId),
    onSuccess: (data) => {
      const url = `${window.location.origin}/reports/${data.token}`;
      setShareLink(url);
      navigator.clipboard.writeText(url).catch(() => undefined);
      toast.success('Public report link copied to clipboard.');
    },
    onError: (e) => toast.error(apiErrorMessage(e))
  });

  const opportunityFactors = useMemo(() => auditQuery.data?.opportunityScore?.factors ?? [], [auditQuery.data]);

  if (prospectQuery.isLoading) return <PageSpinner />;
  if (prospectQuery.isError || !prospectQuery.data) return <div className="text-rose-700">Prospect not found.</div>;

  const prospect = prospectQuery.data;
  const audit = auditQuery.data;
  const opportunity = audit?.opportunityScore;

  return (
    <div>
      <Link to="/prospects" className="inline-flex items-center gap-1 text-sm text-slate-500 hover:text-slate-800 mb-4">
        <ArrowLeft size={14} /> Back to prospects
      </Link>

      <div className="card mb-6">
        <div className="card-body flex flex-col md:flex-row md:items-start md:justify-between gap-4">
          <div className="min-w-0">
            <div className="flex flex-wrap items-center gap-3">
              <h1 className="text-2xl font-semibold text-slate-900">{prospect.companyName}</h1>
              <PriorityBadge priority={prospect.lead?.priority} />
              <StatusBadge status={prospect.lead?.status} />
            </div>
            <div className="mt-2 flex flex-wrap items-center gap-x-4 gap-y-1 text-sm text-slate-500">
              <a className="inline-flex items-center gap-1 text-brand-700 hover:underline" href={prospect.websiteUrl} target="_blank" rel="noreferrer">
                {prospect.websiteUrl} <ExternalLink size={12} />
              </a>
              {prospect.industry && <span>{prospect.industry}</span>}
              {(prospect.city || prospect.country) && (
                <span className="inline-flex items-center gap-1">
                  <MapPin size={12} /> {[prospect.city, prospect.state, prospect.country].filter(Boolean).join(', ')}
                </span>
              )}
            </div>
          </div>
          <div className="flex flex-col items-end gap-2 shrink-0">
            <div className="text-right">
              <div className="metric-label">Opportunity score</div>
              <div className="mt-1 flex items-center gap-2 justify-end">
                <ScoreBadge score={opportunity?.score ?? null} size="lg" />
                <PriorityBadge priority={opportunity?.priority} />
              </div>
            </div>
          </div>
        </div>
        <div className="border-t border-slate-100 px-6 py-4 flex flex-wrap gap-2">
          <button
            className="btn btn-primary"
            disabled={startAudit.isPending || audit?.status === 'Running' || audit?.status === 'Queued'}
            onClick={() => startAudit.mutate()}
          >
            {audit?.status === 'Queued' || audit?.status === 'Running' ? (
              <><InlineSpinner /> Running audit…</>
            ) : (
              <><Zap size={14} /> Run SEO audit</>
            )}
          </button>
          <button className="btn btn-secondary" disabled={!audit || audit.status !== 'Completed'} onClick={() => qualify.mutate()}>
            Qualify as lead
          </button>
          <button
            className="btn btn-secondary"
            disabled={!audit || audit.status !== 'Completed' || generateOutreach.isPending}
            onClick={() => generateOutreach.mutate()}
          >
            {generateOutreach.isPending ? <><InlineSpinner /> Generating…</> : <><Sparkles size={14} /> Generate outreach</>}
          </button>
          {audit?.status === 'Completed' && (
            <button className="btn btn-secondary" onClick={() => createShare.mutate(audit.id)}>
              <Share2 size={14} /> Share report
            </button>
          )}
        </div>
      </div>

      {shareLink && (
        <div className="card mb-6 bg-brand-50 border-brand-200">
          <div className="card-body">
            <div className="text-sm font-medium text-brand-900">Public report link</div>
            <div className="mt-1 text-sm text-brand-800 break-all font-mono">{shareLink}</div>
            <div className="text-xs text-brand-700 mt-1">Share this link with the prospect — no login required, expires in 30 days.</div>
          </div>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          {audit && (
            <div className="card">
              <div className="card-header">
                <h2 className="section-title">SEO overview</h2>
                <StatusBadge status={audit.status} />
              </div>
              <div className="card-body pt-2">
                {audit.failureReason && (
                  <div className="text-sm text-rose-700 bg-rose-50 rounded-lg p-3">{audit.failureReason}</div>
                )}
                {audit.status === 'Completed' && (
                  <>
                    <div className="grid grid-cols-2 md:grid-cols-6 gap-4">
                      <ScoreCell label="Overall" value={audit.overallScore} />
                      <ScoreCell label="Technical" value={audit.technicalScore} />
                      <ScoreCell label="On-page" value={audit.onPageScore} />
                      <ScoreCell label="Content" value={audit.contentScore} />
                      <ScoreCell label="Performance" value={audit.performanceScore} />
                      <ScoreCell label="Local" value={audit.localSeoScore} />
                    </div>
                    {audit.analysisSummary && <p className="text-sm text-slate-600 mt-4">{audit.analysisSummary}</p>}
                  </>
                )}
              </div>
            </div>
          )}

          {audit?.issues?.length ? (
            <div className="card">
              <div className="card-header">
                <h2 className="section-title">Top problems</h2>
                <span className="text-xs text-slate-500">{audit.issues.length} issues detected</span>
              </div>
              <div className="card-body pt-2">
                <ul className="divide-y divide-slate-100">
                  {audit.issues.slice(0, 8).map((issue) => (
                    <li key={issue.id} className="py-3">
                      <div className="flex items-center justify-between gap-3">
                        <div className="font-medium text-slate-900">{issue.title}</div>
                        <SeverityBadge severity={issue.severity} />
                      </div>
                      <p className="text-sm text-slate-600 mt-1">{issue.description}</p>
                      <p className="text-sm text-slate-500 mt-1">
                        <span className="font-medium text-slate-700">Recommendation:</span> {issue.recommendation}
                      </p>
                    </li>
                  ))}
                </ul>
              </div>
            </div>
          ) : null}

          {prospect.lead && (
            <div className="card">
              <div className="card-header">
                <h2 className="section-title">Sales pipeline</h2>
                <StatusBadge status={prospect.lead.status} />
              </div>
              <div className="card-body pt-2">
                <div className="flex flex-wrap gap-2">
                  {PIPELINE.map((status) => {
                    const active = prospect.lead!.status === status;
                    return (
                      <button
                        key={status}
                        type="button"
                        onClick={() => updateStatus.mutate(status)}
                        className={`px-3 py-1.5 rounded-full text-xs font-medium border transition-colors ${
                          active
                            ? 'bg-brand-600 text-white border-brand-600 shadow-sm'
                            : 'bg-white text-slate-600 border-slate-200 hover:border-brand-500 hover:text-brand-700'
                        }`}
                      >
                        {status}
                      </button>
                    );
                  })}
                </div>
              </div>
            </div>
          )}

          <div className="card">
            <div className="card-header">
              <h2 className="section-title">Outreach</h2>
              <span className="text-xs text-slate-500">{outreachQuery.data?.length ?? 0} messages</span>
            </div>
            <div className="card-body pt-2">
              {outreachQuery.data && outreachQuery.data.length === 0 && (
                <p className="text-sm text-slate-500 py-4 text-center">No outreach yet. Generate an AI draft to get started.</p>
              )}
              <ul className="space-y-3">
                {outreachQuery.data?.map((m) => (
                  <OutreachRow
                    key={m.id}
                    message={m}
                    editing={outreachEditing?.id === m.id ? outreachEditing : null}
                    onEdit={(e) => setOutreachEditing(e)}
                    onSave={(payload) => updateOutreach.mutate(payload)}
                    onApprove={(mid) => approveOutreach.mutate(mid)}
                    onMarkSent={(mid) => markOutreachSent.mutate(mid)}
                  />
                ))}
              </ul>
            </div>
          </div>

          <ProposalsCard prospectId={id} hasCompletedAudit={audit?.status === 'Completed'} />
        </div>

        <div className="space-y-6">
          {opportunity && (
            <div className="card">
              <div className="card-header">
                <h2 className="section-title">Why this is a good prospect</h2>
              </div>
              <div className="card-body pt-2">
                <p className="text-sm text-slate-700">{opportunity.reasoning}</p>
                <ul className="mt-4 space-y-3">
                  {opportunityFactors.filter((f) => f.weight > 0).slice(0, 5).map((f) => (
                    <li key={f.name} className="text-xs">
                      <div className="flex justify-between font-medium text-slate-700">
                        <span>{f.name}</span>
                        <span>{f.score}/100</span>
                      </div>
                      <div className="h-1.5 bg-slate-100 rounded-full overflow-hidden mt-1">
                        <div className="h-full bg-brand-500" style={{ width: `${f.score}%` }} />
                      </div>
                      <div className="text-slate-500 mt-1">{f.reason}</div>
                    </li>
                  ))}
                </ul>
              </div>
            </div>
          )}

          <LeadDetailsCard prospectId={id} lead={prospect.lead ?? null} />
          <ContactsCard prospectId={id} contacts={prospect.contacts} />
          <ActivityTimeline prospectId={id} />
        </div>
      </div>
    </div>
  );
}

function ScoreCell({ label, value }: { label: string; value: number }) {
  return (
    <div>
      <div className="metric-label">{label}</div>
      <div className="text-xl font-semibold text-slate-900 mt-1">{value}</div>
    </div>
  );
}

function OutreachRow({
  message,
  editing,
  onEdit,
  onSave,
  onApprove,
  onMarkSent
}: {
  message: OutreachMessage;
  editing: { id: string; subject: string; body: string } | null;
  onEdit: (v: { id: string; subject: string; body: string } | null) => void;
  onSave: (v: { id: string; subject: string; body: string }) => void;
  onApprove: (id: string) => void;
  onMarkSent: (id: string) => void;
}) {
  if (editing && editing.id === message.id) {
    return (
      <li className="border border-slate-200 rounded-xl p-4">
        <input className="input mb-2" value={editing.subject} onChange={(e) => onEdit({ ...editing, subject: e.target.value })} />
        <textarea className="input min-h-[160px]" value={editing.body} onChange={(e) => onEdit({ ...editing, body: e.target.value })} />
        <div className="flex justify-end gap-2 mt-3">
          <button className="btn btn-secondary" onClick={() => onEdit(null)}>Cancel</button>
          <button className="btn btn-primary" onClick={() => onSave(editing)}>Save</button>
        </div>
      </li>
    );
  }

  const statusTone =
    message.status === 'Sent' ? 'bg-brand-100 text-brand-800'
      : message.status === 'Approved' ? 'bg-emerald-100 text-emerald-800'
      : message.status === 'Replied' ? 'bg-amber-100 text-amber-800'
      : 'bg-slate-100 text-slate-700';

  return (
    <li className="border border-slate-200 rounded-xl p-4">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <div className="text-sm font-semibold text-slate-900">{message.subject}</div>
          <div className="text-xs text-slate-500 mt-0.5 inline-flex items-center gap-2">
            {message.type}
            <span className={`badge ${statusTone}`}>{message.status}</span>
            {message.generatedByAi && <span className="text-brand-700 font-medium">AI drafted</span>}
          </div>
        </div>
        <div className="flex gap-2 shrink-0">
          {message.status === 'Draft' && (
            <>
              <button className="btn btn-secondary btn-sm" onClick={() => onEdit({ id: message.id, subject: message.subject, body: message.body })}>Edit</button>
              <button className="btn btn-primary btn-sm" onClick={() => onApprove(message.id)}>Approve</button>
            </>
          )}
          {(message.status === 'Approved' || message.status === 'Draft') && (
            <button className="btn btn-primary btn-sm" onClick={() => onMarkSent(message.id)}>
              <Send size={12} /> Mark sent
            </button>
          )}
        </div>
      </div>
      <pre className="mt-3 whitespace-pre-wrap text-sm text-slate-700 font-sans">{message.body}</pre>
    </li>
  );
}
