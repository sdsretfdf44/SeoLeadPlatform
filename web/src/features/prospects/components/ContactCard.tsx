import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { ExternalLink, Mail, MapPin, Phone, Star, Trash2, User } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { contactsApi } from '../../../services/contactsApi';
import type { Contact, UpsertContactRequest } from '../../../types/api';
import { useToast } from '../../../components/ui/Toast';
import { Modal } from '../../../components/ui/Modal';
import { apiErrorMessage } from '../../../services/api';

interface Props {
  prospectId: string;
  contacts: Contact[];
}

export function ContactsCard({ prospectId, contacts }: Props) {
  const queryClient = useQueryClient();
  const toast = useToast();
  const [editing, setEditing] = useState<Contact | null>(null);
  const [creating, setCreating] = useState(false);

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['prospect', prospectId] });

  const removeMutation = useMutation({
    mutationFn: (id: string) => contactsApi.remove(id),
    onSuccess: () => {
      toast.success('Contact removed.');
      invalidate();
    },
    onError: (err) => toast.error(apiErrorMessage(err))
  });

  return (
    <div className="card">
      <div className="card-header">
        <div>
          <h2 className="section-title">Decision makers</h2>
          <p className="text-xs text-slate-500 mt-1">Who to reach at this company.</p>
        </div>
        <button type="button" className="btn btn-secondary btn-sm" onClick={() => setCreating(true)}>Add contact</button>
      </div>
      <div className="card-body pt-2">
        {contacts.length === 0 ? (
          <div className="text-center py-6">
            <div className="mx-auto h-10 w-10 rounded-full bg-slate-100 flex items-center justify-center text-slate-400">
              <User size={20} />
            </div>
            <p className="text-sm text-slate-500 mt-2">No contacts yet. Add a decision maker to enable personalized outreach.</p>
          </div>
        ) : (
          <ul className="divide-y divide-slate-100">
            {contacts.map((contact) => (
              <li key={contact.id} className="py-3 flex items-start justify-between gap-3">
                <div className="min-w-0 flex-1">
                  <div className="flex items-center gap-2">
                    <span className="font-medium text-slate-900">{contact.fullName}</span>
                    {contact.isDecisionMaker && (
                      <span className="badge bg-amber-100 text-amber-800 inline-flex items-center gap-1"><Star size={10} /> Decision maker</span>
                    )}
                  </div>
                  {contact.jobTitle && <div className="text-xs text-slate-500 mt-0.5">{contact.jobTitle}</div>}
                  <div className="flex flex-wrap gap-x-3 gap-y-1 mt-2 text-xs text-slate-600">
                    {contact.email && (
                      <a href={`mailto:${contact.email}`} className="inline-flex items-center gap-1 hover:text-brand-700">
                        <Mail size={12} /> {contact.email}
                      </a>
                    )}
                    {contact.phoneNumber && (
                      <span className="inline-flex items-center gap-1">
                        <Phone size={12} /> {contact.phoneNumber}
                      </span>
                    )}
                    {contact.linkedInUrl && (
                      <a href={contact.linkedInUrl} target="_blank" rel="noreferrer" className="inline-flex items-center gap-1 hover:text-brand-700">
                        <ExternalLink size={12} /> LinkedIn
                      </a>
                    )}
                  </div>
                </div>
                <div className="flex gap-1">
                  <button type="button" className="btn btn-ghost btn-sm" onClick={() => setEditing(contact)}>Edit</button>
                  <button
                    type="button"
                    className="btn btn-ghost btn-sm text-rose-600"
                    onClick={() => {
                      if (confirm(`Remove ${contact.fullName}?`)) removeMutation.mutate(contact.id);
                    }}
                  >
                    <Trash2 size={14} />
                  </button>
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>

      <ContactModal
        open={creating}
        onClose={() => setCreating(false)}
        onSuccess={() => {
          setCreating(false);
          invalidate();
        }}
        title="Add contact"
        prospectId={prospectId}
      />
      <ContactModal
        open={!!editing}
        onClose={() => setEditing(null)}
        onSuccess={() => {
          setEditing(null);
          invalidate();
        }}
        title="Edit contact"
        prospectId={prospectId}
        contact={editing ?? undefined}
      />
    </div>
  );
}

function ContactModal({
  open,
  onClose,
  onSuccess,
  title,
  prospectId,
  contact
}: {
  open: boolean;
  onClose: () => void;
  onSuccess: () => void;
  title: string;
  prospectId: string;
  contact?: Contact;
}) {
  const toast = useToast();
  const { register, handleSubmit, reset } = useForm<UpsertContactRequest>({
    values: {
      fullName: contact?.fullName ?? '',
      jobTitle: contact?.jobTitle ?? '',
      email: contact?.email ?? '',
      phoneNumber: contact?.phoneNumber ?? '',
      linkedInUrl: contact?.linkedInUrl ?? '',
      isDecisionMaker: contact?.isDecisionMaker ?? false
    }
  });

  const mutation = useMutation({
    mutationFn: (values: UpsertContactRequest) =>
      contact ? contactsApi.update(contact.id, values) : contactsApi.create(prospectId, values),
    onSuccess: () => {
      toast.success(contact ? 'Contact updated.' : 'Contact added.');
      reset();
      onSuccess();
    },
    onError: (err) => toast.error(apiErrorMessage(err))
  });

  return (
    <Modal open={open} onClose={onClose} title={title}>
      <form
        className="space-y-3"
        onSubmit={handleSubmit((values) => mutation.mutate(values))}
      >
        <div>
          <label className="label">Full name</label>
          <input className="input" required {...register('fullName')} />
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
          <div>
            <label className="label">Job title</label>
            <input className="input" {...register('jobTitle')} placeholder="e.g. Owner / Head of marketing" />
          </div>
          <div>
            <label className="label">Email</label>
            <input className="input" type="email" {...register('email')} />
          </div>
          <div>
            <label className="label">Phone</label>
            <input className="input" {...register('phoneNumber')} />
          </div>
          <div>
            <label className="label">LinkedIn URL</label>
            <input className="input" {...register('linkedInUrl')} placeholder="https://linkedin.com/in/..." />
          </div>
        </div>
        <label className="inline-flex items-center gap-2 text-sm text-slate-700">
          <input type="checkbox" className="rounded border-slate-300 text-brand-600 focus:ring-brand-500" {...register('isDecisionMaker')} />
          Decision maker
        </label>
        <div className="flex justify-end gap-2 pt-2">
          <button type="button" className="btn btn-secondary" onClick={onClose}>Cancel</button>
          <button type="submit" className="btn btn-primary" disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving…' : 'Save'}
          </button>
        </div>
      </form>
    </Modal>
  );
}

export { MapPin };
