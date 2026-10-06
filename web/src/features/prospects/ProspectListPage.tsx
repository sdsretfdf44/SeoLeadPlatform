import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import {
  ArrowRight,
  ChevronLeft,
  ChevronRight,
  ExternalLink,
  Plus,
  Search,
  SlidersHorizontal,
  Target,
  X
} from 'lucide-react';
import { prospectsApi, type ProspectSearchQuery } from '../../services/prospectsApi';
import { apiErrorMessage } from '../../services/api';
import {
  AuditStatusBadge,
  PriorityBadge,
  ScoreBar,
  StatusBadge
} from '../../components/ui/Score';
import { CompanyAvatar } from '../../components/ui/CompanyAvatar';
import { PageSpinner } from '../../components/ui/Loader';
import { EmptyState } from '../../components/ui/EmptyState';

const createSchema = z.object({
  companyName: z.string().min(2, 'Company name required'),
  websiteUrl: z.string().url('Enter a valid URL').or(z.string().regex(/^[^\s]+\.[^\s]+$/, 'Enter a valid URL')),
  industry: z.string().optional(),
  city: z.string().optional(),
  country: z.string().optional()
});
type CreateValues = z.infer<typeof createSchema>;

export function ProspectListPage() {
  const [query, setQuery] = useState<ProspectSearchQuery>({ page: 1, pageSize: 20, sortBy: 'opportunityScore' });
  const [creating, setCreating] = useState(false);
  const [filtersOpen, setFiltersOpen] = useState(false);
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ['prospects', query],
    queryFn: () => prospectsApi.search(query)
  });

  const createForm = useForm<CreateValues>({ resolver: zodResolver(createSchema) });
  const [createError, setCreateError] = useState<string | null>(null);

  const createMutation = useMutation({
    mutationFn: prospectsApi.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['prospects'] });
      setCreating(false);
      createForm.reset();
    },
    onError: (err) => setCreateError(apiErrorMessage(err))
  });

  const activeFilters = [
    query.industry && { key: 'industry', label: `Industry: ${query.industry}` },
    query.country && { key: 'country', label: `Country: ${query.country}` },
    query.minOpportunityScore != null && { key: 'minOpportunityScore', label: `Min score ≥ ${query.minOpportunityScore}` }
  ].filter(Boolean) as { key: keyof ProspectSearchQuery; label: string }[];

  const clearFilter = (key: keyof ProspectSearchQuery) =>
    setQuery((q) => ({ ...q, [key]: undefined, page: 1 }));

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-start justify-between gap-4 flex-wrap">
        <div>
          <h1 className="text-2xl font-semibold text-slate-900 tracking-tight">Prospects</h1>
          <p className="text-sm text-slate-500 mt-1">Find and qualify businesses that need SEO help.</p>
        </div>
        <div className="flex items-center gap-2">
          <button
            type="button"
            className="btn btn-secondary"
            onClick={() => setFiltersOpen((v) => !v)}
          >
            <SlidersHorizontal size={16} /> Filters
            {activeFilters.length > 0 && (
              <span className="chip !py-0 !px-1.5 !bg-brand-600 !text-white !border-brand-700">
                {activeFilters.length}
              </span>
            )}
          </button>
          <button type="button" className="btn btn-primary" onClick={() => setCreating(true)}>
            <Plus size={16} /> Add prospect
          </button>
        </div>
      </div>

      {/* Search + filter panel */}
      <div className="card">
        <div className="card-body space-y-3">
          <div className="relative">
            <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
            <input
              className="input pl-9"
              placeholder="Search by company name or URL…"
              value={query.search ?? ''}
              onChange={(e) => setQuery((q) => ({ ...q, search: e.target.value || undefined, page: 1 }))}
            />
          </div>

          {filtersOpen && (
            <div className="grid grid-cols-1 md:grid-cols-3 gap-3 pt-2">
              <div>
                <label className="label">Industry</label>
                <input
                  className="input"
                  placeholder="e.g. Dental, Legal, SaaS"
                  value={query.industry ?? ''}
                  onChange={(e) => setQuery((q) => ({ ...q, industry: e.target.value || undefined, page: 1 }))}
                />
              </div>
              <div>
                <label className="label">Country</label>
                <input
                  className="input"
                  placeholder="e.g. US, DE"
                  value={query.country ?? ''}
                  onChange={(e) => setQuery((q) => ({ ...q, country: e.target.value || undefined, page: 1 }))}
                />
              </div>
              <div>
                <label className="label">Min opportunity score</label>
                <input
                  className="input"
                  type="number"
                  min={0}
                  max={100}
                  placeholder="0–100"
                  value={query.minOpportunityScore ?? ''}
                  onChange={(e) => setQuery((q) => ({ ...q, minOpportunityScore: e.target.value ? Number(e.target.value) : undefined, page: 1 }))}
                />
              </div>
            </div>
          )}

          {activeFilters.length > 0 && (
            <div className="flex flex-wrap items-center gap-2 pt-1">
              {activeFilters.map((f) => (
                <button
                  key={f.key}
                  type="button"
                  onClick={() => clearFilter(f.key)}
                  className="chip hover:bg-slate-50 group"
                >
                  {f.label}
                  <X size={12} className="text-slate-400 group-hover:text-slate-700" />
                </button>
              ))}
              <button
                type="button"
                className="text-xs text-slate-500 hover:text-slate-800 ml-1"
                onClick={() => setQuery((q) => ({ ...q, industry: undefined, country: undefined, minOpportunityScore: undefined, page: 1 }))}
              >
                Clear all
              </button>
            </div>
          )}
        </div>
      </div>

      {/* Table / list */}
      {isLoading && <PageSpinner />}

      {!isLoading && data && data.items.length === 0 && (
        <EmptyState
          title="No prospects yet"
          description="Add businesses you want to qualify. We'll run a mini SEO audit and calculate an opportunity score."
          action={
            <button type="button" className="btn btn-primary" onClick={() => setCreating(true)}>
              <Plus size={16} /> Add first prospect
            </button>
          }
        />
      )}

      {!isLoading && data && data.items.length > 0 && (
        <div className="card overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-slate-50/80 backdrop-blur sticky top-0 text-[11px] uppercase tracking-wider text-slate-500 font-semibold">
                <tr>
                  <th className="text-left px-5 py-3">Company</th>
                  <th className="text-left px-4 py-3">Industry</th>
                  <th className="text-left px-4 py-3">Location</th>
                  <th className="text-left px-4 py-3 w-32">Audit</th>
                  <th className="text-left px-4 py-3 w-40">SEO score</th>
                  <th className="text-left px-4 py-3 w-40">Opportunity</th>
                  <th className="text-left px-4 py-3">Priority</th>
                  <th className="text-left px-4 py-3">Status</th>
                  <th className="w-10"></th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {data.items.map((p) => (
                  <tr key={p.id} className="table-row-hover group">
                    <td className="px-5 py-3">
                      <Link to={`/prospects/${p.id}`} className="flex items-center gap-3 min-w-0">
                        <CompanyAvatar name={p.companyName} />
                        <div className="min-w-0">
                          <div className="font-medium text-slate-900 group-hover:text-brand-700 truncate max-w-[220px]">
                            {p.companyName}
                          </div>
                          <div className="text-xs text-slate-500 flex items-center gap-1 truncate max-w-[220px]">
                            <ExternalLink size={10} />
                            <span className="truncate">{p.websiteUrl.replace(/^https?:\/\//, '')}</span>
                          </div>
                        </div>
                      </Link>
                    </td>
                    <td className="px-4 py-3">
                      {p.industry ? (
                        <span className="chip">{p.industry}</span>
                      ) : (
                        <span className="text-slate-400">—</span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-slate-700">
                      {[p.city, p.country].filter(Boolean).join(', ') || <span className="text-slate-400">—</span>}
                    </td>
                    <td className="px-4 py-3">
                      <AuditStatusBadge status={p.latestAuditStatus} />
                    </td>
                    <td className="px-4 py-3">
                      <ScoreBar score={p.overallSeoScore ?? null} />
                    </td>
                    <td className="px-4 py-3">
                      <ScoreBar score={p.opportunityScore ?? null} />
                    </td>
                    <td className="px-4 py-3">
                      <PriorityBadge priority={p.leadPriority} />
                    </td>
                    <td className="px-4 py-3">
                      {p.leadStatus ? (
                        <StatusBadge status={p.leadStatus} />
                      ) : (
                        <span className="text-slate-400 text-xs">—</span>
                      )}
                    </td>
                    <td className="px-3 py-3 text-right">
                      <Link
                        to={`/prospects/${p.id}`}
                        className="inline-flex h-7 w-7 items-center justify-center rounded-md text-slate-400 opacity-0 group-hover:opacity-100 hover:bg-brand-50 hover:text-brand-700 transition"
                        aria-label="Open"
                      >
                        <ArrowRight size={14} />
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="flex items-center justify-between px-5 py-3 text-xs text-slate-500 border-t border-slate-100 bg-white">
            <span>
              Showing <span className="font-semibold text-slate-700 tabular-nums">{data.items.length}</span> of{' '}
              <span className="font-semibold text-slate-700 tabular-nums">{data.totalCount}</span>
            </span>
            <div className="flex items-center gap-2">
              <button
                type="button"
                className="btn btn-secondary btn-sm"
                disabled={(query.page ?? 1) <= 1}
                onClick={() => setQuery((q) => ({ ...q, page: Math.max(1, (q.page ?? 1) - 1) }))}
              >
                <ChevronLeft size={14} /> Prev
              </button>
              <span className="text-xs text-slate-500 tabular-nums">
                Page {query.page ?? 1} of {data.totalPages}
              </span>
              <button
                type="button"
                className="btn btn-secondary btn-sm"
                disabled={(query.page ?? 1) >= data.totalPages}
                onClick={() => setQuery((q) => ({ ...q, page: (q.page ?? 1) + 1 }))}
              >
                Next <ChevronRight size={14} />
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Add prospect modal */}
      {creating && (
        <div
          className="fixed inset-0 bg-slate-900/50 backdrop-blur-sm flex items-center justify-center px-4 z-50"
          onClick={() => setCreating(false)}
        >
          <div className="card w-full max-w-lg animate-in fade-in zoom-in-95" onClick={(e) => e.stopPropagation()}>
            <div className="card-header">
              <div className="flex items-center gap-3">
                <div className="h-10 w-10 rounded-lg bg-brand-50 text-brand-700 flex items-center justify-center">
                  <Target size={20} />
                </div>
                <div>
                  <h2 className="text-lg font-semibold text-slate-900">Add prospect</h2>
                  <p className="text-xs text-slate-500 mt-0.5">Add a business you'd like to analyse and potentially work with.</p>
                </div>
              </div>
              <button
                type="button"
                onClick={() => setCreating(false)}
                className="text-slate-400 hover:text-slate-700 p-1 rounded-md hover:bg-slate-100"
                aria-label="Close"
              >
                <X size={18} />
              </button>
            </div>
            <div className="card-body pt-4">
              <form
                className="space-y-3"
                onSubmit={createForm.handleSubmit((values) => {
                  setCreateError(null);
                  createMutation.mutate({
                    companyName: values.companyName,
                    websiteUrl: values.websiteUrl,
                    industry: values.industry,
                    city: values.city,
                    country: values.country
                  });
                })}
              >
                {createError && (
                  <div className="text-sm text-rose-700 bg-rose-50 rounded-lg p-3 border border-rose-100">
                    {createError}
                  </div>
                )}
                <div>
                  <label className="label">Company name</label>
                  <input className="input" placeholder="Acme Dental Group" {...createForm.register('companyName')} />
                  {createForm.formState.errors.companyName && (
                    <p className="text-xs text-rose-600 mt-1">{createForm.formState.errors.companyName.message}</p>
                  )}
                </div>
                <div>
                  <label className="label">Website URL</label>
                  <input className="input" placeholder="https://example.com" {...createForm.register('websiteUrl')} />
                  {createForm.formState.errors.websiteUrl && (
                    <p className="text-xs text-rose-600 mt-1">{createForm.formState.errors.websiteUrl.message}</p>
                  )}
                </div>
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="label">Industry</label>
                    <input className="input" placeholder="Dental, Legal…" {...createForm.register('industry')} />
                  </div>
                  <div>
                    <label className="label">City</label>
                    <input className="input" placeholder="Austin" {...createForm.register('city')} />
                  </div>
                </div>
                <div>
                  <label className="label">Country</label>
                  <input className="input" placeholder="US" {...createForm.register('country')} />
                </div>
                <div className="flex justify-end gap-2 pt-3 border-t border-slate-100 -mx-6 px-6 mt-4">
                  <button type="button" className="btn btn-secondary" onClick={() => setCreating(false)}>
                    Cancel
                  </button>
                  <button type="submit" className="btn btn-primary" disabled={createMutation.isPending}>
                    {createMutation.isPending ? 'Saving…' : (
                      <>
                        <Plus size={16} /> Add prospect
                      </>
                    )}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
