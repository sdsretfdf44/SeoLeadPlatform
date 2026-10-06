import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { leadsApi, type UpdateLeadRequest } from '../../../services/leadsApi';
import type { Lead, LeadPriority, LeadStatus } from '../../../types/api';
import { useToast } from '../../../components/ui/Toast';
import { apiErrorMessage } from '../../../services/api';

const STATUSES: LeadStatus[] = ['New', 'Qualified', 'Contacted', 'Replied', 'MeetingScheduled', 'ProposalSent', 'Negotiation', 'Won', 'Lost', 'Disqualified'];
const PRIORITIES: LeadPriority[] = ['Low', 'Medium', 'High', 'Critical'];

export function LeadDetailsCard({ prospectId, lead }: { prospectId: string; lead: Lead | null }) {
  const queryClient = useQueryClient();
  const toast = useToast();

  const { register, handleSubmit, reset } = useForm<UpdateLeadRequest>({
    defaultValues: buildDefaults(lead)
  });

  useEffect(() => reset(buildDefaults(lead)), [lead, reset]);

  const mutation = useMutation({
    mutationFn: (values: UpdateLeadRequest) => leadsApi.update(lead!.id, values),
    onSuccess: () => {
      toast.success('Lead updated.');
      queryClient.invalidateQueries({ queryKey: ['prospect', prospectId] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
    },
    onError: (err) => toast.error(apiErrorMessage(err))
  });

  if (!lead) {
    return (
      <div className="card">
        <div className="card-body">
          <h2 className="section-title">Lead details</h2>
          <p className="text-sm text-slate-500 mt-2">Qualify this prospect as a lead to track deal value, follow-ups, and notes.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="card">
      <div className="card-header">
        <h2 className="section-title">Lead details</h2>
        <span className="text-xs text-slate-500">Opportunity score {lead.opportunityScore}/100</span>
      </div>
      <form
        className="card-body pt-2 space-y-3"
        onSubmit={handleSubmit((values) =>
          mutation.mutate({
            ...values,
            probability: Number(values.probability),
            estimatedProjectValue: values.estimatedProjectValue ? Number(values.estimatedProjectValue) : null,
            estimatedMonthlyRetainer: values.estimatedMonthlyRetainer ? Number(values.estimatedMonthlyRetainer) : null,
            nextFollowUpAt: values.nextFollowUpAt ? new Date(values.nextFollowUpAt).toISOString() : null
          })
        )}
      >
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="label">Status</label>
            <select className="input" {...register('status')}>
              {STATUSES.map((s) => <option key={s} value={s}>{s}</option>)}
            </select>
          </div>
          <div>
            <label className="label">Priority</label>
            <select className="input" {...register('priority')}>
              {PRIORITIES.map((p) => <option key={p} value={p}>{p}</option>)}
            </select>
          </div>
          <div>
            <label className="label">Project value ($)</label>
            <input type="number" step="100" className="input" {...register('estimatedProjectValue')} />
          </div>
          <div>
            <label className="label">Monthly retainer ($)</label>
            <input type="number" step="100" className="input" {...register('estimatedMonthlyRetainer')} />
          </div>
          <div>
            <label className="label">Probability (%)</label>
            <input type="number" min="0" max="100" className="input" {...register('probability')} />
          </div>
          <div>
            <label className="label">Next follow-up</label>
            <input type="datetime-local" className="input" {...register('nextFollowUpAt')} />
          </div>
        </div>
        <div>
          <label className="label">Notes</label>
          <textarea className="input min-h-[80px]" placeholder="Conversation notes, objections, blockers…" {...register('notes')} />
        </div>
        <div className="flex justify-end">
          <button type="submit" className="btn btn-primary" disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving…' : 'Save lead'}
          </button>
        </div>
      </form>
    </div>
  );
}

function buildDefaults(lead: Lead | null): UpdateLeadRequest {
  return {
    status: lead?.status ?? 'New',
    priority: lead?.priority ?? 'Medium',
    estimatedProjectValue: lead?.estimatedProjectValue ?? null,
    estimatedMonthlyRetainer: lead?.estimatedMonthlyRetainer ?? null,
    probability: lead?.probability ?? 0,
    nextFollowUpAt: lead?.nextFollowUpAt ? lead.nextFollowUpAt.slice(0, 16) : null,
    notes: lead?.notes ?? ''
  };
}
