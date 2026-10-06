import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { Copy, FileText, Send, Sparkles } from 'lucide-react';
import { proposalsApi } from '../../../services/proposalsApi';
import { useToast } from '../../../components/ui/Toast';
import { apiErrorMessage } from '../../../services/api';
import { Modal } from '../../../components/ui/Modal';
import type { Proposal } from '../../../types/api';

export function ProposalsCard({ prospectId, hasCompletedAudit }: { prospectId: string; hasCompletedAudit: boolean }) {
  const toast = useToast();
  const queryClient = useQueryClient();
  const [editing, setEditing] = useState<Proposal | null>(null);

  const { data: proposals } = useQuery({
    queryKey: ['proposals', prospectId],
    queryFn: () => proposalsApi.listForProspect(prospectId)
  });

  const generate = useMutation({
    mutationFn: () => proposalsApi.generate(prospectId),
    onSuccess: (proposal) => {
      toast.success('Proposal drafted.');
      queryClient.invalidateQueries({ queryKey: ['proposals', prospectId] });
      queryClient.invalidateQueries({ queryKey: ['activities', prospectId] });
      setEditing(proposal);
    },
    onError: (err) => toast.error(apiErrorMessage(err))
  });

  const markSent = useMutation({
    mutationFn: (id: string) => proposalsApi.markSent(id),
    onSuccess: () => {
      toast.success('Proposal marked as sent.');
      queryClient.invalidateQueries({ queryKey: ['proposals', prospectId] });
      queryClient.invalidateQueries({ queryKey: ['prospect', prospectId] });
      queryClient.invalidateQueries({ queryKey: ['activities', prospectId] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
    },
    onError: (err) => toast.error(apiErrorMessage(err))
  });

  const currency = new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD', maximumFractionDigits: 0 });

  return (
    <div className="card">
      <div className="card-header">
        <div>
          <h2 className="section-title">Proposals</h2>
          <p className="text-xs text-slate-500 mt-1">Turn qualified leads into signed projects.</p>
        </div>
        <button
          type="button"
          className="btn btn-primary btn-sm"
          disabled={!hasCompletedAudit || generate.isPending}
          onClick={() => generate.mutate()}
        >
          <Sparkles size={14} />
          {generate.isPending ? 'Generating…' : 'Generate proposal'}
        </button>
      </div>
      <div className="card-body pt-2">
        {!hasCompletedAudit && (
          <p className="text-sm text-slate-500 py-2">Complete an SEO audit before generating a proposal.</p>
        )}
        {hasCompletedAudit && (!proposals || proposals.length === 0) && (
          <div className="text-center py-6">
            <div className="mx-auto h-10 w-10 rounded-full bg-brand-50 flex items-center justify-center text-brand-600">
              <FileText size={20} />
            </div>
            <p className="text-sm text-slate-500 mt-2">Draft an AI-generated proposal tailored to the audit findings.</p>
          </div>
        )}
        {proposals && proposals.length > 0 && (
          <ul className="divide-y divide-slate-100">
            {proposals.map((p) => (
              <li key={p.id} className="py-3 flex items-start justify-between gap-3">
                <div className="min-w-0 flex-1">
                  <div className="flex items-center gap-2">
                    <span className="font-medium text-slate-900">{p.title}</span>
                    <ProposalStatusBadge status={p.status} />
                  </div>
                  <div className="text-xs text-slate-500 mt-1">
                    Pricing: {currency.format(p.pricing)}
                    {p.monthlyRetainer ? ` · ${currency.format(p.monthlyRetainer)}/mo` : ''}
                    {p.sentAt ? ` · sent ${new Date(p.sentAt).toLocaleDateString()}` : ''}
                  </div>
                </div>
                <div className="flex gap-1">
                  {p.publicToken && (
                    <button
                      type="button"
                      className="btn btn-ghost btn-sm"
                      onClick={() => {
                        const url = `${window.location.origin}/proposals/${p.publicToken}`;
                        navigator.clipboard.writeText(url).then(() => toast.success('Public link copied.'));
                      }}
                      title="Copy public link"
                    >
                      <Copy size={14} />
                    </button>
                  )}
                  {p.status === 'Draft' && (
                    <>
                      <button className="btn btn-secondary btn-sm" onClick={() => setEditing(p)}>Edit</button>
                      <button className="btn btn-primary btn-sm" onClick={() => markSent.mutate(p.id)}>
                        <Send size={12} /> Mark sent
                      </button>
                    </>
                  )}
                  {p.publicToken && (
                    <Link to={`/proposals/${p.publicToken}`} target="_blank" className="btn btn-secondary btn-sm">
                      View
                    </Link>
                  )}
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>

      {editing && (
        <ProposalEditor
          proposal={editing}
          onClose={() => setEditing(null)}
          onSaved={() => {
            queryClient.invalidateQueries({ queryKey: ['proposals', prospectId] });
            setEditing(null);
          }}
        />
      )}
    </div>
  );
}

function ProposalStatusBadge({ status }: { status: string }) {
  const tone =
    status === 'Sent' ? 'bg-brand-100 text-brand-800'
      : status === 'Accepted' ? 'bg-emerald-100 text-emerald-800'
      : status === 'Rejected' ? 'bg-rose-100 text-rose-800'
      : 'bg-slate-100 text-slate-700';
  return <span className={`badge ${tone}`}>{status}</span>;
}

function ProposalEditor({
  proposal,
  onClose,
  onSaved
}: {
  proposal: Proposal;
  onClose: () => void;
  onSaved: () => void;
}) {
  const toast = useToast();
  const [state, setState] = useState({
    title: proposal.title,
    executiveSummary: proposal.executiveSummary,
    problemsIdentified: proposal.problemsIdentified,
    recommendedServices: proposal.recommendedServices,
    deliverables: proposal.deliverables,
    timeline: proposal.timeline,
    pricing: proposal.pricing,
    monthlyRetainer: proposal.monthlyRetainer ?? null,
    terms: proposal.terms
  });

  const save = useMutation({
    mutationFn: () => proposalsApi.update(proposal.id, state),
    onSuccess: () => {
      toast.success('Proposal saved.');
      onSaved();
    },
    onError: (err) => toast.error(apiErrorMessage(err))
  });

  return (
    <Modal open onClose={onClose} title="Edit proposal" size="lg">
      <div className="space-y-3">
        <div>
          <label className="label">Title</label>
          <input className="input" value={state.title} onChange={(e) => setState({ ...state, title: e.target.value })} />
        </div>
        <div>
          <label className="label">Executive summary</label>
          <textarea className="input min-h-[80px]" value={state.executiveSummary} onChange={(e) => setState({ ...state, executiveSummary: e.target.value })} />
        </div>
        <div>
          <label className="label">Problems identified</label>
          <textarea className="input min-h-[120px] font-mono text-xs" value={state.problemsIdentified} onChange={(e) => setState({ ...state, problemsIdentified: e.target.value })} />
        </div>
        <div>
          <label className="label">Recommended services</label>
          <textarea className="input min-h-[80px] font-mono text-xs" value={state.recommendedServices} onChange={(e) => setState({ ...state, recommendedServices: e.target.value })} />
        </div>
        <div>
          <label className="label">Deliverables</label>
          <textarea className="input min-h-[80px] font-mono text-xs" value={state.deliverables} onChange={(e) => setState({ ...state, deliverables: e.target.value })} />
        </div>
        <div>
          <label className="label">Timeline</label>
          <textarea className="input min-h-[60px]" value={state.timeline} onChange={(e) => setState({ ...state, timeline: e.target.value })} />
        </div>
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="label">Pricing ($)</label>
            <input type="number" className="input" value={state.pricing} onChange={(e) => setState({ ...state, pricing: Number(e.target.value) })} />
          </div>
          <div>
            <label className="label">Monthly retainer ($)</label>
            <input type="number" className="input" value={state.monthlyRetainer ?? ''} onChange={(e) => setState({ ...state, monthlyRetainer: e.target.value ? Number(e.target.value) : null })} />
          </div>
        </div>
        <div>
          <label className="label">Terms</label>
          <textarea className="input min-h-[80px]" value={state.terms} onChange={(e) => setState({ ...state, terms: e.target.value })} />
        </div>
        <div className="flex justify-end gap-2 pt-2">
          <button type="button" className="btn btn-secondary" onClick={onClose}>Close</button>
          <button type="button" className="btn btn-primary" disabled={save.isPending} onClick={() => save.mutate()}>
            {save.isPending ? 'Saving…' : 'Save proposal'}
          </button>
        </div>
      </div>
    </Modal>
  );
}
