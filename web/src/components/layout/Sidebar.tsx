import { NavLink } from 'react-router-dom';
import { Building2, LayoutDashboard, LogOut, Settings, Sparkles, Target, Trophy } from 'lucide-react';
import { useAuth } from '../../app/AuthContext';

const nav = [
  { to: '/', label: 'Dashboard', icon: LayoutDashboard, end: true },
  { to: '/prospects', label: 'Prospects', icon: Target },
  { to: '/pipeline', label: 'Pipeline', icon: Trophy },
  { to: '/agency', label: 'Agency profile', icon: Building2 },
  { to: '/settings', label: 'Settings', icon: Settings }
];

export function Sidebar() {
  const { user, logout } = useAuth();
  return (
    <aside className="hidden md:flex md:w-64 md:flex-col md:fixed md:inset-y-0 bg-slate-950 text-slate-100">
      <div className="px-6 py-5 border-b border-slate-800/60">
        <div className="flex items-center gap-2.5">
          <div className="h-9 w-9 rounded-lg bg-gradient-to-br from-brand-500 to-brand-700 flex items-center justify-center text-white shadow-lg shadow-brand-900/50">
            <Sparkles size={18} />
          </div>
          <div className="min-w-0">
            <div className="text-sm font-semibold text-white leading-tight">SEO Lead Platform</div>
            <div className="text-[11px] text-slate-400 truncate">{user?.agencyName}</div>
          </div>
        </div>
      </div>
      <nav className="flex-1 p-3 space-y-1">
        {nav.map((item) => {
          const Icon = item.icon;
          return (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.end}
              className={({ isActive }) =>
                `flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium transition-colors ${
                  isActive
                    ? 'bg-brand-600/20 text-white'
                    : 'text-slate-400 hover:bg-slate-800/50 hover:text-white'
                }`
              }
            >
              <Icon size={18} />
              {item.label}
            </NavLink>
          );
        })}
      </nav>
      <div className="p-4 border-t border-slate-800/60">
        <div className="flex items-center gap-3">
          <div className="h-8 w-8 rounded-full bg-gradient-to-br from-brand-400 to-brand-600 flex items-center justify-center text-xs font-semibold text-white">
            {user?.fullName?.charAt(0).toUpperCase()}
          </div>
          <div className="flex-1 min-w-0">
            <div className="text-sm font-medium text-white truncate">{user?.fullName}</div>
            <div className="text-[11px] text-slate-400 truncate">{user?.email}</div>
          </div>
        </div>
        <button
          type="button"
          onClick={logout}
          className="mt-3 flex items-center gap-2 text-xs text-slate-400 hover:text-white"
        >
          <LogOut size={14} />
          Sign out
        </button>
      </div>
    </aside>
  );
}
