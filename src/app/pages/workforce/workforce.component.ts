import { Component, OnInit, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { KPICardComponent, CardComponent } from '../../components';
import { ChartComponent, type ChartConfig } from '../../components/chart/chart.component';
import { BadgeComponent } from '../../components/badge/badge.component';
import { DataService } from '../../services/data.service';
import { forkJoin } from 'rxjs';
import { fadeIn, slideInUp } from '../../shared/animations';

/**
 * Workforce Metrics Dashboard
 *
 * Displays key workforce metrics including headcount, active employees, on leave,
 * new hires, departures, and historical trends segmented by region, department,
 * and employment status.
 *
 * Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6
 */

@Component({
  selector: 'app-workforce',
  standalone: true,
  imports: [CommonModule, CardComponent, KPICardComponent, ChartComponent, BadgeComponent],
  animations: [fadeIn, slideInUp],
  template: `
    <div class="page-container" [@fadeIn]>
      <!-- Balanced Page Header -->
      <div class="flex flex-col lg:flex-row lg:items-end justify-between gap-8 mb-4" [@slideInUp]>
        <div class="space-y-2">
          <h1 class="text-4xl font-black text-slate-900 dark:text-white tracking-tighter">
            Workforce Intelligence
          </h1>
          <p class="text-slate-500 dark:text-slate-400 text-lg font-medium leading-relaxed max-w-2xl">
            Real-time workforce monitoring, regional distribution, and employment status analytics.
          </p>
        </div>
        
        <div *ngIf="isLoading()" class="flex items-center gap-3 bg-indigo-50 dark:bg-indigo-900/40 px-6 py-3 rounded-2xl border border-indigo-100 dark:border-indigo-800/30">
          <div class="w-2 h-2 bg-indigo-600 rounded-full animate-ping"></div>
          <span class="text-sm font-bold text-indigo-600 dark:text-indigo-400 uppercase tracking-widest">Syncing Data...</span>
        </div>
      </div>

      <!-- KPI Cards Grid -->
      <div class="card-grid" [@slideInUp]>
        <app-kpi-card
          [data]="{
            label: 'Total Headcount',
            value: totalHeadcount(),
            previousValue: 11700,
            trend: 'up',
            trendPercentage: 2.5,
            loading: isLoading()
          }"
        ></app-kpi-card>

        <app-kpi-card
          [data]="{
            label: 'Active Employees',
            value: activeEmployees(),
            previousValue: 11500,
            trend: 'up',
            trendPercentage: 1.2,
            loading: isLoading()
          }"
        ></app-kpi-card>

        <app-kpi-card
          [data]="{
            label: 'On Leave',
            value: onLeave(),
            previousValue: 148,
            trend: 'up',
            trendPercentage: 0.8,
            loading: isLoading()
          }"
        ></app-kpi-card>

        <app-kpi-card
          [data]="{
            label: 'New Hires',
            value: newHires(),
            previousValue: 40,
            trend: 'up',
            trendPercentage: 12,
            loading: isLoading()
          }"
        ></app-kpi-card>

        <app-kpi-card
          [data]="{
            label: 'Departures',
            value: departures(),
            previousValue: 30,
            trend: 'up',
            trendPercentage: 5,
            loading: isLoading()
          }"
        ></app-kpi-card>
      </div>

      <!-- Metrics by Region & Department -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-8 mt-8" [@slideInUp]>
        <!-- Headcount by Region -->
        <app-card>
          <div appCardHeader class="flex items-center justify-between">
            <div class="flex items-center gap-3">
               <div class="w-1.5 h-6 bg-indigo-600 rounded-full"></div>
               <h2 class="text-xl font-black text-slate-900 dark:text-white tracking-tight">Regional Distribution</h2>
            </div>
            <app-badge variant="info">Geo Split</app-badge>
          </div>
          <div class="py-4">
            @defer (on viewport) {
              <app-chart
                [chartConfig]="headcountByRegionChart()"
                (chartClick)="onChartClick($event)"
              ></app-chart>
            } @placeholder {
              <div class="h-[400px] flex items-center justify-center bg-slate-50/50 dark:bg-slate-900/20 rounded-2xl border border-dashed border-slate-200 dark:border-slate-700">
                <span class="text-xs font-bold text-slate-400 uppercase tracking-widest animate-pulse">Mapping Regions...</span>
              </div>
            }
          </div>
        </app-card>

        <!-- Headcount by Department -->
        <app-card>
          <div appCardHeader class="flex items-center justify-between">
            <div class="flex items-center gap-3">
               <div class="w-1.5 h-6 bg-emerald-500 rounded-full"></div>
               <h2 class="text-xl font-black text-slate-900 dark:text-white tracking-tight">Functional Headcount</h2>
            </div>
            <app-badge variant="info">Dept View</app-badge>
          </div>
          <div class="py-4">
            @defer (on viewport) {
              <app-chart
                [chartConfig]="headcountByDepartmentChart()"
                (chartClick)="onChartClick($event)"
              ></app-chart>
            } @placeholder {
              <div class="h-[400px] flex items-center justify-center bg-slate-50/50 dark:bg-slate-900/20 rounded-2xl border border-dashed border-slate-200 dark:border-slate-700">
                <span class="text-xs font-bold text-slate-400 uppercase tracking-widest animate-pulse">Analyzing Departments...</span>
              </div>
            }
          </div>
        </app-card>
      </div>

      <!-- Employment Status & Historical Trends -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-8 mt-8" [@slideInUp]>
        <!-- Historical Trend -->
        <app-card>
          <div appCardHeader class="flex items-center justify-between">
            <div class="flex items-center gap-3">
               <div class="w-1.5 h-6 bg-indigo-400 rounded-full"></div>
               <h2 class="text-xl font-black text-slate-900 dark:text-white tracking-tight">12-Month Momentum</h2>
            </div>
            <app-badge variant="success">↑ 3.2% Trend</app-badge>
          </div>
          <div class="py-4">
            @defer (on viewport) {
              <app-chart
                [chartConfig]="headcountTrendChart()"
                (chartClick)="onChartClick($event)"
              ></app-chart>
            } @placeholder {
              <div class="h-[400px] flex items-center justify-center bg-slate-50/50 dark:bg-slate-900/20 rounded-2xl border border-dashed border-slate-200 dark:border-slate-700">
                <span class="text-xs font-bold text-slate-400 uppercase tracking-widest animate-pulse">Mapping Momentum...</span>
              </div>
            }
          </div>
        </app-card>

        <!-- Employment Status -->
        <app-card>
          <div appCardHeader class="flex items-center justify-between">
            <div class="flex items-center gap-3">
               <div class="w-1.5 h-6 bg-amber-400 rounded-full"></div>
               <h2 class="text-xl font-black text-slate-900 dark:text-white tracking-tight">Contractual Mix</h2>
            </div>
            <app-badge variant="info">Status Split</app-badge>
          </div>
          <div class="py-4">
            @defer (on viewport) {
              <app-chart
                [chartConfig]="employmentStatusChart()"
                (chartClick)="onChartClick($event)"
              ></app-chart>
            } @placeholder {
              <div class="h-[400px] flex items-center justify-center bg-slate-50/50 dark:bg-slate-900/20 rounded-2xl border border-dashed border-slate-200 dark:border-slate-700">
                <span class="text-xs font-bold text-slate-400 uppercase tracking-widest animate-pulse">Computing Mix...</span>
              </div>
            }
          </div>
        </app-card>
      </div>

      <!-- Advanced Drill-Down -->
      <app-card class="mt-8" [@slideInUp]>
        <div appCardHeader class="flex items-center gap-3">
          <div class="w-1.5 h-6 bg-slate-400 rounded-full"></div>
          <h2 class="text-xl font-black text-slate-900 dark:text-white tracking-tight">Strategic Drill-Down</h2>
        </div>
        <div class="py-6">
          <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
            <button
              (click)="drillDownByRegion('Middle East')"
              class="group h-24 bg-slate-50 dark:bg-slate-900/40 border border-slate-200 dark:border-slate-700/50 rounded-2xl p-6 flex items-center justify-between hover:border-indigo-500/50 hover:bg-white dark:hover:bg-slate-800 transition-all duration-300"
            >
              <div class="text-left">
                <p class="text-xs font-black text-slate-400 uppercase tracking-widest mb-1">Region Focus</p>
                <p class="text-lg font-bold text-slate-900 dark:text-white">Middle East Force</p>
              </div>
              <div class="w-10 h-10 rounded-xl bg-indigo-50 dark:bg-indigo-900/40 flex items-center justify-center text-indigo-600 dark:text-indigo-400 group-hover:scale-110 transition-transform">
                <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 8l4 4m0 0l-4 4m4-4H3" /></svg>
              </div>
            </button>

            <button
              (click)="drillDownByRegion('Europe')"
               class="group h-24 bg-slate-50 dark:bg-slate-900/40 border border-slate-200 dark:border-slate-700/50 rounded-2xl p-6 flex items-center justify-between hover:border-indigo-500/50 hover:bg-white dark:hover:bg-slate-800 transition-all duration-300"
            >
              <div class="text-left">
                <p class="text-xs font-black text-slate-400 uppercase tracking-widest mb-1">Region Focus</p>
                <p class="text-lg font-bold text-slate-900 dark:text-white">European Force</p>
              </div>
              <div class="w-10 h-10 rounded-xl bg-indigo-50 dark:bg-indigo-900/40 flex items-center justify-center text-indigo-600 dark:text-indigo-400 group-hover:scale-110 transition-transform">
                <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 8l4 4m0 0l-4 4m4-4H3" /></svg>
              </div>
            </button>

            <button
              (click)="drillDownByStatus('Active')"
               class="group h-24 bg-slate-50 dark:bg-slate-900/40 border border-slate-200 dark:border-slate-700/50 rounded-2xl p-6 flex items-center justify-between hover:border-emerald-500/50 hover:bg-white dark:hover:bg-slate-800 transition-all duration-300"
            >
              <div class="text-left">
                <p class="text-xs font-black text-slate-400 uppercase tracking-widest mb-1">Status Focus</p>
                <p class="text-lg font-bold text-slate-900 dark:text-white">Active Personnel</p>
              </div>
              <div class="w-10 h-10 rounded-xl bg-emerald-50 dark:bg-emerald-900/40 flex items-center justify-center text-emerald-600 dark:text-emerald-400 group-hover:scale-110 transition-transform">
                <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 8l4 4m0 0l-4 4m4-4H3" /></svg>
              </div>
            </button>
          </div>
        </div>
      </app-card>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkforceComponent implements OnInit {
  private dataService = inject(DataService);

  totalHeadcount = signal<number>(0);
  activeEmployees = signal<number>(0);
  onLeave = signal<number>(0);
  newHires = signal<number>(0);
  departures = signal<number>(0);
  isLoading = signal<boolean>(true);

  headcountByRegionChart = signal<ChartConfig>({
    type: 'bar',
    title: 'Headcount by Region',
    data: { categories: [], values: [] },
    height: '400px',
  });

  headcountByDepartmentChart = signal<ChartConfig>({
    type: 'bar',
    title: 'Headcount by Department',
    data: { categories: [], values: [] },
    height: '400px',
  });

  employmentStatusChart = signal<ChartConfig>({
    type: 'pie',
    title: 'Employment Status Distribution',
    data: { values: [] },
    height: '400px',
  });

  headcountTrendChart = signal<ChartConfig>({
    type: 'line',
    title: 'Headcount Trend (Last 12 Months)',
    data: { categories: [], values: [] },
    height: '400px',
  });

  ngOnInit(): void {
    this.loadWorkforceData();
  }

  private loadWorkforceData(): void {
    this.isLoading.set(true);
    forkJoin({
      summary: this.dataService.getDashboardMetrics(),
      details: this.dataService.getWorkforceMetrics()
    }).subscribe({
      next: (data) => {
        // Update summary KPIs
        this.totalHeadcount.set(data.summary.totalHeadcount);
        this.activeEmployees.set(data.summary.activeEmployees);
        this.onLeave.set(data.summary.onLeave);
        this.newHires.set(data.summary.newHires);
        this.departures.set(data.summary.departures);

        // Update Charts
        this.headcountByRegionChart.update(config => ({
          ...config,
          data: {
            categories: data.details.byRegion.map((r: any) => r.region),
            values: data.details.byRegion.map((r: any) => r.headcount)
          }
        }));

        this.headcountByDepartmentChart.update(config => ({
          ...config,
          data: {
            categories: data.details.byDepartment.map((d: any) => d.department),
            values: data.details.byDepartment.map((d: any) => d.headcount)
          }
        }));

        this.employmentStatusChart.update(config => ({
          ...config,
          data: {
            values: data.details.byStatus.map((s: any) => ({
              name: s.status,
              value: s.count
            }))
          }
        }));

        this.headcountTrendChart.update(config => ({
          ...config,
          data: {
            categories: data.details.trends.map((t: any) => t.month),
            values: data.details.trends.map((t: any) => t.headcount)
          }
        }));

        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading workforce data:', err);
        this.isLoading.set(false);
      }
    });
  }

  onChartClick(event: { name: string; value: unknown }): void {
    console.log('Chart clicked:', event);
  }

  drillDownByRegion(region: string): void {
    console.log('Drilling down to region:', region);
  }

  drillDownByStatus(status: string): void {
    console.log('Drilling down to status:', status);
  }
}
