import { api } from './api';
import type { PublicReport } from '../types/api';

export const reportsApi = {
  createToken: (auditId: string) =>
    api.post<{ token: string; expiresAt: string; publicPath: string }>(`/api/reports/audits/${auditId}/token`).then((r) => r.data),
  getPublic: (token: string) =>
    api.get<PublicReport>(`/api/reports/public/${token}`).then((r) => r.data)
};
