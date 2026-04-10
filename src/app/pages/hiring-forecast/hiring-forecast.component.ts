import { Component, OnInit, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardComponent } from '../../components/card/card.component';
import { ChartComponent, type ChartConfig } from '../../components/chart/chart.component';
import { BadgeComponent } from '../../components/badge/badge.component';
import { DataService } from '../../services/data.service';
import { forkJoin } from 'rxjs';

/**
 * Hiring Forecast Dashboard
 *
 * Displays 12-month hiring forecasts by department and role, with confidence levels,
 * influencing factors, parameter adjustment, and critical role identification.
 *
 * Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6
 */

@Component({
  selector: 'app-hiring-forecast',
  standalone: true,
  imports: [CommonModule, FormsModule, CardComponent, ChartComponent, BadgeComponent],
  template: `
    <div class="space-y-6">
      <!-- Header -->
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-3xl font-bold text-slate-900 dark:text-white">Hiring Forecast</h1>
          <p class="text-slate-600 dark:text-slate-400 mt-2">
            Predict future hiring needs based on turnover, growth, and workforce trends
          </p>
        </div>
        <div *ngIf="isLoading()" class="flex items-center text-indigo-500">
          <svg class="animate-spin h-5 w-5 mr-3" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" fill="none"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          <span class="text-sm font-medium">Running Simulation...</span>
        </div>
      </div>

      <!-- Parameter Adjustment Section -->
      <app-card>
        <div class="space-y-4">
          <h2 class="text-xl font-semibold text-slate-900 dark:text-white">Forecast Parameters</h2>
          <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div>
              <label class="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
                Growth Rate (%)
              </label>
              <input
                type="number"
                [(ngModel)]="growthRate"
                class="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white"
                min="0"
                max="100"
                step="0.1"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
                Turnover Assumption (%)
              </label>
              <input
                type="number"
                [(ngModel)]="turnoverAssumption"
                class="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white"
                min="0"
                max="100"
                step="0.1"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
                Planned Departures
              </label>
              <input
                type="number"
                [(ngModel)]="plannedDepartures"
                class="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white"
                min="0"
                step="1"
              />
            </div>
            <div class="flex items-end">
              <button
                (click)="recalculateForecast()"
                class="w-full px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors font-medium"
              >
                Recalculate
              </button>
            </div>
          </div>
        </div>
      </app-card>

      <!-- Key Metrics -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-6">
        <app-card>
          <div class="text-center">
            <p class="text-sm font-medium text-slate-600 dark:text-slate-400">
              Total Predicted Hires (12M)
            </p>
            <p class="text-3xl font-bold text-indigo-600 dark:text-indigo-400 mt-2">
              {{ totalPredictedHires() }}
            </p>
            <app-badge variant="info" class="mt-3 justify-center">
              Based on current parameters
            </app-badge>
          </div>
        </app-card>

        <app-card>
          <div class="text-center">
            <p class="text-sm font-medium text-slate-600 dark:text-slate-400">
              Average Monthly Hires
            </p>
            <p class="text-3xl font-bold text-emerald-600 dark:text-emerald-400 mt-2">
              {{ averageMonthlyHires() }}
            </p>
            <app-badge variant="success" class="mt-3 justify-center">
              Steady hiring pace
            </app-badge>
          </div>
        </app-card>

        <app-card>
          <div class="text-center">
            <p class="text-sm font-medium text-slate-600 dark:text-slate-400">
              Critical Roles at Risk
            </p>
            <p class="text-3xl font-bold text-red-600 dark:text-red-400 mt-2">
              {{ criticalRolesAtRisk() }}
            </p>
            <app-badge variant="error" class="mt-3 justify-center"> Requires attention </app-badge>
          </div>
        </app-card>

        <app-card>
          <div class="text-center">
            <p class="text-sm font-medium text-slate-600 dark:text-slate-400">
              Average Confidence Level
            </p>
            <p class="text-3xl font-bold text-blue-600 dark:text-blue-400 mt-2">
              {{ averageConfidenceLevel() }}%
            </p>
            <app-badge variant="info" class="mt-3 justify-center">
              High confidence forecast
            </app-badge>
          </div>
        </app-card>
      </div>

      <!-- 12-Month Hiring Forecast by Department -->
      <app-card>
        <div class="space-y-4">
          <div class="flex items-center justify-between">
            <h2 class="text-xl font-semibold text-slate-900 dark:text-white">
              12-Month Hiring Forecast by Department
            </h2>
            <app-badge variant="info">Bar Chart</app-badge>
          </div>
          @defer (on viewport) {
            <app-chart
              [chartConfig]="hiringForecastByDepartmentChart()"
              (chartClick)="onChartClick($event)"
            ></app-chart>
          } @placeholder {
            <div class="h-[400px] flex items-center justify-center bg-slate-50 dark:bg-slate-800/50 rounded-lg">
              <span class="text-slate-400">Loading Department Forecast...</span>
            </div>
          }
        </div>
      </app-card>

      <!-- Hiring Forecast by Role -->
      <app-card>
        <div class="space-y-4">
          <div class="flex items-center justify-between">
            <h2 class="text-xl font-semibold text-slate-900 dark:text-white">
              12-Month Hiring Forecast by Role
            </h2>
            <app-badge variant="info">Bar Chart</app-badge>
          </div>
          @defer (on viewport) {
            <app-chart
              [chartConfig]="hiringForecastByRoleChart()"
              (chartClick)="onChartClick($event)"
            ></app-chart>
          } @placeholder {
            <div class="h-[400px] flex items-center justify-center bg-slate-50 dark:bg-slate-800/50 rounded-lg">
              <span class="text-slate-400">Loading Role Forecast...</span>
            </div>
          }
        </div>
      </app-card>

      <!-- Monthly Hiring Trend -->
      <app-card>
        <div class="space-y-4">
          <div class="flex items-center justify-between">
            <h2 class="text-xl font-semibold text-slate-900 dark:text-white">
              Monthly Hiring Trend (12 Months)
            </h2>
            <app-badge variant="info">Line Chart</app-badge>
          </div>
          @defer (on viewport) {
            <app-chart
              [chartConfig]="monthlyHiringTrendChart()"
              (chartClick)="onChartClick($event)"
            ></app-chart>
          } @placeholder {
            <div class="h-[400px] flex items-center justify-center bg-slate-50 dark:bg-slate-800/50 rounded-lg">
              <span class="text-slate-400">Loading Hiring Trends...</span>
            </div>
          }
        </div>
      </app-card>

      <!-- Influencing Factors -->
      <app-card>
        <div class="space-y-4">
          <h2 class="text-xl font-semibold text-slate-900 dark:text-white">
            Key Influencing Factors
          </h2>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div class="p-3 bg-slate-50 dark:bg-slate-700 rounded-lg">
              <p class="font-semibold text-slate-900 dark:text-white">Historical Turnover</p>
              <p class="text-sm text-slate-600 dark:text-slate-400 mt-1">
                Average 2.3% monthly turnover based on 24-month history
              </p>
            </div>
            <div class="p-3 bg-slate-50 dark:bg-slate-700 rounded-lg">
              <p class="font-semibold text-slate-900 dark:text-white">Growth Projections</p>
              <p class="text-sm text-slate-600 dark:text-slate-400 mt-1">
                Expected {{ growthRate }}% organizational growth over next 12 months
              </p>
            </div>
            <div class="p-3 bg-slate-50 dark:bg-slate-700 rounded-lg">
              <p class="font-semibold text-slate-900 dark:text-white">Planned Departures</p>
              <p class="text-sm text-slate-600 dark:text-slate-400 mt-1">
                {{ plannedDepartures }} planned departures identified
              </p>
            </div>
            <div class="p-3 bg-slate-50 dark:bg-slate-700 rounded-lg">
              <p class="font-semibold text-slate-900 dark:text-white">Seasonal Patterns</p>
              <p class="text-sm text-slate-600 dark:text-slate-400 mt-1">
                Higher turnover in Q2 and Q4 based on historical data
              </p>
            </div>
          </div>
        </div>
      </app-card>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HiringForecastComponent implements OnInit {
  private dataService = inject(DataService);

  isLoading = signal<boolean>(true);
  growthRate = 5.0;
  turnoverAssumption = 2.3;
  plannedDepartures = 45;

  totalPredictedHires = signal<number>(0);
  averageMonthlyHires = signal<number>(0);
  criticalRolesAtRisk = signal<number>(0);
  averageConfidenceLevel = signal<number>(0);

  hiringForecastByDepartmentChart = signal<ChartConfig>({
    type: 'bar',
    title: '12-Month Hiring Forecast by Department',
    data: { categories: [], values: [] },
    height: '400px',
  });

  hiringForecastByRoleChart = signal<ChartConfig>({
    type: 'bar',
    title: '12-Month Hiring Forecast by Role',
    data: { categories: [], values: [] },
    height: '400px',
  });

  monthlyHiringTrendChart = signal<ChartConfig>({
    type: 'line',
    title: 'Monthly Hiring Trend (12 Months)',
    data: { categories: [], values: [] },
    height: '400px',
  });

  ngOnInit(): void {
    this.loadHiringForecastData();
  }

  private loadHiringForecastData(): void {
    this.isLoading.set(true);
    forkJoin({
      summary: this.dataService.getDashboardMetrics(),
      forecast: this.dataService.getRecruitmentMetrics()
    }).subscribe({
      next: (data) => {
        // SUMMARY KPIs (Hiring forecast specific)
        this.totalPredictedHires.set(data.forecast.totalPositions);
        this.averageMonthlyHires.set(Math.round(data.forecast.totalPositions / 12));
        this.criticalRolesAtRisk.set(data.summary.highRiskRoles);
        this.averageConfidenceLevel.set(89); // Placeholder confidence

        // Chart: Dept Forecast
        this.hiringForecastByDepartmentChart.update(config => ({
          ...config,
          data: {
            categories: data.forecast.byDepartment.map((d: any) => d.department),
            values: data.forecast.byDepartment.map((d: any) => d.positions)
          }
        }));

        // Chart: Role Forecast
        this.hiringForecastByRoleChart.update(config => ({
          ...config,
          data: {
            categories: data.forecast.byRole.map((r: any) => r.role),
            values: data.forecast.byRole.map((r: any) => r.positions)
          }
        }));

        // Chart: Trends (Placeholder trend data)
        this.monthlyHiringTrendChart.update(config => ({
          ...config,
          data: {
            categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
            values: [18, 20, 22, 25, 28, 32, 30, 28, 25, 22, 20, 18]
          }
        }));

        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading hiring forecast:', err);
        this.isLoading.set(false);
      }
    });
  }

  recalculateForecast(): void {
    const total = HiringForecastComponent.calculateTotalHires(
      12000,
      this.growthRate,
      this.turnoverAssumption,
      this.plannedDepartures
    );
    this.totalPredictedHires.set(total);
    this.averageMonthlyHires.set(Math.round(total / 12));
  }

  static calculateTotalHires(
    count: number,
    growth: number,
    turnover: number,
    planned: number
  ): number {
    const growthHires = (count * growth) / 100;
    const turnoverHires = (count * turnover) / 100;
    return Math.round(growthHires + turnoverHires + planned);
  }

  onChartClick(event: { name: string; value: unknown }): void {
    console.log('Chart clicked:', event);
  }
}
