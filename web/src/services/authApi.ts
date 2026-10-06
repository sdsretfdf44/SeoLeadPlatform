import { api } from './api';
import type { AuthResponse, UserProfile } from '../types/api';

export const authApi = {
  register: (body: { agencyName: string; fullName: string; email: string; password: string }) =>
    api.post<AuthResponse>('/api/auth/register', body).then((r) => r.data),
  login: (body: { email: string; password: string }) =>
    api.post<AuthResponse>('/api/auth/login', body).then((r) => r.data),
  me: () => api.get<UserProfile>('/api/auth/me').then((r) => r.data)
};
