export function PageSpinner() {
  return (
    <div className="flex items-center justify-center py-16">
      <div className="h-8 w-8 animate-spin rounded-full border-2 border-slate-300 border-t-brand-600" />
    </div>
  );
}

export function InlineSpinner() {
  return <div className="h-4 w-4 animate-spin rounded-full border-2 border-slate-300 border-t-brand-600 inline-block" />;
}

export function Skeleton({ className = '' }: { className?: string }) {
  return <div className={`animate-pulse bg-slate-200 rounded ${className}`} />;
}
