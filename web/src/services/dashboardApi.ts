import { api } from './api';
import type { DashboardSummary } from '../types/api';

export const dashboardApi = {
  summary: () => api.get<DashboardSummary>('/api/dashboard/summary').then((r) => r.data)
};
