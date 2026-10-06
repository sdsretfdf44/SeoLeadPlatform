import { api } from './api';
import type { Paged, ProspectDetail, ProspectListItem, SeoAuditDetail, Lead, OutreachMessage, LeadStatus } from '../types/api';

export interface ProspectSearchQuery {
  search?: string;
  industry?: string;
  country?: string;
  city?: string;
  leadStatus?: LeadStatus;
  minOpportunityScore?: number;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
}

export const prospectsApi = {
  search: (query: ProspectSearchQuery) =>
    api.get<Paged<ProspectListItem>>('/api/prospects', { params: query }).then((r) => r.data),
  get: (id: string) => api.get<ProspectDetail>(`/api/prospects/${id}`).then((r) => r.data),
  create: (body: Partial<ProspectDetail> & { companyName: string; websiteUrl: string }) =>
    api.post<ProspectDetail>('/api/prospects', body).then((r) => r.data),
  remove: (id: string) => api.delete(`/api/prospects/${id}`).then((r) => r.status),
  startAudit: (id: string) => api.post<SeoAuditDetail>(`/api/prospects/${id}/audit`).then((r) => r.data),
  getLatestAudit: (id: string) => api.get<SeoAuditDetail>(`/api/prospects/${id}/audit`).then((r) => r.data),
  qualify: (id: string) => api.post<Lead>(`/api/prospects/${id}/qualify`).then((r) => r.data),
  generateOutreach: (id: string, body: { contactId?: string; type?: string } = {}) =>
    api.post<OutreachMessage>(`/api/prospects/${id}/outreach`, body).then((r) => r.data),
  listOutreach: (id: string) => api.get<OutreachMessage[]>(`/api/prospects/${id}/outreach`).then((r) => r.data)
};
