/**
 * Recruitment Feature Routes
 * Lazy-loaded routes for recruitment and hiring
 */

import { Routes } from '@angular/router';
import { authGuard, authorizationGuard } from '../../core';

export const RECRUITMENT_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'hiring',
    pathMatch: 'full',
  },
  {
    path: 'hiring',
    loadComponent: () =>
      import('./pages/hiring/hiring.component').then((m) => m.HiringComponent),
    canActivate: [authGuard, authorizationGuard],
    data: {
      title: 'Hiring',
      roles: ['talent_acquisition', 'admin'],
    },
  },
];
