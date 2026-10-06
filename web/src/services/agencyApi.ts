import { api } from './api';
import type { Agency, ServiceOffering } from '../types/api';

export const agencyApi = {
  get: () => api.get<Agency>('/api/agency').then((r) => r.data),
  update: (body: Partial<Agency>) => api.put<Agency>('/api/agency', body).then((r) => r.data),
  addService: (body: Partial<ServiceOffering>) =>
    api.post<ServiceOffering>('/api/agency/services', body).then((r) => r.data)
};
