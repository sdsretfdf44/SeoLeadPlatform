import { api } from './api';
import type { Contact, UpsertContactRequest } from '../types/api';

export const contactsApi = {
  create: (prospectId: string, body: UpsertContactRequest) =>
    api.post<Contact>(`/api/prospects/${prospectId}/contacts`, body).then((r) => r.data),
  update: (id: string, body: UpsertContactRequest) =>
    api.put<Contact>(`/api/contacts/${id}`, body).then((r) => r.data),
  remove: (id: string) => api.delete(`/api/contacts/${id}`)
};
