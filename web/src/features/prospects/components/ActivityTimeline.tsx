import { useQuery } from '@tanstack/react-query';
import { Activity as ActivityIcon, Check, FileText, Handshake, Mail, MessageCircle, Search, Trophy, X } from 'lucide-react';
import { activitiesApi } from '../../../services/activitiesApi';
import type { ActivityType } from '../../../types/api';
import { formatDistanceToNow } from '../../../utils/time';

const iconFor: Record<ActivityType, { icon: typeof ActivityIcon; tone: string }> = {
  ProspectDiscovered: { icon: Search, tone: 'bg-slate-100 text-slate-600' },
  AuditCompleted: { icon: Check, tone: 'bg-emerald-100 text-emerald-700' },
  LeadQualified: { icon: Check, tone: 'bg-brand-100 text-brand-700' },
  OutreachGenerated: { icon: Mail, tone: 'bg-slate-100 text-slate-600' },
  OutreachApproved: { icon: Mail, tone: 'bg-brand-100 text-brand-700' },
  OutreachSent: { icon: Mail, tone: 'bg-brand-100 text-brand-700' },
  ReplyReceived: { icon: MessageCircle, tone: 'bg-amber-100 text-amber-700' },
  MeetingScheduled: { icon: Handshake, tone: 'bg-violet-100 text-violet-700' },
  ProposalCreated: { icon: FileText, tone: 'bg-slate-100 text-slate-600' },
  ProposalSent: { icon: FileText, tone: 'bg-violet-100 text-violet-700' },
  LeadWon: { icon: Trophy, tone: 'bg-emerald-100 text-emerald-700' },
  LeadLost: { icon: X, tone: 'bg-rose-100 text-rose-700' },
  NoteAdded: { icon: MessageCircle, tone: 'bg-slate-100 text-slate-600' },
  StatusChanged: { icon: ActivityIcon, tone: 'bg-slate-100 text-slate-600' }
};

export function ActivityTimeline({ prospectId }: { prospectId: string }) {
  const { data, isLoading } = useQuery({
    queryKey: ['activities', prospectId],
    queryFn: () => activitiesApi.listForProspect(prospectId)
  });

  return (
    <div className="card">
      <div className="card-header">
        <h2 className="section-title">Activity</h2>
        <span className="text-xs text-slate-500">{data?.length ?? 0} events</span>
      </div>
      <div className="card-body pt-2">
        {isLoading && <p className="text-sm text-slate-500 py-2">Loading activity…</p>}
        {!isLoading && (!data || data.length === 0) && (
          <p className="text-sm text-slate-500 py-4 text-center">No activity yet.</p>
        )}
        {!isLoading && data && data.length > 0 && (
          <ol className="relative border-l border-slate-100 ml-3 space-y-4 pt-2">
            {data.map((a) => {
              const { icon: Icon, tone } = iconFor[a.type] ?? { icon: ActivityIcon, tone: 'bg-slate-100 text-slate-600' };
              return (
                <li key={a.id} className="pl-6 relative">
                  <span className={`absolute -left-3 flex h-6 w-6 items-center justify-center rounded-full ${tone} ring-4 ring-white`}>
                    <Icon size={12} />
                  </span>
                  <div className="text-sm text-slate-900">{a.description}</div>
                  <div className="text-[11px] text-slate-500 mt-0.5">{formatDistanceToNow(a.createdAt)}</div>
                </li>
              );
            })}
          </ol>
        )}
      </div>
    </div>
  );
}
