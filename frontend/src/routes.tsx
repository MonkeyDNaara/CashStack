import type { RouteObject } from 'react-router';
import { AppLayout } from './components/layout/AppLayout';
import { CategoriesPage } from './pages/CategoriesPage';
import { NotFoundPage } from './pages/NotFoundPage';
import { PageLoading } from './pages/PageLoading';
import { RouteErrorPage } from './pages/RouteErrorPage';
import { TransactionsPage } from './pages/TransactionsPage';

// Shared by the browser router (main.tsx) and the memory router in tests
export const routes: RouteObject[] = [
  {
    path: '/',
    element: <AppLayout />,
    errorElement: <RouteErrorPage />,
    children: [
      {
        index: true,
        // Code splitting: Recharts (~400 kB) is only loaded when the dashboard is opened
        lazy: () => import('./pages/DashboardPage').then((m) => ({ Component: m.DashboardPage })),
        HydrateFallback: PageLoading,
      },
      { path: 'transactions', element: <TransactionsPage /> },
      { path: 'categories', element: <CategoriesPage /> },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
];
