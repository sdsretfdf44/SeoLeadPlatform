import { createBrowserRouter, Navigate } from 'react-router-dom';
import { AppShell } from '../components/layout/AppShell';
import { ProtectedRoute } from './ProtectedRoute';
import { LoginPage } from '../features/auth/LoginPage';
import { RegisterPage } from '../features/auth/RegisterPage';
import { DashboardPage } from '../features/dashboard/DashboardPage';
import { ProspectListPage } from '../features/prospects/ProspectListPage';
import { ProspectDetailPage } from '../features/prospects/ProspectDetailPage';
import { PublicReportPage } from '../features/reports/PublicReportPage';
import { PublicProposalPage } from '../features/reports/PublicProposalPage';
import { AgencyPage } from '../features/agency/AgencyPage';
import { PipelinePage } from '../features/pipeline/PipelinePage';

export const router = createBrowserRouter([
  { path: '/login', element: <LoginPage /> },
  { path: '/register', element: <RegisterPage /> },
  { path: '/reports/:token', element: <PublicReportPage /> },
  { path: '/proposals/:token', element: <PublicProposalPage /> },
  {
    element: (
      <ProtectedRoute>
        <AppShell />
      </ProtectedRoute>
    ),
    children: [
      { index: true, element: <DashboardPage /> },
      { path: 'prospects', element: <ProspectListPage /> },
      { path: 'prospects/:id', element: <ProspectDetailPage /> },
      { path: 'pipeline', element: <PipelinePage /> },
      { path: 'agency', element: <AgencyPage /> },
      { path: 'settings', element: <AgencyPage /> },
      { path: '*', element: <Navigate to="/" replace /> }
    ]
  }
]);
