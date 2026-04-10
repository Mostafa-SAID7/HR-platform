import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Store } from '@ngrx/store';
import { Observable, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { CardComponent, KPICardComponent, BadgeComponent } from '../../components';
import { DashboardMetrics } from '../../store/dashboard/dashboard.state';
import { selectDashboardMetrics } from '../../store/dashboard/dashboard.selectors';
import { AppState } from '../../store/app.state';
import { loadDashboardMetrics, updateDashboardMetricsRealtime } from '../../store/dashboard/dashboard.actions';
import { WebSocketService } from '../../services/websocket.service';

/**
 * Dashboard Page Component
 *
 * Main dashboard page displaying KPI cards and key metrics with real-time updates.
 *
 * Requirements: 1.1, 2.1, 3.1, 17.1
 */

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    CardComponent,
    KPICardComponent,
    BadgeComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-container">
      <!-- KPI Cards Grid -->
      <div class="card-grid">
        <!-- Total Headcount KPI -->
        <app-kpi-card
          [data]="{
            label: 'Total Headcount',
            value: metrics()?.totalHeadcount || 0,
            previousValue: 11700,
            trend: 'up',
            trendPercentage: 2.5,
            loading: metricsLoading()
          }"
        ></app-kpi-card>

        <!-- Active Employees KPI -->
        <app-kpi-card
          [data]="{
            label: 'Active Employees',
            value: metrics()?.activeEmployees || 0,
            previousValue: 11700,
            trend: 'up',
            trendPercentage: 1.2,
            loading: metricsLoading()
          }"
        ></app-kpi-card>

        <!-- On Leave KPI -->
        <app-kpi-card
          [data]="{
            label: 'On Leave',
            value: metrics()?.onLeave || 0,
            previousValue: 148,
            trend: 'up',
            trendPercentage: 0.8,
            loading: metricsLoading()
          }"
        ></app-kpi-card>

        <!-- New Hires KPI -->
        <app-kpi-card
          [data]="{
            label: 'New Hires (This Month)',
            value: metrics()?.newHires || 0,
            previousValue: 40,
            trend: 'up',
            trendPercentage: 12,
            loading: metricsLoading()
          }"
        ></app-kpi-card>
      </div>

      <!-- Main Dashboard Grid -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-8 mt-8">
        <!-- Welcome Message -->
        <app-card>
          <div appCardHeader class="flex items-center gap-2">
             <div class="w-2 h-2 bg-indigo-500 rounded-full animate-pulse"></div>
             <span class="text-sm font-bold uppercase tracking-wider text-slate-500">Welcome</span>
          </div>
          <div class="py-10 text-center">
            <h2 class="text-3xl font-black text-slate-900 dark:text-white mb-4 tracking-tight">
              Workforce Intelligence Hub
            </h2>
            <p class="text-slate-600 dark:text-slate-400 max-w-md mx-auto leading-relaxed">
              Your real-time command center for talent metrics, engagement data, and organizational growth insights.
            </p>
          </div>
        </app-card>

        <!-- Quick Stats -->
        <app-card>
          <div appCardHeader class="flex items-center gap-2">
             <div class="w-2 h-2 bg-emerald-500 rounded-full"></div>
             <span class="text-sm font-bold uppercase tracking-wider text-slate-500">System Pulse</span>
          </div>
          <div class="grid grid-cols-1 md:grid-cols-3 gap-8 py-4">
            <div class="flex flex-col items-center">
              <p class="text-xs font-black text-slate-400 dark:text-slate-500 uppercase tracking-tighter mb-1">Departures</p>
              <p class="text-4xl font-black text-red-500">{{ metrics()?.departures || 0 }}</p>
              <span class="text-[10px] text-slate-400">Current Month</span>
            </div>
            <div class="flex flex-col items-center">
              <p class="text-xs font-black text-slate-400 dark:text-slate-500 uppercase tracking-tighter mb-1">Last Sync</p>
              <p class="text-base font-bold text-slate-700 dark:text-slate-300 mt-2">{{ formatLastUpdated(metrics()?.lastUpdated) }}</p>
              <div class="flex items-center gap-1 mt-1">
                <span class="w-1.5 h-1.5 bg-emerald-500 rounded-full animate-ping"></span>
                <span class="text-[10px] text-emerald-600">Live</span>
              </div>
            </div>
            <div class="flex flex-col items-center justify-center">
               <div class="bg-emerald-50 dark:bg-emerald-900/20 px-4 py-2 rounded-full border border-emerald-100 dark:border-emerald-800/30">
                  <span class="text-xs font-black text-emerald-600 dark:text-emerald-400 uppercase tracking-widest">Active</span>
               </div>
            </div>
          </div>
        </app-card>
      </div>
    </div>
  `,
})
export class DashboardComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private store: Store<AppState>;

  private wsService: WebSocketService;

  // Signals for reactive state
  metrics = signal<DashboardMetrics | null>(null);
  metricsLoading = signal(false);

  constructor(store: Store<AppState>, wsService: WebSocketService) {
    this.store = store;
    this.wsService = wsService;
  }

  ngOnInit(): void {
    // Load initial metrics
    this.store.dispatch(loadDashboardMetrics());

    // Subscribe to metrics from store
    this.store
      .select(selectDashboardMetrics)
      .pipe(takeUntil(this.destroy$))
      .subscribe((metrics) => {
        if (metrics) {
          this.metrics.set(metrics);
        }
      });

    // Subscribe to real-time web socket updates
    this.wsService.subscribe<DashboardMetrics>('DASHBOARD_METRICS_UPDATE')
      .pipe(takeUntil(this.destroy$))
      .subscribe((realtimeMetrics) => {
        this.store.dispatch(updateDashboardMetricsRealtime({ metrics: realtimeMetrics }));
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  formatLastUpdated(timestamp?: string): string {
    if (!timestamp) return 'Just now';
    const date = new Date(timestamp);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours}h ago`;
    return date.toLocaleDateString();
  }
}
