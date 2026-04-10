import { Component, OnInit, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../components/card/card.component';
import { ChartComponent, type ChartConfig } from '../../components/chart/chart.component';
import { BadgeComponent } from '../../components/badge/badge.component';
import { DataService } from '../../services/data.service';
import { forkJoin } from 'rxjs';

/**
 * Performance Analytics Dashboard
 *
 * Displays employee performance metrics including distribution, trends, ratings,
 * performance vs salary analysis, and departmental comparisons.
 *
 * Requirements: 1.1, 1.2, 1.6, 2.1
 */

@Component({
  selector: 'app-performance',
  standalone: true,
  imports: [CommonModule, CardComponent, ChartComponent, BadgeComponent],
  template: `
    <div class="space-y-6">
      <!-- Header -->
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-3xl font-bold text-slate-900 dark:text-white">Performance Analytics</h1>
          <p class="text-slate-600 dark:text-slate-400 mt-2">
            Track employee performance metrics, trends, and departmental comparisons
          </p>
        </div>
        <div *ngIf="isLoading()" class="flex items-center text-indigo-500">
          <svg class="animate-spin h-5 w-5 mr-3" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" fill="none"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          <span class="text-sm font-medium">Loading Analysis...</span>
        </div>
      </div>

      <!-- Performance Distribution Histogram -->
      <app-card>
        <div class="space-y-4">
          <div class="flex items-center justify-between">
            <h2 class="text-xl font-semibold text-slate-900 dark:text-white">
              Performance Distribution
            </h2>
            <app-badge variant="info">Histogram</app-badge>
          </div>
          @defer (on viewport) {
            <app-chart
              [chartConfig]="performanceDistributionChart()"
              (chartClick)="onChartClick($event)"
            ></app-chart>
          } @placeholder {
            <div class="h-[400px] flex items-center justify-center bg-slate-50 dark:bg-slate-800/50 rounded-lg">
              <span class="text-slate-400">Loading Distribution Chart...</span>
            </div>
          }
        </div>
      </app-card>

      <!-- Performance Trends Over Time -->
      <app-card>
        <div class="space-y-4">
          <div class="flex items-center justify-between">
            <h2 class="text-xl font-semibold text-slate-900 dark:text-white">
              Performance Trends (12 Months)
            </h2>
            <app-badge variant="success">↑ 3.2% improvement</app-badge>
          </div>
          @defer (on viewport) {
            <app-chart
              [chartConfig]="performanceTrendsChart()"
              (chartClick)="onChartClick($event)"
            ></app-chart>
          } @placeholder {
            <div class="h-[400px] flex items-center justify-center bg-slate-50 dark:bg-slate-800/50 rounded-lg">
              <span class="text-slate-400">Loading Trends Chart...</span>
            </div>
          }
        </div>
      </app-card>

      <!-- Two Column Layout -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Performance Rating Breakdown -->
        <app-card>
          <div class="space-y-4">
            <div class="flex items-center justify-between">
              <h2 class="text-xl font-semibold text-slate-900 dark:text-white">Rating Breakdown</h2>
              <app-badge variant="info">Pie Chart</app-badge>
            </div>
            @defer (on viewport) {
              <app-chart
                [chartConfig]="performanceRatingChart()"
                (chartClick)="onChartClick($event)"
              ></app-chart>
            } @placeholder {
              <div class="h-[400px] flex items-center justify-center bg-slate-50 dark:bg-slate-800/50 rounded-lg">
                <span class="text-slate-400">Loading Rating Chart...</span>
              </div>
            }
          </div>
        </app-card>

        <!-- Performance vs Salary Analysis -->
        <app-card>
          <div class="space-y-4">
            <div class="flex items-center justify-between">
              <h2 class="text-xl font-semibold text-slate-900 dark:text-white">
                Performance vs Salary
              </h2>
              <app-badge variant="info">Scatter Plot</app-badge>
            </div>
            @defer (on viewport) {
              <app-chart
                [chartConfig]="performanceVsSalaryChart()"
                (chartClick)="onChartClick($event)"
              ></app-chart>
            } @placeholder {
              <div class="h-[400px] flex items-center justify-center bg-slate-50 dark:bg-slate-800/50 rounded-lg">
                <span class="text-slate-400">Loading Analysis Chart...</span>
              </div>
            }
          </div>
        </app-card>
      </div>

      <!-- Performance Comparison by Department -->
      <app-card>
        <div class="space-y-4">
          <div class="flex items-center justify-between">
            <h2 class="text-xl font-semibold text-slate-900 dark:text-white">
              Performance by Department
            </h2>
            <app-badge variant="info">Bar Chart</app-badge>
          </div>
          @defer (on viewport) {
            <app-chart
              [chartConfig]="departmentComparisonChart()"
              (chartClick)="onChartClick($event)"
            ></app-chart>
          } @placeholder {
            <div class="h-[400px] flex items-center justify-center bg-slate-50 dark:bg-slate-800/50 rounded-lg">
              <span class="text-slate-400">Loading Department Chart...</span>
            </div>
          }
        </div>
      </app-card>

      <!-- Performance Metrics Summary -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-6">
        <app-card>
          <div class="text-center">
            <p class="text-sm font-medium text-slate-600 dark:text-slate-400">
              Average Performance Score
            </p>
            <p class="text-3xl font-bold text-indigo-600 dark:text-indigo-400 mt-2">
              {{ averagePerformanceScore() }}
            </p>
            <app-badge variant="success" class="mt-3 justify-center">
              ↑ 2.1% from last quarter
            </app-badge>
          </div>
        </app-card>

        <app-card>
          <div class="text-center">
            <p class="text-sm font-medium text-slate-600 dark:text-slate-400">High Performers</p>
            <p class="text-3xl font-bold text-emerald-600 dark:text-emerald-400 mt-2">
              {{ highPerformersCount() }}
            </p>
            <app-badge variant="success" class="mt-3 justify-center">
              ↑ 5.3% from last quarter
            </app-badge>
          </div>
        </app-card>

        <app-card>
          <div class="text-center">
            <p class="text-sm font-medium text-slate-600 dark:text-slate-400">Needs Improvement</p>
            <p class="text-3xl font-bold text-yellow-600 dark:text-yellow-400 mt-2">
              {{ needsImprovementCount() }}
            </p>
            <app-badge variant="warning" class="mt-3 justify-center">
              ↓ 1.2% from last quarter
            </app-badge>
          </div>
        </app-card>

        <app-card>
          <div class="text-center">
            <p class="text-sm font-medium text-slate-600 dark:text-slate-400">
              Performance Reviews Completed
            </p>
            <p class="text-3xl font-bold text-blue-600 dark:text-blue-400 mt-2">
              {{ reviewsCompletedPercentage() }}%
            </p>
            <app-badge variant="info" class="mt-3 justify-center">
              ↑ 8.5% from last quarter
            </app-badge>
          </div>
        </app-card>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PerformanceComponent implements OnInit {
  private dataService = inject(DataService);

  isLoading = signal<boolean>(true);
  averagePerformanceScore = signal<number>(0);
  highPerformersCount = signal<number>(0);
  needsImprovementCount = signal<number>(0);
  reviewsCompletedPercentage = signal<number>(0);

  performanceDistributionChart = signal<ChartConfig>({
    type: 'bar',
    title: 'Performance Score Distribution',
    data: { categories: [], values: [] },
    height: '400px',
  });

  performanceTrendsChart = signal<ChartConfig>({
    type: 'line',
    title: 'Performance Trends (Last 12 Months)',
    data: { categories: [], values: [] },
    height: '400px',
  });

  performanceRatingChart = signal<ChartConfig>({
    type: 'pie',
    title: 'Performance Rating Distribution',
    data: { values: [] },
    height: '400px',
  });

  performanceVsSalaryChart = signal<ChartConfig>({
    type: 'scatter',
    title: 'Performance vs Salary Analysis',
    data: { values: [] },
    options: {
      xAxis: { name: 'Salary ($)', type: 'value' },
      yAxis: { name: 'Performance Score', type: 'value' },
    },
    height: '400px',
  });

  departmentComparisonChart = signal<ChartConfig>({
    type: 'bar',
    title: 'Average Performance Score by Department',
    data: { categories: [], values: [] },
    height: '400px',
  });

  ngOnInit(): void {
    this.loadPerformanceData();
  }

  private loadPerformanceData(): void {
    this.isLoading.set(true);
    forkJoin({
      summary: this.dataService.getDashboardMetrics(),
      details: this.dataService.getPerformanceMetrics(),
      employees: this.dataService.getEmployees()
    }).subscribe({
      next: (data) => {
        // Summary KPIs
        this.averagePerformanceScore.set(data.summary.averagePerformance);
        this.highPerformersCount.set(data.summary.highPerformers);
        this.needsImprovementCount.set(data.summary.needsImprovement);
        this.reviewsCompletedPercentage.set(data.summary.reviewsCompleted);

        // Chart: Distribution
        this.performanceDistributionChart.update(config => ({
          ...config,
          data: {
            categories: data.details.distribution.map((d: any) => d.range),
            values: data.details.distribution.map((d: any) => d.count)
          }
        }));

        // Chart: Trends
        this.performanceTrendsChart.update(config => ({
          ...config,
          data: {
            categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
            values: [68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79] // Keep dummy trends if not in JSON or fetch from details
          }
        }));

        // Chart: Rating Breakdown
        this.performanceRatingChart.update(config => ({
          ...config,
          data: {
            values: data.details.distribution.map((d: any) => ({
              name: d.range,
              value: d.count
            }))
          }
        }));

        // Chart: Performance vs Salary (Derived from employee data)
        const scatterData = data.employees.map((e: any) => [e.salary, e.performanceScore]);
        this.performanceVsSalaryChart.update(config => ({
          ...config,
          data: { values: scatterData }
        }));

        // Chart: Dept Comparison
        this.departmentComparisonChart.update(config => ({
          ...config,
          data: {
            categories: data.details.byDepartment.map((d: any) => d.department),
            values: data.details.byDepartment.map((d: any) => d.score)
          }
        }));

        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading performance data:', err);
        this.isLoading.set(false);
      }
    });
  }

  onChartClick(event: { name: string; value: unknown }): void {
    console.log('Chart clicked:', event);
  }
}
