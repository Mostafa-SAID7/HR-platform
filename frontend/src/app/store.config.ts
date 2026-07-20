/**
 * Application Store Configuration
 * Central configuration for all feature stores
 * 
 * This file manages the registration of all NgRx feature stores
 * from different features to maintain a scalable store architecture.
 */

import { EnvironmentProviders } from '@angular/core';
import { ActionReducerMap } from '@ngrx/store';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';

// Feature Store Configurations
import { EMPLOYEES_STORE_CONFIG } from './features/employees/store/employees.config';
import { DASHBOARD_STORE_CONFIG } from './features/dashboard/store/dashboard.config';
import { PERFORMANCE_STORE_CONFIG } from './features/performance/store/performance.config';
import { PREFERENCES_STORE_CONFIG } from './features/settings/store/preferences.config';

/**
 * Root Application State Interface
 * Combines all feature states
 */
export interface AppState {
  [EMPLOYEES_STORE_CONFIG.featureName]: any;
  [DASHBOARD_STORE_CONFIG.featureName]: any;
  [PERFORMANCE_STORE_CONFIG.featureName]: any;
  [PREFERENCES_STORE_CONFIG.featureName]: any;
}

/**
 * Store Configuration Providers
 * Include all feature store configurations
 */
export function getStoreConfig(): EnvironmentProviders[] {
  return [
    provideStore({
      [EMPLOYEES_STORE_CONFIG.featureName]: EMPLOYEES_STORE_CONFIG.reducer,
      [DASHBOARD_STORE_CONFIG.featureName]: DASHBOARD_STORE_CONFIG.reducer,
      [PERFORMANCE_STORE_CONFIG.featureName]: PERFORMANCE_STORE_CONFIG.reducer,
      [PREFERENCES_STORE_CONFIG.featureName]: PREFERENCES_STORE_CONFIG.reducer,
    } as ActionReducerMap<any>),
    provideEffects([
      ...EMPLOYEES_STORE_CONFIG.effects,
      ...DASHBOARD_STORE_CONFIG.effects,
      ...PERFORMANCE_STORE_CONFIG.effects,
      ...PREFERENCES_STORE_CONFIG.effects,
    ]),
  ];
}

/**
 * Feature Store Configurations Array
 * Can be used for dynamic registration or reference
 */
export const FEATURE_STORE_CONFIGS = [
  EMPLOYEES_STORE_CONFIG,
  DASHBOARD_STORE_CONFIG,
  PERFORMANCE_STORE_CONFIG,
  PREFERENCES_STORE_CONFIG,
];
