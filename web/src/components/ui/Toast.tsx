import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from 'react';
import { AlertTriangle, CheckCircle2, Info, X } from 'lucide-react';

type ToastKind = 'success' | 'error' | 'info';

interface ToastMessage {
  id: number;
  kind: ToastKind;
  text: string;
}

interface ToastContextValue {
  push: (kind: ToastKind, text: string) => void;
  success: (text: string) => void;
  error: (text: string) => void;
  info: (text: string) => void;
}

const ToastContext = createContext<ToastContextValue | undefined>(undefined);

export function ToastProvider({ children }: { children: ReactNode }) {
  const [items, setItems] = useState<ToastMessage[]>([]);

  const push = useCallback((kind: ToastKind, text: string) => {
    const id = Date.now() + Math.random();
    setItems((prev) => [...prev, { id, kind, text }]);
    setTimeout(() => setItems((prev) => prev.filter((i) => i.id !== id)), 4500);
  }, []);

  const value = useMemo<ToastContextValue>(
    () => ({
      push,
      success: (t) => push('success', t),
      error: (t) => push('error', t),
      info: (t) => push('info', t)
    }),
    [push]
  );

  return (
    <ToastContext.Provider value={value}>
      {children}
      <div className="pointer-events-none fixed bottom-4 right-4 z-[100] flex flex-col gap-2 w-full max-w-sm">
        {items.map((item) => (
          <div
            key={item.id}
            className={`pointer-events-auto flex items-start gap-3 rounded-xl border p-3 shadow-lg bg-white transition ${
              item.kind === 'success'
                ? 'border-emerald-200'
                : item.kind === 'error'
                ? 'border-rose-200'
                : 'border-slate-200'
            }`}
          >
            <div className="pt-0.5">
              {item.kind === 'success' && <CheckCircle2 size={18} className="text-emerald-600" />}
              {item.kind === 'error' && <AlertTriangle size={18} className="text-rose-600" />}
              {item.kind === 'info' && <Info size={18} className="text-slate-500" />}
            </div>
            <div className="text-sm text-slate-800 flex-1">{item.text}</div>
            <button
              type="button"
              className="text-slate-400 hover:text-slate-600"
              onClick={() => setItems((prev) => prev.filter((i) => i.id !== item.id))}
            >
              <X size={16} />
            </button>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}

export function useToast() {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error('useToast must be used within ToastProvider');
  return ctx;
}
