import { Component, OnInit, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../components/card/card.component';
import { ChartComponent, type ChartConfig } from '../../components/chart/chart.component';
import { BadgeComponent } from '../../components/badge/badge.component';
import { DataService } from '../../services/data.service';
import { forkJoin } from 'rxjs';

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
  imports: [CommonModule, CardComponent, ChartComponent, BadgeComponent],
  template: `
    <div class="space-y-6">
      <!-- Header -->
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-3xl font-bold text-slate-900 dark:text-white">Workforce Metrics</h1>
          <p class="text-slate-600 dark:text-slate-400 mt-2">
            Monitor key workforce indicators and trends across regions and departments
          </p>
        </div>
        <div *ngIf="isLoading()" class="flex items-center text-indigo-500">
          <svg class="animate-spin h-5 w-5 mr-3" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" fill="none"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          <span class="text-sm font-medium">Refreshing Data...</span>
        </div>
      </div>

      <!-- KPI Cards -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-6">
        <!-- Total Headcount -->
        <app-card>
          <div class="text-center">
            <p class="text-sm font-medium text-slate-600 dark:text-slate-400">Total Headcount</p>
            <p class="text-3xl font-bold text-indigo-600 dark:text-indigo-400 mt-2">
              {{ totalHeadcount() | number }}
            </p>
            <app-badge variant="success" class="mt-3 justify-center">
              ↑ 2.5% from last month
            </app-badge>
          </div>
        </app-card>

        <!-- Active Employees -->
        <app-card>
          <div class="text-center">
            <p class="text-sm font-medium text-slate-600 dark:text-slate-400">Active Employees</p>
            <p class="text-3xl font-bold text-emerald-600 dark:text-emerald-400 mt-2">
              {{ activeEmployees() | number }}
            </p>
            <app-badge variant="success" class="mt-3 justify-center">
              ↑ 1.2% from last month
            </app-badge>
          </div>
        </app-card>

        <!-- On Leave -->
        <app-card>
          <div class="text-center">
            <p class="text-sm font-medium text-slate-600 dark:text-slate-400">On Leave</p>
            <p class="text-3xl font-bold text-yellow-600 dark:text-yellow-400 mt-2">
              {{ onLeave() | number }}
            </p>
            <app-badge variant="warning" class="mt-3 justify-center">
              ↑ 0.8% from last month
            </app-badge>
          </div>
        </app-card>

        <!-- New Hires -->
        <app-card>
          <div class="text-center">
            <p class="text-sm font-medium text-slate-600 dark:text-slate-400">New Hires (Month)</p>
            <p class="text-3xl font-bold text-blue-600 dark:text-blue-400 mt-2">
              {{ newHires() | number }}
            </p>
            <app-badge variant="info" class="mt-3 justify-center">
              ↑ 12% from last month
            </app-badge>
          </div>
        </app-card>

        <!-- Departures -->
        <app-card>
          <div class="text-center">
            <p class="text-sm font-medium text-slate-600 dark:text-slate-400">Departures (Month)</p>
            <p class="text-3xl font-bold text-red-600 dark:text-red-400 mt-2">
              {{ departures() | number }}
            </p>
            <app-badge variant="error" class="mt-3 justify-center">
              ↑ 5% from last month
            </app-badge>
          </div>
        </app-card>
      </div>

      <!-- Metrics by Region -->
      <app-card>
        <div class="space-y-4">
          <div class="flex items-center justify-between">
            <h2 class="text-xl font-semibold text-slate-900 dark:text-white">
              Headcount by Region
            </h2>
            <app-badge variant="info">Bar Chart</app-badge>
          </div>
          @defer (on viewport) {
            <app-chart
              [chartConfig]="headcountByRegionChart()"
              (chartClick)="onChartClick($event)"
            ></app-chart>
          } @placeholder {
            <div class="h-[400px] flex items-center justify-center bg-slate-50 dark:bg-slate-800/50 rounded-lg">
              <span class="text-slate-400">Loading Regional Data...</span>
            </div>
          }
        </div>
      </app-card>

      <!-- Metrics by Department -->
      <app-card>
        <div class="space-y-4">
          <div class="flex items-center justify-between">
            <h2 class="text-xl font-semibold text-slate-900 dark:text-white">
              Headcount by Department
            </h2>
            <app-badge variant="info">Bar Chart</app-badge>
          </div>
          @defer (on viewport) {
            <app-chart
              [chartConfig]="headcountByDepartmentChart()"
              (chartClick)="onChartClick($event)"
            ></app-chart>
          } @placeholder {
            <div class="h-[400px] flex items-center justify-center bg-slate-50 dark:bg-slate-800/50 rounded-lg">
              <span class="text-slate-400">Loading Department Data...</span>
            </div>
          }
        </div>
      </app-card>

      <!-- Employment Status Distribution -->
      <app-card>
        <div class="space-y-4">
          <div class="flex items-center justify-between">
            <h2 class="text-xl font-semibold text-slate-900 dark:text-white">
              Employment Status Distribution
            </h2>
            <app-badge variant="info">Pie Chart</app-badge>
          </div>
          @defer (on viewport) {
            <app-chart
              [chartConfig]="employmentStatusChart()"
              (chartClick)="onChartClick($event)"
            ></app-chart>
          } @placeholder {
            <div class="h-[400px] flex items-center justify-center bg-slate-50 dark:bg-slate-800/50 rounded-lg">
              <span class="text-slate-400">Loading Status Distribution...</span>
            </div>
          }
        </div>
      </app-card>

      <!-- Historical Trend Data (12 Months) -->
      <app-card>
        <div class="space-y-4">
          <div class="flex items-center justify-between">
            <h2 class="text-xl font-semibold text-slate-900 dark:text-white">
              Headcount Trend (12 Months)
            </h2>
            <app-badge variant="success">↑ 3.2% growth</app-badge>
          </div>
          @defer (on viewport) {
            <app-chart
              [chartConfig]="headcountTrendChart()"
              (chartClick)="onChartClick($event)"
            ></app-chart>
          } @placeholder {
            <div class="h-[400px] flex items-center justify-center bg-slate-50 dark:bg-slate-800/50 rounded-lg">
              <span class="text-slate-400">Loading Trend Data...</span>
            </div>
          }
        </div>
      </app-card>

      <!-- Drill-down Section -->
      <app-card>
        <div class="space-y-4">
          <h2 class="text-xl font-semibold text-slate-900 dark:text-white">
            Drill-Down to Employee Records
          </h2>
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <button
              (click)="drillDownByRegion('Middle East')"
              class="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
            >
              View Middle East Employees
            </button>
            <button
              (click)="drillDownByRegion('Europe')"
              class="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
            >
              View Europe Employees
            </button>
            <button
              (click)="drillDownByStatus('Active')"
              class="px-4 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 transition-colors"
            >
              View Active Employees
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
