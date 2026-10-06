import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { Modal } from '../../../components/ui/Modal';
import { importApi } from '../../../services/importApi';
import { useToast } from '../../../components/ui/Toast';
import type { ImportProspectRow } from '../../../types/api';

export function ImportModal({ open, onClose }: { open: boolean; onClose: () => void }) {
  const toast = useToast();
  const queryClient = useQueryClient();
  const [text, setText] = useState('');

  const importMutation = useMutation({
    mutationFn: (rows: ImportProspectRow[]) => importApi.import(rows),
    onSuccess: (result) => {
      queryClient.invalidateQueries({ queryKey: ['prospects'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
      toast.success(`Imported ${result.imported} · skipped ${result.skipped}${result.errors.length ? ` · ${result.errors.length} errors` : ''}`);
      if (result.errors.length === 0) {
        setText('');
        onClose();
      }
    },
    onError: () => toast.error('Import failed.')
  });

  const parse = (): ImportProspectRow[] => {
    const lines = text.split(/\r?\n/).map((l) => l.trim()).filter(Boolean);
    if (lines.length === 0) return [];
    const first = lines[0].toLowerCase();
    const hasHeader = first.includes('company') || first.includes('website') || first.includes('name');
    const data = hasHeader ? lines.slice(1) : lines;
    return data.map((line) => {
      const cols = line.split(',').map((c) => c.trim());
      const [companyName, websiteUrl, industry, country, state, city, primaryEmail, phoneNumber] = cols;
      return { companyName, websiteUrl, industry, country, state, city, primaryEmail, phoneNumber };
    });
  };

  return (
    <Modal
      open={open}
      onClose={onClose}
      title="Bulk import prospects"
      description="Paste one row per line: companyName, websiteUrl, industry, country, state, city, email, phone. Header row is optional."
      size="lg"
    >
      <textarea
        className="input min-h-[220px] font-mono text-xs"
        placeholder="Acme Dental,https://acmedental.com,Dental,US,TX,Austin,hello@acmedental.com,555-0100"
        value={text}
        onChange={(e) => setText(e.target.value)}
      />
      <div className="flex items-center justify-between mt-3">
        <div className="text-xs text-slate-500">Max 500 rows per import. Duplicates by website URL are skipped.</div>
        <div className="flex gap-2">
          <button type="button" className="btn btn-secondary" onClick={onClose}>Cancel</button>
          <button
            type="button"
            className="btn btn-primary"
            disabled={importMutation.isPending}
            onClick={() => {
              const rows = parse();
              if (rows.length === 0) {
                toast.error('Nothing to import.');
                return;
              }
              importMutation.mutate(rows);
            }}
          >
            {importMutation.isPending ? 'Importing…' : 'Import'}
          </button>
        </div>
      </div>
    </Modal>
  );
}
