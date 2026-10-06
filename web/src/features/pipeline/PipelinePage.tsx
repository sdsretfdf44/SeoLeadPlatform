import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { prospectsApi } from '../../services/prospectsApi';
import { PageSpinner } from '../../components/ui/Loader';
import { PageHeader } from '../../components/ui/PageHeader';
import { PriorityBadge, ScoreBadge } from '../../components/ui/Score';
import type { LeadStatus, ProspectListItem } from '../../types/api';

const STAGES: LeadStatus[] = ['New', 'Qualified', 'Contacted', 'Replied', 'MeetingScheduled', 'ProposalSent', 'Won'];

export function PipelinePage() {
  const { data, isLoading } = useQuery({
    queryKey: ['pipeline'],
    queryFn: () => prospectsApi.search({ page: 1, pageSize: 200, sortBy: 'opportunityScore', sortDescending: true })
  });

  const grouped = useMemo(() => {
    const map = new Map<LeadStatus, ProspectListItem[]>();
    STAGES.forEach((s) => map.set(s, []));
    (data?.items ?? []).forEach((p) => {
      if (!p.leadStatus) return;
      if (!STAGES.includes(p.leadStatus as LeadStatus)) return;
      map.get(p.leadStatus as LeadStatus)!.push(p);
    });
    return map;
  }, [data]);

  if (isLoading) return <PageSpinner />;

  return (
    <div>
      <PageHeader
        title="Sales pipeline"
        description="Every qualified lead grouped by stage. Click a card to update status and next steps."
      />
      <div className="grid grid-cols-2 lg:grid-cols-4 xl:grid-cols-7 gap-4">
        {STAGES.map((stage) => {
          const items = grouped.get(stage) ?? [];
          return (
            <div key={stage} className="bg-slate-100/70 rounded-2xl p-3">
              <div className="flex items-center justify-between mb-3 px-1">
                <div className="text-xs font-semibold text-slate-700 uppercase tracking-wide">{stage}</div>
                <span className="text-xs text-slate-500">{items.length}</span>
              </div>
              <div className="space-y-2">
                {items.map((p) => (
                  <Link
                    key={p.id}
                    to={`/prospects/${p.id}`}
                    className="block bg-white rounded-xl p-3 border border-slate-200 hover:border-brand-300 hover:shadow-sm transition"
                  >
                    <div className="text-sm font-medium text-slate-900 truncate">{p.companyName}</div>
                    <div className="text-[11px] text-slate-500 truncate">{[p.city, p.country].filter(Boolean).join(', ') || p.websiteUrl}</div>
                    <div className="flex items-center justify-between mt-2">
                      <PriorityBadge priority={p.leadPriority} />
                      <ScoreBadge score={p.opportunityScore ?? null} size="sm" />
                    </div>
                  </Link>
                ))}
                {items.length === 0 && (
                  <div className="text-[11px] text-slate-400 text-center py-4">No leads</div>
                )}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
