/**
 * Analytics Feature Routes
 * Lazy-loaded routes for reports, analytics, and insights
 */

import { Routes } from '@angular/router';
import { authGuard, authorizationGuard } from '../../core';

export const ANALYTICS_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'workforce',
    pathMatch: 'full',
  },
  {
    path: 'reports',
    loadComponent: () =>
      import('./pages/reports/reports.component').then((m) => m.ReportsComponent),
    canActivate: [authGuard, authorizationGuard],
    data: {
      title: 'Reports',
      roles: ['hr_user', 'hr_director', 'leadership', 'admin'],
    },
  },
  {
    path: 'workforce',
    loadComponent: () =>
      import('./pages/workforce/workforce.component').then(
        (m) => m.WorkforceComponent
      ),
    canActivate: [authGuard, authorizationGuard],
    data: {
      title: 'Workforce Analytics',
      roles: ['leadership', 'admin'],
    },
  },
  {
    path: 'turnover',
    loadComponent: () =>
      import('./pages/turnover/turnover.component').then(
        (m) => m.TurnoverComponent
      ),
    canActivate: [authGuard, authorizationGuard],
    data: {
      title: 'Turnover Analysis',
      roles: ['hr_director', 'admin'],
    },
  },
];
