import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { isDevMode } from '@angular/core';

import { routes } from './app.routes';
import { AppState } from './store/app.state';
import { employeeReducer } from './store/employees/employees.reducer';
import { performanceReducer } from './store/performance/performance.reducer';
import { preferencesReducer } from './store/preferences/preferences.reducer';
import { dashboardReducer } from './store/dashboard/dashboard.reducer';
import { EmployeeEffects } from './store/employees/employees.effects';
import { PerformanceEffects } from './store/performance/performance.effects';
import { PreferencesEffects } from './store/preferences/preferences.effects';
import { DashboardEffects } from './store/dashboard/dashboard.effects';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';

export const appConfig: ApplicationConfig = {
  providers: [
    provideAnimationsAsync(),
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(),
    provideStore<AppState>({
      employees: employeeReducer,
      performance: performanceReducer,
      preferences: preferencesReducer,
      dashboard: dashboardReducer,
    }),
    provideEffects([EmployeeEffects, PerformanceEffects, PreferencesEffects, DashboardEffects]),
    provideStoreDevtools({
      maxAge: 25,
      logOnly: !isDevMode(),
      features: {
        pause: true,
        lock: true,
        persist: true,
      },
    }),
  ],
};
