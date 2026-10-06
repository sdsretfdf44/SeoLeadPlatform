import { api } from './api';
import type { Lead, LeadPriority, LeadStatus } from '../types/api';

export interface UpdateLeadRequest {
  status: LeadStatus;
  priority: LeadPriority;
  estimatedProjectValue?: number | null;
  estimatedMonthlyRetainer?: number | null;
  probability: number;
  nextFollowUpAt?: string | null;
  notes?: string | null;
}

export const leadsApi = {
  updateStatus: (id: string, status: LeadStatus) =>
    api.patch<Lead>(`/api/leads/${id}/status`, { status }).then((r) => r.data),
  update: (id: string, body: UpdateLeadRequest) =>
    api.put<Lead>(`/api/leads/${id}`, body).then((r) => r.data)
};
