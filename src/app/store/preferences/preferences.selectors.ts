import { createFeatureSelector, createSelector } from '@ngrx/store';
import { PreferencesState } from './preferences.state';

export const selectPreferencesState = createFeatureSelector<PreferencesState>('preferences');

export const selectPreferences = createSelector(
  selectPreferencesState,
  (state) => state.preferences,
);

export const selectPreferencesLoading = createSelector(
  selectPreferencesState,
  (state) => state.loading,
);

export const selectPreferencesError = createSelector(
  selectPreferencesState,
  (state) => state.error,
);

export const selectLanguage = createSelector(selectPreferences, (prefs) => prefs?.language || 'en');

export const selectDarkMode = createSelector(
  selectPreferences,
  (prefs) => prefs?.darkMode ?? false,
);

export const selectTheme = createSelector(selectPreferences, (prefs) => prefs?.theme || 'light');

export const selectNotificationsEnabled = createSelector(
  selectPreferences,
  (prefs) => prefs?.notificationsEnabled ?? true,
);

export const selectEmailNotifications = createSelector(
  selectPreferences,
  (prefs) => prefs?.emailNotifications ?? true,
);

export const selectPushNotifications = createSelector(
  selectPreferences,
  (prefs) => prefs?.pushNotifications ?? true,
);

export const selectItemsPerPage = createSelector(
  selectPreferences,
  (prefs) => prefs?.itemsPerPage || 10,
);

export const selectDefaultDashboard = createSelector(
  selectPreferences,
  (prefs) => prefs?.defaultDashboard || 'main',
);

export const selectSavedFilters = createSelector(
  selectPreferences,
  (prefs) => prefs?.savedFilters || [],
);

export const selectSavedFilterById = (filterId: string) =>
  createSelector(selectSavedFilters, (filters) => filters.find((f) => f.id === filterId));
