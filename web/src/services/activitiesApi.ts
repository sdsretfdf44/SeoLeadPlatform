import { api } from './api';
import type { Activity } from '../types/api';

export const activitiesApi = {
  listForProspect: (prospectId: string) =>
    api.get<Activity[]>(`/api/prospects/${prospectId}/activities`).then((r) => r.data)
};
