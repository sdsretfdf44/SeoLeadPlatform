import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../app/AuthContext';
import { useState } from 'react';
import { apiErrorMessage } from '../../services/api';
import { Lock, Mail, Sparkles, Target, TrendingUp, Trophy } from 'lucide-react';

const schema = z.object({
  email: z.string().email('Enter a valid email'),
  password: z.string().min(1, 'Password required')
});
type Values = z.infer<typeof schema>;

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const { register, handleSubmit, formState } = useForm<Values>({ resolver: zodResolver(schema) });

  const onSubmit = async (values: Values) => {
    setError(null);
    try {
      await login(values.email, values.password);
      navigate('/');
    } catch (err) {
      setError(apiErrorMessage(err));
    }
  };

  return (
    <div className="min-h-screen grid lg:grid-cols-2">
      {/* Brand side */}
      <div className="hidden lg:flex flex-col justify-between p-12 relative overflow-hidden bg-gradient-to-br from-brand-700 via-brand-800 to-slate-900 text-white">
        <div className="absolute -top-24 -right-24 h-96 w-96 rounded-full bg-brand-500/20 blur-3xl" aria-hidden />
        <div className="absolute -bottom-32 -left-16 h-96 w-96 rounded-full bg-indigo-500/20 blur-3xl" aria-hidden />

        <div className="flex items-center gap-2.5 text-sm font-semibold relative">
          <div className="h-9 w-9 rounded-lg bg-white/10 backdrop-blur-sm flex items-center justify-center border border-white/10">
            <Sparkles size={18} />
          </div>
          SEO Lead Platform
        </div>

        <div className="max-w-md space-y-6 relative">
          <h2 className="text-4xl font-semibold tracking-tight leading-[1.1]">
            An AI-powered sales engine for SEO agencies.
          </h2>
          <p className="text-brand-100 leading-relaxed">
            Find businesses that need SEO, qualify them with explainable opportunity scores, and generate personalized outreach — all in one workflow.
          </p>
          <ul className="space-y-3">
            <Feature icon={Target} text="Prospect discovery and SEO audits in one click" />
            <Feature icon={TrendingUp} text="Explainable opportunity scoring with weighted factors" />
            <Feature icon={Trophy} text="From first-touch to proposal in a single workspace" />
          </ul>
        </div>

        <div className="text-xs text-brand-200/70 relative">© {new Date().getFullYear()} SEO Lead Platform.</div>
      </div>

      {/* Form side */}
      <div className="flex items-center justify-center px-6 py-16 bg-slate-50 relative">
        <div className="absolute inset-0 opacity-40 pointer-events-none bg-grid-fade lg:hidden" aria-hidden />
        <div className="w-full max-w-sm relative">
          <div className="text-center mb-7">
            <div className="lg:hidden inline-flex h-11 w-11 rounded-xl bg-gradient-to-br from-brand-500 to-brand-700 items-center justify-center text-white shadow-lg shadow-brand-700/25 mb-4">
              <Sparkles size={22} />
            </div>
            <h1 className="text-2xl font-semibold text-slate-900 tracking-tight">Welcome back</h1>
            <p className="text-sm text-slate-500 mt-1">Sign in to your agency workspace.</p>
          </div>
          <form className="card card-body space-y-4" onSubmit={handleSubmit(onSubmit)}>
            {error && (
              <div className="text-sm text-rose-700 bg-rose-50 rounded-lg p-3 border border-rose-100">
                {error}
              </div>
            )}
            <div>
              <label className="label" htmlFor="email">Email</label>
              <div className="relative">
                <Mail size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
                <input id="email" type="email" className="input pl-9" placeholder="you@agency.com" {...register('email')} />
              </div>
              {formState.errors.email && <p className="mt-1 text-xs text-rose-600">{formState.errors.email.message}</p>}
            </div>
            <div>
              <label className="label" htmlFor="password">Password</label>
              <div className="relative">
                <Lock size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
                <input id="password" type="password" className="input pl-9" {...register('password')} />
              </div>
              {formState.errors.password && <p className="mt-1 text-xs text-rose-600">{formState.errors.password.message}</p>}
            </div>
            <button type="submit" className="btn btn-primary w-full justify-center" disabled={formState.isSubmitting}>
              {formState.isSubmitting ? 'Signing in…' : 'Sign in'}
            </button>
          </form>
          <p className="text-center text-sm text-slate-500 mt-6">
            New here? <Link to="/register" className="text-brand-700 font-semibold hover:text-brand-800">Create an account</Link>
          </p>
        </div>
      </div>
    </div>
  );
}

function Feature({ icon: Icon, text }: { icon: typeof Sparkles; text: string }) {
  return (
    <li className="flex items-start gap-3">
      <div className="h-7 w-7 rounded-lg bg-white/10 backdrop-blur-sm border border-white/10 flex items-center justify-center shrink-0">
        <Icon size={14} />
      </div>
      <span className="text-sm text-brand-50">{text}</span>
    </li>
  );
}
