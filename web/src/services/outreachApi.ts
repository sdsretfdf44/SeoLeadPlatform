import { api } from './api';
import type { OutreachMessage } from '../types/api';

export const outreachApi = {
  update: (id: string, body: { subject: string; body: string }) =>
    api.put<OutreachMessage>(`/api/outreach/${id}`, body).then((r) => r.data),
  approve: (id: string) => api.post<OutreachMessage>(`/api/outreach/${id}/approve`).then((r) => r.data),
  markSent: (id: string) => api.post<OutreachMessage>(`/api/outreach/${id}/sent`).then((r) => r.data)
};
