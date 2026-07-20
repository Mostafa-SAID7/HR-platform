/**
 * Application Routes
 *
 * Feature-based architecture with:
 * - Lazy loading for all features
 * - Layout-based route organization
 * - Authentication and authorization guards
 * - Role-based access control
 * - Error page handling
 *
 * Route Structure:
 * - Main layout (authenticated routes)
 *   - Dashboard feature
 *   - Employees feature
 *   - Performance feature
 *   - Analytics feature (reports, workforce, turnover)
 *   - Recruitment feature
 *   - Attendance feature
 *   - Payroll feature
 *   - Admin feature
 *   - Settings feature
 * - Auth layout (login/signup)
 * - Error pages (unauthorized, not-found)
 */

import { Routes } from '@angular/router';
import { authGuard, authorizationGuard } from './core';
import { DashboardLayoutComponent } from './layouts/index';

// Feature route imports
import {
  DASHBOARD_ROUTES,
  EMPLOYEES_ROUTES,
  PERFORMANCE_ROUTES,
  RECRUITMENT_ROUTES,
  ANALYTICS_ROUTES,
  ATTENDANCE_ROUTES,
  PAYROLL_ROUTES,
  ADMIN_ROUTES,
  SETTINGS_ROUTES,
} from './features/index';

export const routes: Routes = [
  /**
   * Main Layout Routes (Authenticated)
   * All routes here require authentication and wrap with main layout
   * containing sidebar, topbar, and navigation
   */
  {
    path: '',
    loadComponent: () => Promise.resolve(DashboardLayoutComponent),
    canActivate: [authGuard],
    children: [
      // Dashboard - default route
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },

      // Dashboard Feature
      {
        path: 'dashboard',
        children: DASHBOARD_ROUTES,
      },

      // Employees Feature
      {
        path: 'employees',
        children: EMPLOYEES_ROUTES,
      },

      // Performance Feature
      {
        path: 'performance',
        children: PERFORMANCE_ROUTES,
      },

      // Analytics Feature (reports, workforce, turnover)
      {
        path: 'analytics',
        children: ANALYTICS_ROUTES,
      },

      // Recruitment Feature
      {
        path: 'recruitment',
        children: RECRUITMENT_ROUTES,
      },

      // Attendance Feature
      {
        path: 'attendance',
        children: ATTENDANCE_ROUTES,
      },

      // Payroll Feature
      {
        path: 'payroll',
        children: PAYROLL_ROUTES,
      },

      // Admin Feature
      {
        path: 'admin',
        children: ADMIN_ROUTES,
      },

      // Settings Feature
      {
        path: 'settings',
        children: SETTINGS_ROUTES,
      },
    ],
  },

  /**
   * Auth Layout Routes (Unauthenticated)
   * Routes for login, signup, password reset
   * Uses minimal auth layout without navigation
   */
  // TODO: Create auth layout and routes
  // {
  //   path: 'auth',
  //   loadComponent: () => import('./layouts/main-layout/auth-layout/auth-layout.component').then(m => m.AuthLayoutComponent),
  //   children: [
  //     // TODO: Add login, signup, password-reset routes here
  //     // Example:
  //     // {
  //     //   path: 'login',
  //     //   loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent)
  //     // },
  //     // {
  //     //   path: 'signup',
  //     //   loadComponent: () => import('./pages/signup/signup.component').then(m => m.SignupComponent)
  //     // }
  //   ],
  // },

  /**
   * Error Pages
   */
  {
    path: 'unauthorized',
    loadComponent: () =>
      import('./pages/unauthorized/unauthorized.component').then((m) => m.UnauthorizedComponent),
  },

  /**
   * Wildcard Route - Must be last
   * Catches all unmatched routes and shows 404
   */
  {
    path: '**',
    loadComponent: () =>
      import('./pages/not-found/not-found.component').then((m) => m.NotFoundComponent),
  },
];
