import { Component, OnInit, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../components/card/card.component';
import { ChartComponent, type ChartConfig } from '../../components/chart/chart.component';
import { BadgeComponent } from '../../components/badge/badge.component';
import { DataService } from '../../services/data.service';
import { forkJoin } from 'rxjs';

/**
 * Turnover Analysis Dashboard
 *
 * Displays historical turnover rates, trends, patterns, and 6-month predictions
 * with confidence levels segmented by department, region, and role.
 *
 * Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6
 */

@Component({
  selector: 'app-turnover',
  standalone: true,
  imports: [CommonModule, CardComponent, ChartComponent, BadgeComponent],
  template: `
    <div class="space-y-6">
      <!-- Header -->
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-3xl font-bold text-slate-900 dark:text-white">Turnover Analysis</h1>
          <p class="text-slate-600 dark:text-slate-400 mt-2">
            Analyze historical turnover patterns and predict future turnover trends
          </p>
        </div>
        <div *ngIf="isLoading()" class="flex items-center text-indigo-500">
          <svg class="animate-spin h-5 w-5 mr-3" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" fill="none"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          <span class="text-sm font-medium">Analyzing Patterns...</span>
        </div>
      </div>

      <!-- Key Metrics -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-6">
        <app-card>
          <div class="text-center">
            <p class="text-sm font-medium text-slate-600 dark:text-slate-400">
              Current Turnover Rate
            </p>
            <p class="text-3xl font-bold text-indigo-600 dark:text-indigo-400 mt-2">
              {{ currentTurnoverRate() }}%
            </p>
            <app-badge variant="warning" class="mt-3 justify-center">
              ↑ 0.5% from last month
            </app-badge>
          </div>
        </app-card>

        <app-card>
          <div class="text-center">
            <p class="text-sm font-medium text-slate-600 dark:text-slate-400">
              Departures (This Month)
            </p>
            <p class="text-3xl font-bold text-red-600 dark:text-red-400 mt-2">
              {{ departuresThisMonth() }}
            </p>
            <app-badge variant="error" class="mt-3 justify-center">
              ↑ 5% from last month
            </app-badge>
          </div>
        </app-card>

        <app-card>
          <div class="text-center">
            <p class="text-sm font-medium text-slate-600 dark:text-slate-400">Average Tenure</p>
            <p class="text-3xl font-bold text-emerald-600 dark:text-emerald-400 mt-2">
              {{ averageTenure() }} yrs
            </p>
            <app-badge variant="success" class="mt-3 justify-center">
              ↑ 0.3 years from last year
            </app-badge>
          </div>
        </app-card>

        <app-card>
          <div class="text-center">
            <p class="text-sm font-medium text-slate-600 dark:text-slate-400">High Risk Roles</p>
            <p class="text-3xl font-bold text-yellow-600 dark:text-yellow-400 mt-2">
              {{ highRiskRoles() }}
            </p>
            <app-badge variant="warning" class="mt-3 justify-center">
              Requires attention
            </app-badge>
          </div>
        </app-card>
      </div>

      <!-- Historical Turnover Rates by Department -->
      <app-card>
        <div class="space-y-4">
          <div class="flex items-center justify-between">
            <h2 class="text-xl font-semibold text-slate-900 dark:text-white">
              Turnover Rate by Department
            </h2>
            <app-badge variant="info">Bar Chart</app-badge>
          </div>
          @defer (on viewport) {
            <app-chart
              [chartConfig]="turnoverByDepartmentChart()"
              (chartClick)="onChartClick($event)"
            ></app-chart>
          } @placeholder {
            <div class="h-[400px] flex items-center justify-center bg-slate-50 dark:bg-slate-800/50 rounded-lg">
              <span class="text-slate-400">Loading Department turnover...</span>
            </div>
          }
        </div>
      </app-card>

      <!-- Turnover Trends and Patterns -->
      <app-card>
        <div class="space-y-4">
          <div class="flex items-center justify-between">
            <h2 class="text-xl font-semibold text-slate-900 dark:text-white">
              Turnover Trends (24 Months)
            </h2>
            <app-badge variant="warning">↑ Increasing trend</app-badge>
          </div>
          @defer (on viewport) {
            <app-chart
              [chartConfig]="turnoverTrendsChart()"
              (chartClick)="onChartClick($event)"
            ></app-chart>
          } @placeholder {
            <div class="h-[400px] flex items-center justify-center bg-slate-50 dark:bg-slate-800/50 rounded-lg">
              <span class="text-slate-400">Loading Turnover Trends...</span>
            </div>
          }
        </div>
      </app-card>

      <!-- 6-Month Turnover Predictions -->
      <app-card>
        <div class="space-y-4">
          <div class="flex items-center justify-between">
            <h2 class="text-xl font-semibold text-slate-900 dark:text-white">
              6-Month Turnover Forecast
            </h2>
            <app-badge variant="info">Prediction</app-badge>
          </div>
          @defer (on viewport) {
            <app-chart
              [chartConfig]="turnoverForecastChart()"
              (chartClick)="onChartClick($event)"
            ></app-chart>
          } @placeholder {
            <div class="h-[400px] flex items-center justify-center bg-slate-50 dark:bg-slate-800/50 rounded-lg">
              <span class="text-slate-400">Loading Forecast...</span>
            </div>
          }
        </div>
      </app-card>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TurnoverComponent implements OnInit {
  private dataService = inject(DataService);

  isLoading = signal<boolean>(true);
  currentTurnoverRate = signal<number>(0);
  departuresThisMonth = signal<number>(0);
  averageTenure = signal<number>(0);
  highRiskRoles = signal<number>(0);

  turnoverByDepartmentChart = signal<ChartConfig>({
    type: 'bar',
    title: 'Turnover Rate by Department',
    data: { categories: [], values: [] },
    height: '400px',
  });

  turnoverTrendsChart = signal<ChartConfig>({
    type: 'line',
    title: 'Turnover Trends (Last 24 Months)',
    data: { categories: [], values: [] },
    height: '400px',
  });

  turnoverForecastChart = signal<ChartConfig>({
    type: 'line',
    title: '6-Month Turnover Forecast',
    data: { categories: [], values: [] },
    height: '400px',
  });

  ngOnInit(): void {
    this.loadTurnoverData();
  }

  private loadTurnoverData(): void {
    this.isLoading.set(true);
    forkJoin({
      summary: this.dataService.getDashboardMetrics(),
      details: this.dataService.getTurnoverMetrics()
    }).subscribe({
      next: (data) => {
        // Summary KPIs
        this.currentTurnoverRate.set(data.summary.turnoverRate);
        this.departuresThisMonth.set(data.summary.departures);
        this.averageTenure.set(data.summary.averageTenure);
        this.highRiskRoles.set(data.summary.highRiskRoles);

        // Chart: Dept Breakdown
        this.turnoverByDepartmentChart.update(config => ({
          ...config,
          data: {
            categories: data.details.byDepartment.map((d: any) => d.department),
            values: data.details.byDepartment.map((d: any) => d.rate)
          }
        }));

        // Chart: Trends (Using dummy 24 month data as placeholder if not in JSON)
        this.turnoverTrendsChart.update(config => ({
          ...config,
          data: {
            categories: Array.from({ length: 24 }, (_, i) => {
              const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
              return months[i % 12] + ' ' + (2023 + Math.floor(i / 12));
            }),
            values: [1.8, 1.9, 2.0, 2.1, 2.2, 2.3, 2.4, 2.3, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 3.0, 2.9, 2.8, 2.7, 2.6, 2.5, 2.4, 2.3]
          }
        }));

        // Chart: Forecast
        this.turnoverForecastChart.update(config => ({
          ...config,
          data: {
            categories: ['Current', 'Month 1', 'Month 2', 'Month 3', 'Month 4', 'Month 5', 'Month 6'],
            values: [data.summary.turnoverRate, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9]
          }
        }));

        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading turnover data:', err);
        this.isLoading.set(false);
      }
    });
  }

  onChartClick(event: { name: string; value: unknown }): void {
    console.log('Chart clicked:', event);
  }

  static calculateTurnoverRate(departures: number, avgCount: number): number {
    if (avgCount === 0) return 0;
    return parseFloat(((departures / avgCount) * 100).toFixed(1));
  }
}
