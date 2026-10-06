import { clsx } from 'clsx';

const LEAD_STATUS_LABELS: Record<string, string> = {
  New: 'New',
  Qualified: 'Qualified',
  Contacted: 'Contacted',
  Replied: 'Replied',
  MeetingScheduled: 'Meeting scheduled',
  ProposalSent: 'Proposal sent',
  Negotiation: 'Negotiation',
  Won: 'Won',
  Lost: 'Lost',
  Disqualified: 'Disqualified'
};

const LEAD_STATUS_TONES: Record<string, { bg: string; text: string; dot: string }> = {
  New: { bg: 'bg-slate-100', text: 'text-slate-700', dot: 'bg-slate-400' },
  Qualified: { bg: 'bg-brand-50', text: 'text-brand-700', dot: 'bg-brand-500' },
  Contacted: { bg: 'bg-sky-50', text: 'text-sky-700', dot: 'bg-sky-500' },
  Replied: { bg: 'bg-cyan-50', text: 'text-cyan-700', dot: 'bg-cyan-500' },
  MeetingScheduled: { bg: 'bg-indigo-50', text: 'text-indigo-700', dot: 'bg-indigo-500' },
  ProposalSent: { bg: 'bg-violet-50', text: 'text-violet-700', dot: 'bg-violet-500' },
  Negotiation: { bg: 'bg-fuchsia-50', text: 'text-fuchsia-700', dot: 'bg-fuchsia-500' },
  Won: { bg: 'bg-emerald-50', text: 'text-emerald-700', dot: 'bg-emerald-500' },
  Lost: { bg: 'bg-slate-100', text: 'text-slate-500', dot: 'bg-slate-400' },
  Disqualified: { bg: 'bg-slate-100', text: 'text-slate-500', dot: 'bg-slate-400' }
};

export function ScoreBadge({ score, size = 'md' }: { score: number | null | undefined; size?: 'sm' | 'md' | 'lg' }) {
  if (score == null) {
    return <span className="badge bg-slate-100 text-slate-500">—</span>;
  }
  const tone =
    score >= 80 ? 'bg-emerald-50 text-emerald-700 ring-1 ring-inset ring-emerald-600/10'
      : score >= 65 ? 'bg-brand-50 text-brand-700 ring-1 ring-inset ring-brand-600/10'
      : score >= 45 ? 'bg-amber-50 text-amber-800 ring-1 ring-inset ring-amber-600/10'
      : 'bg-slate-100 text-slate-700 ring-1 ring-inset ring-slate-600/10';
  return (
    <span
      className={clsx(
        'inline-flex items-center rounded-md font-semibold tabular-nums',
        tone,
        size === 'lg' ? 'text-sm px-2.5 py-1' : size === 'sm' ? 'text-[10px] px-1.5 py-0.5' : 'text-xs px-2 py-0.5'
      )}
    >
      {score}
    </span>
  );
}

export function ScoreBar({ score, className }: { score: number | null | undefined; className?: string }) {
  if (score == null) {
    return <span className="text-xs text-slate-400">—</span>;
  }
  const tone =
    score >= 80 ? 'bg-emerald-500'
      : score >= 65 ? 'bg-brand-500'
      : score >= 45 ? 'bg-amber-500'
      : 'bg-slate-400';
  return (
    <div className={clsx('flex items-center gap-2', className)}>
      <div className="progress w-16">
        <div className={clsx('progress-bar', tone)} style={{ width: `${Math.min(100, score)}%` }} />
      </div>
      <span className="text-xs font-semibold text-slate-700 tabular-nums w-6 text-right">{score}</span>
    </div>
  );
}

export function PriorityBadge({ priority }: { priority?: string | null }) {
  if (!priority) return <span className="badge bg-slate-100 text-slate-500">—</span>;
  const tone =
    priority === 'Critical' ? { bg: 'bg-rose-50', text: 'text-rose-700', dot: 'bg-rose-500' }
      : priority === 'High' ? { bg: 'bg-amber-50', text: 'text-amber-700', dot: 'bg-amber-500' }
      : priority === 'Medium' ? { bg: 'bg-brand-50', text: 'text-brand-700', dot: 'bg-brand-500' }
      : { bg: 'bg-slate-100', text: 'text-slate-600', dot: 'bg-slate-400' };
  return (
    <span className={clsx('badge', tone.bg, tone.text)}>
      <span className={clsx('badge-dot', tone.dot)} />
      {priority}
    </span>
  );
}

export function StatusBadge({ status }: { status?: string | null }) {
  if (!status) return null;
  const tone = LEAD_STATUS_TONES[status] ?? LEAD_STATUS_TONES.New;
  const label = LEAD_STATUS_LABELS[status] ?? status;
  return (
    <span className={clsx('badge', tone.bg, tone.text)}>
      <span className={clsx('badge-dot', tone.dot)} />
      {label}
    </span>
  );
}

export function AuditStatusBadge({ status }: { status?: string | null }) {
  if (!status) {
    return (
      <span className="badge bg-slate-100 text-slate-500">
        <span className="badge-dot bg-slate-400" />
        No audit
      </span>
    );
  }
  if (status === 'Completed') {
    return (
      <span className="badge bg-emerald-50 text-emerald-700">
        <span className="badge-dot bg-emerald-500" />
        Completed
      </span>
    );
  }
  if (status === 'Failed') {
    return (
      <span className="badge bg-rose-50 text-rose-700">
        <span className="badge-dot bg-rose-500" />
        Failed
      </span>
    );
  }
  // Queued / Running
  const label = status === 'Queued' ? 'Queued' : 'Running';
  return (
    <span className="badge bg-brand-50 text-brand-700">
      <span className="badge-dot bg-brand-500 animate-pulse-dot" />
      {label}
    </span>
  );
}

export function SeverityBadge({ severity }: { severity: string }) {
  const tone =
    severity === 'Critical' ? { bg: 'bg-rose-50', text: 'text-rose-700', dot: 'bg-rose-500' }
      : severity === 'High' ? { bg: 'bg-amber-50', text: 'text-amber-700', dot: 'bg-amber-500' }
      : severity === 'Medium' ? { bg: 'bg-brand-50', text: 'text-brand-700', dot: 'bg-brand-500' }
      : severity === 'Low' ? { bg: 'bg-slate-100', text: 'text-slate-600', dot: 'bg-slate-400' }
      : { bg: 'bg-slate-50', text: 'text-slate-500', dot: 'bg-slate-300' };
  return (
    <span className={clsx('badge', tone.bg, tone.text)}>
      <span className={clsx('badge-dot', tone.dot)} />
      {severity}
    </span>
  );
}
