import { Component, OnInit, signal, inject, computed, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { forkJoin } from 'rxjs';
import { CardComponent, ChartComponent, BadgeComponent, ChartConfig, KPICardComponent } from '../../components';
import { DataService } from '../../services/data.service';
import { fadeIn, slideInUp } from '../../shared/animations';

@Component({
  selector: 'app-performance',
  standalone: true,
  imports: [CommonModule, CardComponent, ChartComponent, BadgeComponent, KPICardComponent],
  animations: [fadeIn, slideInUp],
  template: `
    <div class="page-container" [@fadeIn]>
      <!-- Balanced Page Header -->
      <div class="flex flex-col lg:flex-row lg:items-end justify-between gap-8 mb-6" [@slideInUp]>
        <div class="space-y-2">
          <h1 class="text-4xl font-black text-slate-900 dark:text-white tracking-tighter">
            Performance Analytics
          </h1>
          <p class="text-slate-500 dark:text-slate-400 text-lg font-medium leading-relaxed max-w-2xl">
            Deep dive into organizational efficiency, individual growth trends, and departmental benchmarking.
          </p>
        </div>
        
        <div *ngIf="isLoading()" class="flex items-center gap-3 bg-indigo-50 dark:bg-indigo-900/40 px-6 py-3 rounded-2xl border border-indigo-100 dark:border-indigo-800/30">
          <div class="w-2 h-2 bg-indigo-600 rounded-full animate-ping"></div>
          <span class="text-sm font-bold text-indigo-600 dark:text-indigo-400 uppercase tracking-widest">Analyzing Engine...</span>
        </div>
      </div>

      <!-- KPI Summary Cards -->
      <div class="card-grid" [@slideInUp]>
        <app-kpi-card
          [data]="{
            label: 'Avg Performance',
            value: averagePerformanceScore(),
            previousValue: 68.2,
            trend: 'up',
            trendPercentage: 2.1,
            loading: isLoading()
          }"
        ></app-kpi-card>

        <app-kpi-card
          [data]="{
            label: 'High Performers',
            value: highPerformersCount(),
            previousValue: 120,
            trend: 'up',
            trendPercentage: 5.3,
            loading: isLoading()
          }"
        ></app-kpi-card>

        <app-kpi-card
          [data]="{
            label: 'Needs Review',
            value: needsImprovementCount(),
            previousValue: 12,
            trend: 'neutral',
            loading: isLoading()
          }"
        ></app-kpi-card>

        <app-kpi-card
          [data]="{
            label: 'Review Rate',
            value: reviewsCompletedPercentage(),
            unit: '%',
            trend: 'neutral',
            loading: isLoading()
          }"
        ></app-kpi-card>
      </div>

      <!-- Main Visualizations Section -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8 mt-10" [@slideInUp]>
        <!-- Large Chart: Distribution -->
        <div class="lg:col-span-2">
          <app-card>
            <div appCardHeader class="flex items-center justify-between">
              <div class="flex items-center gap-3">
                 <div class="w-1.5 h-6 bg-indigo-600 rounded-full"></div>
                 <h2 class="text-xl font-black text-slate-900 dark:text-white tracking-tight">Performance Distribution</h2>
              </div>
              <app-badge variant="info">Workforce Spread</app-badge>
            </div>
            <div class="py-4">
              @defer (on viewport) {
                <app-chart
                  [chartConfig]="performanceDistributionChart()"
                  (chartClick)="onChartClick($event)"
                ></app-chart>
              } @placeholder {
                <div class="h-[400px] flex items-center justify-center bg-slate-50/50 dark:bg-slate-900/20 rounded-2xl border border-dashed border-slate-200 dark:border-slate-700">
                  <div class="flex flex-col items-center gap-3">
                    <div class="w-8 h-8 border-4 border-indigo-500 border-t-transparent rounded-full animate-spin"></div>
                    <span class="text-xs font-bold text-slate-400 uppercase tracking-widest">Synthesizing...</span>
                  </div>
                </div>
              }
            </div>
          </app-card>
        </div>

        <!-- Side Chart: Breakdown -->
        <div class="lg:col-span-1">
          <app-card class="h-full">
            <div appCardHeader class="flex items-center gap-3">
              <div class="w-1.5 h-6 bg-emerald-500 rounded-full"></div>
              <h2 class="text-xl font-black text-slate-900 dark:text-white tracking-tight">Rating Split</h2>
            </div>
            <div class="py-4">
              @defer (on viewport) {
                <app-chart
                  [chartConfig]="performanceRatingChart()"
                  (chartClick)="onChartClick($event)"
                ></app-chart>
              } @placeholder {
                <div class="h-[400px] flex items-center justify-center bg-slate-50/50 dark:bg-slate-900/20 rounded-2xl border border-dashed border-slate-200 dark:border-slate-700">
                   <span class="text-xs font-bold text-slate-400 uppercase tracking-widest animate-pulse">Computing...</span>
                </div>
              }
            </div>
          </app-card>
        </div>
      </div>

      <!-- Secondary Data Layer -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-8 mt-8" [@slideInUp]>
        <!-- Trend Analysis -->
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
                [chartConfig]="performanceTrendsChart()"
                (chartClick)="onChartClick($event)"
              ></app-chart>
            } @placeholder {
              <div class="h-[400px] flex items-center justify-center bg-slate-50/50 dark:bg-slate-900/20 rounded-2xl border border-dashed border-slate-200 dark:border-slate-700">
                <span class="text-xs font-bold text-slate-400 uppercase tracking-widest animate-pulse">Mapping Momentum...</span>
              </div>
            }
          </div>
        </app-card>

        <!-- Correlation Analysis -->
        <app-card>
          <div appCardHeader class="flex items-center justify-between">
            <div class="flex items-center gap-3">
               <div class="w-1.5 h-6 bg-amber-400 rounded-full"></div>
               <h2 class="text-xl font-black text-slate-900 dark:text-white tracking-tight">Performance vs Salary</h2>
            </div>
            <app-badge variant="info">Correlation</app-badge>
          </div>
          <div class="py-4">
            @defer (on viewport) {
              <app-chart
                [chartConfig]="performanceVsSalaryChart()"
                (chartClick)="onChartClick($event)"
              ></app-chart>
            } @placeholder {
              <div class="h-[400px] flex items-center justify-center bg-slate-50/50 dark:bg-slate-900/20 rounded-2xl border border-dashed border-slate-200 dark:border-slate-700">
                <span class="text-xs font-bold text-slate-400 uppercase tracking-widest animate-pulse">Analyzing Correlations...</span>
              </div>
            }
          </div>
        </app-card>
      </div>

      <!-- Departmental Benchmarking -->
      <app-card class="mt-8" [@slideInUp]>
        <div appCardHeader class="flex items-center gap-3">
          <div class="w-1.5 h-6 bg-slate-400 rounded-full"></div>
          <h2 class="text-xl font-black text-slate-900 dark:text-white tracking-tight">Departmental Benchmarking</h2>
        </div>
        <div class="py-4">
          @defer (on viewport) {
            <app-chart
              [chartConfig]="departmentComparisonChart()"
              (chartClick)="onChartClick($event)"
            ></app-chart>
          } @placeholder {
            <div class="h-[400px] flex items-center justify-center bg-slate-50/50 dark:bg-slate-900/20 rounded-2xl border border-dashed border-slate-200 dark:border-slate-700">
              <span class="text-xs font-bold text-slate-400 uppercase tracking-widest animate-pulse">Benchmarking Departments...</span>
            </div>
          }
        </div>
      </app-card>
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
        this.performanceDistributionChart.update((config: ChartConfig) => ({
          ...config,
          data: {
            categories: data.details.distribution.map((d: any) => d.range),
            values: data.details.distribution.map((d: any) => d.count)
          }
        }));

        // Chart: Trends
        this.performanceTrendsChart.update((config: ChartConfig) => ({
          ...config,
          data: {
            categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
            values: [68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79] // Keep dummy trends if not in JSON or fetch from details
          }
        }));

        // Chart: Rating Breakdown
        this.performanceRatingChart.update((config: ChartConfig) => ({
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
        this.performanceVsSalaryChart.update((config: ChartConfig) => ({
          ...config,
          data: { values: scatterData }
        }));

        // Chart: Dept Comparison
        this.departmentComparisonChart.update((config: ChartConfig) => ({
          ...config,
          data: {
            categories: data.details.byDepartment.map((d: any) => d.department),
            values: data.details.byDepartment.map((d: any) => d.score)
          }
        }));

        this.isLoading.set(false);
      },
      error: (err: any) => {
        console.error('Error loading performance data:', err);
        this.isLoading.set(false);
      }
    });
  }

  onChartClick(event: { name: string; value: unknown }): void {
    console.log('Chart clicked:', event);
  }
}
