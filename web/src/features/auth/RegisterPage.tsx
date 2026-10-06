import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../app/AuthContext';
import { useState } from 'react';
import { apiErrorMessage } from '../../services/api';
import { Building2, Mail, Lock, Sparkles, User } from 'lucide-react';

const schema = z.object({
  agencyName: z.string().min(2, 'Enter your agency name'),
  fullName: z.string().min(2, 'Enter your name'),
  email: z.string().email('Enter a valid email'),
  password: z.string().min(8, 'At least 8 characters')
});
type Values = z.infer<typeof schema>;

export function RegisterPage() {
  const { register: registerUser } = useAuth();
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const { register, handleSubmit, formState } = useForm<Values>({ resolver: zodResolver(schema) });

  const onSubmit = async (values: Values) => {
    setError(null);
    try {
      await registerUser(values);
      navigate('/');
    } catch (err) {
      setError(apiErrorMessage(err));
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center px-4 py-10 bg-slate-50 relative">
      <div className="absolute inset-0 opacity-40 pointer-events-none bg-grid-fade" aria-hidden />
      <div className="w-full max-w-md relative">
        <div className="text-center mb-6">
          <div className="inline-flex h-11 w-11 rounded-xl bg-gradient-to-br from-brand-500 to-brand-700 items-center justify-center text-white shadow-lg shadow-brand-700/25">
            <Sparkles size={22} />
          </div>
          <h1 className="text-2xl font-semibold text-slate-900 mt-4 tracking-tight">Create your workspace</h1>
          <p className="text-sm text-slate-500 mt-1">Find, qualify and win SEO clients in one place.</p>
        </div>
        <form className="card card-body space-y-4" onSubmit={handleSubmit(onSubmit)}>
          {error && (
            <div className="text-sm text-rose-700 bg-rose-50 rounded-lg p-3 border border-rose-100">
              {error}
            </div>
          )}
          <IconInput id="agency" label="Agency name" placeholder="Acme SEO" Icon={Building2}
            reg={register('agencyName')} error={formState.errors.agencyName?.message} />
          <IconInput id="fullName" label="Your name" placeholder="Jane Doe" Icon={User}
            reg={register('fullName')} error={formState.errors.fullName?.message} />
          <IconInput id="email" type="email" label="Work email" placeholder="jane@acmeseo.com" Icon={Mail}
            reg={register('email')} error={formState.errors.email?.message} />
          <IconInput id="password" type="password" label="Password" placeholder="At least 8 characters" Icon={Lock}
            reg={register('password')} error={formState.errors.password?.message} />
          <button type="submit" className="btn btn-primary w-full justify-center" disabled={formState.isSubmitting}>
            {formState.isSubmitting ? 'Creating account…' : 'Create account'}
          </button>
        </form>
        <p className="text-center text-sm text-slate-500 mt-6">
          Already have an account? <Link to="/login" className="text-brand-700 font-semibold hover:text-brand-800">Sign in</Link>
        </p>
      </div>
    </div>
  );
}

interface IconInputProps {
  id: string;
  label: string;
  placeholder?: string;
  type?: string;
  Icon: React.ComponentType<{ size?: number; className?: string }>;
  reg: ReturnType<ReturnType<typeof useForm<Values>>['register']>;
  error?: string;
}

function IconInput({ id, label, placeholder, type = 'text', Icon, reg, error }: IconInputProps) {
  return (
    <div>
      <label className="label" htmlFor={id}>{label}</label>
      <div className="relative">
        <Icon size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
        <input id={id} type={type} className="input pl-9" placeholder={placeholder} {...reg} />
      </div>
      {error && <p className="mt-1 text-xs text-rose-600">{error}</p>}
    </div>
  );
}
