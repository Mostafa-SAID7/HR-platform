import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { catchError, map, switchMap, retry, tap } from 'rxjs/operators';
import * as DashboardActions from './dashboard.actions';
import { DataService } from '../../services/data.service';

/**
 * DashboardEffects handles side effects for dashboard-related operations.
 */
@Injectable()
export class DashboardEffects {
  private actions$ = inject(Actions);
  private dataService = inject(DataService);

  /**
   * Effect: Load Dashboard Configs
   */
  readonly loadDashboardConfigs$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DashboardActions.loadDashboardConfigs),
      switchMap(() =>
        of([]).pipe(
          retry({
            count: 3,
            delay: (error, retryCount) => {
              const delayMs = Math.pow(2, retryCount) * 1000;
              return new Promise((resolve) => setTimeout(resolve, delayMs));
            },
          }),
          map((configs) => DashboardActions.loadDashboardConfigsSuccess({ configs })),
          catchError((error) =>
            of(
              DashboardActions.loadDashboardConfigsFailure({
                error: error.message || 'Failed to load dashboard configurations',
              }),
            ),
          ),
        ),
      ),
    ),
  );

  /**
   * Effect: Load Dashboard Metrics
   */
  readonly loadDashboardMetrics$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DashboardActions.loadDashboardMetrics),
      switchMap(() =>
        this.dataService.getDashboardMetrics().pipe(
          retry({
            count: 3,
            delay: (error, retryCount) => {
              const delayMs = Math.pow(2, retryCount) * 1000;
              return new Promise((resolve) => setTimeout(resolve, delayMs));
            },
          }),
          map((metrics) => DashboardActions.loadDashboardMetricsSuccess({ metrics })),
          catchError((error) =>
            of(
              DashboardActions.loadDashboardMetricsFailure({
                error: error.message || 'Failed to load dashboard metrics',
              }),
            ),
          ),
        )
      ),
    ),
  );

  /**
   * Effect: Create Dashboard Config
   */
  readonly createDashboardConfig$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DashboardActions.createDashboardConfig),
      switchMap(({ config }) =>
        of(config).pipe(
          retry(2),
          tap(() => console.log('[DashboardEffects] Dashboard config created successfully')),
          switchMap(() => of(DashboardActions.loadDashboardConfigs())),
          catchError((error) =>
            of(
              DashboardActions.loadDashboardConfigsFailure({
                error: error.message || 'Failed to create dashboard configuration',
              }),
            ),
          ),
        ),
      ),
    ),
  );

  /**
   * Effect: Update Dashboard Config
   */
  readonly updateDashboardConfig$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DashboardActions.updateDashboardConfig),
      switchMap(({ config }) =>
        of(config).pipe(
          retry(2),
          tap(() => console.log('[DashboardEffects] Dashboard config updated successfully')),
          switchMap(() => of(DashboardActions.loadDashboardConfigs())),
          catchError((error) =>
            of(
              DashboardActions.loadDashboardConfigsFailure({
                error: error.message || 'Failed to update dashboard configuration',
              }),
            ),
          ),
        ),
      ),
    ),
  );

  /**
   * Effect: Delete Dashboard Config
   */
  readonly deleteDashboardConfig$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DashboardActions.deleteDashboardConfig),
      switchMap(({ configId }) =>
        of(configId).pipe(
          retry(2),
          tap(() => console.log('[DashboardEffects] Dashboard config deleted successfully')),
          switchMap(() => of(DashboardActions.loadDashboardConfigs())),
          catchError((error) =>
            of(
              DashboardActions.loadDashboardConfigsFailure({
                error: error.message || 'Failed to delete dashboard configuration',
              }),
            ),
          ),
        ),
      ),
    ),
  );
}
