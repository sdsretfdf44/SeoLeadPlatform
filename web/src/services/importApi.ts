import { api } from './api';
import type { ImportProspectRow, ImportProspectsResult } from '../types/api';

export const importApi = {
  import: (rows: ImportProspectRow[]) =>
    api.post<ImportProspectsResult>('/api/prospects/import', { rows }).then((r) => r.data)
};
