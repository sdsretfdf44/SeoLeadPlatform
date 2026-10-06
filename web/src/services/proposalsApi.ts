import { api } from './api';
import type { Proposal, UpdateProposalRequest } from '../types/api';

export const proposalsApi = {
  generate: (prospectId: string) =>
    api.post<Proposal>(`/api/proposals/generate/${prospectId}`).then((r) => r.data),
  listForProspect: (prospectId: string) =>
    api.get<Proposal[]>(`/api/proposals/prospect/${prospectId}`).then((r) => r.data),
  update: (id: string, body: UpdateProposalRequest) =>
    api.put<Proposal>(`/api/proposals/${id}`, body).then((r) => r.data),
  markSent: (id: string) =>
    api.post<Proposal>(`/api/proposals/${id}/send`).then((r) => r.data),
  getPublic: (token: string) =>
    api.get<Proposal>(`/api/proposals/public/${token}`).then((r) => r.data)
};
