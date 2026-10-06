import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { agencyApi } from '../../services/agencyApi';
import { PageSpinner } from '../../components/ui/Loader';
import { useEffect, useState } from 'react';
import { apiErrorMessage } from '../../services/api';

interface FormValues {
  name: string;
  websiteUrl?: string;
  description?: string;
  targetIndustries?: string;
  targetLocations?: string;
  targetServices?: string;
  minimumProjectValue?: number;
  preferredProjectTypes?: string;
}

export function AgencyPage() {
  const queryClient = useQueryClient();
  const { data, isLoading } = useQuery({ queryKey: ['agency'], queryFn: agencyApi.get });
  const [saveMessage, setSaveMessage] = useState<string | null>(null);
  const [saveError, setSaveError] = useState<string | null>(null);
  const [newService, setNewService] = useState({ name: '', typicalProjectValue: '' });

  const form = useForm<FormValues>();
  useEffect(() => {
    if (data) form.reset(data as FormValues);
  }, [data, form]);

  const update = useMutation({
    mutationFn: (values: FormValues) => agencyApi.update(values),
    onSuccess: () => {
      setSaveMessage('Agency profile updated.');
      queryClient.invalidateQueries({ queryKey: ['agency'] });
    },
    onError: (err) => setSaveError(apiErrorMessage(err))
  });

  const addService = useMutation({
    mutationFn: (body: { name: string; typicalProjectValue?: number }) => agencyApi.addService(body),
    onSuccess: () => {
      setNewService({ name: '', typicalProjectValue: '' });
      queryClient.invalidateQueries({ queryKey: ['agency'] });
    }
  });

  if (isLoading || !data) return <PageSpinner />;

  return (
    <div className="space-y-6">
      <header>
        <h1 className="text-2xl font-semibold text-slate-900">Agency profile</h1>
        <p className="text-sm text-slate-500 mt-1">Your profile powers targeting, outreach and reporting.</p>
      </header>

      {saveMessage && <div className="rounded-lg bg-brand-50 text-brand-800 px-4 py-2 text-sm">{saveMessage}</div>}
      {saveError && <div className="rounded-lg bg-rose-50 text-rose-800 px-4 py-2 text-sm">{saveError}</div>}

      <form
        className="card card-body space-y-4"
        onSubmit={form.handleSubmit((values) => {
          setSaveError(null);
          setSaveMessage(null);
          update.mutate(values);
        })}
      >
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="label">Agency name</label>
            <input className="input" {...form.register('name')} />
          </div>
          <div>
            <label className="label">Website</label>
            <input className="input" {...form.register('websiteUrl')} />
          </div>
        </div>
        <div>
          <label className="label">Description</label>
          <textarea className="input min-h-[80px]" {...form.register('description')} />
        </div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="label">Target industries</label>
            <input className="input" placeholder="dentists, law firms…" {...form.register('targetIndustries')} />
          </div>
          <div>
            <label className="label">Target locations</label>
            <input className="input" placeholder="US, UK…" {...form.register('targetLocations')} />
          </div>
          <div>
            <label className="label">Services offered</label>
            <input className="input" placeholder="local SEO, content, ads" {...form.register('targetServices')} />
          </div>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="label">Minimum project value ($)</label>
            <input type="number" className="input" {...form.register('minimumProjectValue', { valueAsNumber: true })} />
          </div>
          <div>
            <label className="label">Preferred project types</label>
            <input className="input" placeholder="retainer, one-off, hybrid" {...form.register('preferredProjectTypes')} />
          </div>
        </div>
        <div className="flex justify-end">
          <button type="submit" className="btn btn-primary" disabled={update.isPending}>
            {update.isPending ? 'Saving…' : 'Save profile'}
          </button>
        </div>
      </form>

      <div className="card">
        <div className="card-body">
          <h2 className="text-sm font-semibold text-slate-900">Service offerings</h2>
          <ul className="mt-3 divide-y divide-slate-100">
            {data.serviceOfferings.length === 0 && <li className="text-sm text-slate-500 py-2">Add services you sell to power scoring and outreach.</li>}
            {data.serviceOfferings.map((s) => (
              <li key={s.id} className="py-2 flex items-center justify-between">
                <div>
                  <div className="font-medium text-slate-900">{s.name}</div>
                  {s.description && <div className="text-xs text-slate-500">{s.description}</div>}
                </div>
                <div className="text-sm text-slate-600">
                  {s.typicalProjectValue ? `$${s.typicalProjectValue.toLocaleString()}` : 'No price set'}
                </div>
              </li>
            ))}
          </ul>
          <div className="mt-4 flex gap-2">
            <input
              className="input"
              placeholder="e.g. Local SEO"
              value={newService.name}
              onChange={(e) => setNewService((s) => ({ ...s, name: e.target.value }))}
            />
            <input
              className="input max-w-[160px]"
              type="number"
              placeholder="Typical value"
              value={newService.typicalProjectValue}
              onChange={(e) => setNewService((s) => ({ ...s, typicalProjectValue: e.target.value }))}
            />
            <button
              className="btn btn-primary"
              type="button"
              disabled={!newService.name || addService.isPending}
              onClick={() => addService.mutate({
                name: newService.name,
                typicalProjectValue: newService.typicalProjectValue ? Number(newService.typicalProjectValue) : undefined
              })}
            >
              Add
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
