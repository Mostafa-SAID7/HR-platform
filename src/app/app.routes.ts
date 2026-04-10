import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { authorizationGuard } from './guards/authorization.guard';

/**
 * Application Routes
 *
 * Configured with:
 * - Lazy loading for all feature and layout components
 * - Route guards for authentication and authorization
 * - Smart preloading strategy
 *
 * Requirements: 1.3, 6.2, 11.4
 */
export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./layouts/dashboard-layout/dashboard-layout.component').then(
        (m) => m.DashboardLayoutComponent,
      ),
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./pages/dashboard/dashboard.component').then((m) => m.DashboardComponent),
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./pages/dashboard/dashboard.component').then((m) => m.DashboardComponent),
      },
      {
        path: 'employees',
        loadComponent: () =>
          import('./pages/employees/employees.component').then((m) => m.EmployeesComponent),
        canActivate: [authGuard, authorizationGuard],
        data: { roles: ['hr_user', 'manager', 'admin'] },
      },
      {
        path: 'performance',
        loadComponent: () =>
          import('./pages/performance/performance.component').then((m) => m.PerformanceComponent),
        canActivate: [authGuard, authorizationGuard],
        data: { roles: ['hr_user', 'manager', 'admin'] },
      },
      {
        path: 'workforce',
        loadComponent: () =>
          import('./pages/workforce/workforce.component').then((m) => m.WorkforceComponent),
        canActivate: [authGuard, authorizationGuard],
        data: { roles: ['leadership', 'admin'] },
      },
      {
        path: 'turnover',
        loadComponent: () =>
          import('./pages/turnover/turnover.component').then((m) => m.TurnoverComponent),
        canActivate: [authGuard, authorizationGuard],
        data: { roles: ['hr_director', 'admin'] },
      },
      {
        path: 'hiring',
        loadComponent: () =>
          import('./pages/hiring/hiring.component').then(
            (m) => m.HiringComponent,
          ),
        canActivate: [authGuard, authorizationGuard],
        data: { roles: ['talent_acquisition', 'admin'] },
      },
      {
        path: 'reports',
        loadComponent: () =>
          import('./pages/reports/reports.component').then((m) => m.ReportsComponent),
        canActivate: [authGuard, authorizationGuard],
        data: { roles: ['hr_user', 'hr_director', 'leadership', 'admin'] },
      },
    ],
  },
  {
    path: 'unauthorized',
    loadComponent: () =>
      import('./pages/unauthorized/unauthorized.component').then((m) => m.UnauthorizedComponent),
  },
  {
    path: '**',
    loadComponent: () =>
      import('./pages/not-found/not-found.component').then((m) => m.NotFoundComponent),
  },
];
