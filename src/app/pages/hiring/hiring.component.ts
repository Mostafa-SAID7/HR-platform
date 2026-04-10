import {
  Component,
  OnInit,
  ChangeDetectionStrategy,
  signal,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardComponent, KPICardComponent } from '../../components';
import { ChartComponent, type ChartConfig } from '../../components/chart/chart.component';
import { BadgeComponent } from '../../components/badge/badge.component';
import { InputComponent } from '../../components/input/input.component';
import { SelectComponent, type SelectOption } from '../../components/select/select.component';
import { DatePickerComponent } from '../../components/date-picker/date-picker.component';
import { DataService } from '../../services/data.service';
import { forkJoin } from 'rxjs';
import { fadeIn, slideInUp } from '../../shared/animations';

/**
 * Hiring Intelligence Component
 *
 * Upgraded from HiringForecastComponent. Now uses premium reusable
 * input components (InputComponent, SelectComponent, DatePickerComponent)
 * for a fully consistent, high-fidelity UI.
 *
 * Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6
 */
@Component({
  selector: 'app-hiring',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CardComponent,
    KPICardComponent,
    ChartComponent,
    BadgeComponent,
    InputComponent,
    SelectComponent,
    DatePickerComponent,
  ],
  animations: [fadeIn, slideInUp],
  template: `
    <div class="page-container" [@fadeIn]>

      <!-- Page Header -->
      <div class="flex flex-col lg:flex-row lg:items-end justify-between gap-6" [@slideInUp]>
        <div class="space-y-2">
          <p class="text-xs font-black text-indigo-500 dark:text-indigo-400 uppercase tracking-[0.25em]">
            Talent Acquisition Intelligence
          </p>
          <h1 class="text-4xl font-black text-slate-900 dark:text-white tracking-tighter">
            Hiring Intelligence
          </h1>
          <p class="text-slate-500 dark:text-slate-400 text-base font-medium leading-relaxed max-w-2xl">
            Forecast future talent requirements based on turnover velocity, growth targets, and historical patterns.
          </p>
        </div>

        <div *ngIf="isLoading()" class="flex items-center gap-3 bg-indigo-50 dark:bg-indigo-900/40 px-6 py-3 rounded-2xl border border-indigo-100 dark:border-indigo-800/30 shrink-0">
          <div class="w-2 h-2 bg-indigo-600 rounded-full animate-ping"></div>
          <span class="text-sm font-black text-indigo-600 dark:text-indigo-400 uppercase tracking-widest">Running Simulation...</span>
        </div>
      </div>

      <!-- Scenario Configuration Card -->
      <app-card [@slideInUp]>
        <div appCardHeader class="flex items-center gap-3">
          <div class="w-1.5 h-8 bg-indigo-600 rounded-full"></div>
          <div>
            <h2 class="text-xl font-black text-slate-900 dark:text-white tracking-tight">Scenario Configuration</h2>
            <p class="text-xs font-medium text-slate-400 mt-0.5">Adjust parameters to model different hiring scenarios</p>
          </div>
        </div>

        <div class="pt-2 pb-4">
          <!-- Row 1: Forecast Window + Department -->
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-6">

            <!-- Forecast Window (Date Range) -->
            <div class="lg:col-span-1">
              <label class="block text-xs font-black text-slate-400 uppercase tracking-widest mb-2">
                Forecast Window
              </label>
              <app-date-picker
                id="forecast-window"
                [range]="true"
                [minDate]="today"
                (dateChange)="onForecastDateChange($event)"
              ></app-date-picker>
            </div>

            <!-- Department Filter -->
            <div>
              <label class="block text-xs font-black text-slate-400 uppercase tracking-widest mb-2">
                Department Focus
              </label>
              <app-select
                id="dept-filter"
                placeholder="All Departments"
                [options]="departmentOptions"
                [searchable]="true"
                (selectionChange)="onDepartmentChange($event)"
              ></app-select>
            </div>

            <!-- Scenario Preset -->
            <div>
              <label class="block text-xs font-black text-slate-400 uppercase tracking-widest mb-2">
                Scenario Preset
              </label>
              <app-select
                id="scenario-preset"
                placeholder="Custom"
                [options]="scenarioOptions"
                (selectionChange)="onScenarioChange($event)"
              ></app-select>
            </div>
          </div>

          <!-- Divider -->
          <div class="border-t border-slate-100 dark:border-slate-800 mb-6"></div>

          <!-- Row 2: Numeric Parameters + Trigger -->
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 items-end">

            <!-- Growth Rate -->
            <div>
              <label class="block text-xs font-black text-slate-400 uppercase tracking-widest mb-2">
                Growth Rate (%)
              </label>
              <app-input
                id="growth-rate"
                type="number"
                placeholder="e.g. 5.0"
                [value]="growthRateStr"
                (valueChange)="onGrowthRateChange($event)"
                ariaLabel="Growth rate percentage"
              ></app-input>
            </div>

            <!-- Turnover Assumption -->
            <div>
              <label class="block text-xs font-black text-slate-400 uppercase tracking-widest mb-2">
                Turnover Assumption (%)
              </label>
              <app-input
                id="turnover-assumption"
                type="number"
                placeholder="e.g. 2.3"
                [value]="turnoverStr"
                (valueChange)="onTurnoverChange($event)"
                ariaLabel="Turnover assumption percentage"
              ></app-input>
            </div>

            <!-- Planned Departures -->
            <div>
              <label class="block text-xs font-black text-slate-400 uppercase tracking-widest mb-2">
                Planned Departures
              </label>
              <app-input
                id="planned-departures"
                type="number"
                placeholder="e.g. 45"
                [value]="departuresStr"
                (valueChange)="onDeparturesChange($event)"
                ariaLabel="Number of planned departures"
              ></app-input>
            </div>

            <!-- CTA Button -->
            <button
              (click)="recalculateForecast()"
              id="model-forecast-btn"
              class="w-full h-11 bg-indigo-600 text-white rounded-xl hover:bg-indigo-700 active:scale-[0.98] transition-all font-black uppercase tracking-widest text-xs shadow-lg shadow-indigo-500/25 hover:-translate-y-0.5"
            >
              ⚡ Model Forecast
            </button>
          </div>
        </div>
      </app-card>

      <!-- KPI Cards -->
      <div class="card-grid" [@slideInUp]>
        <app-kpi-card
          [data]="{
            label: 'Total Predicted Hires (12M)',
            value: totalPredictedHires(),
            trend: 'up',
            unit: 'Talents',
            loading: isLoading()
          }"
        ></app-kpi-card>

        <app-kpi-card
          [data]="{
            label: 'Average Monthly Pace',
            value: averageMonthlyHires(),
            trend: 'neutral',
            unit: '/mo',
            loading: isLoading()
          }"
        ></app-kpi-card>

        <app-kpi-card
          [data]="{
            label: 'Critical Roles at Risk',
            value: criticalRolesAtRisk(),
            trend: 'down',
            loading: isLoading()
          }"
        ></app-kpi-card>

        <app-kpi-card
          [data]="{
            label: 'Forecast Confidence',
            value: averageConfidenceLevel(),
            unit: '%',
            loading: isLoading()
          }"
        ></app-kpi-card>
      </div>

      <!-- Charts Row -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-8" [@slideInUp]>

        <!-- Departmental Demand -->
        <app-card>
          <div appCardHeader class="flex items-center justify-between">
            <div class="flex items-center gap-3">
              <div class="w-1.5 h-6 bg-indigo-600 rounded-full"></div>
              <h2 class="text-xl font-black text-slate-900 dark:text-white tracking-tight">Departmental Demand</h2>
            </div>
            <app-badge variant="info">Next 12M</app-badge>
          </div>
          <div class="py-4">
            @defer (on viewport) {
              <app-chart
                [chartConfig]="hiringForecastByDepartmentChart()"
                (chartClick)="onChartClick($event)"
              ></app-chart>
            } @placeholder {
              <div class="h-[340px] flex items-center justify-center bg-slate-50/50 dark:bg-slate-900/20 rounded-2xl border border-dashed border-slate-200 dark:border-slate-700">
                <span class="text-xs font-bold text-slate-400 uppercase tracking-widest animate-pulse">Modeling Demand...</span>
              </div>
            }
          </div>
        </app-card>

        <!-- Strategic Role Gap -->
        <app-card>
          <div appCardHeader class="flex items-center justify-between">
            <div class="flex items-center gap-3">
              <div class="w-1.5 h-6 bg-emerald-500 rounded-full"></div>
              <h2 class="text-xl font-black text-slate-900 dark:text-white tracking-tight">Strategic Role Gap</h2>
            </div>
            <app-badge variant="info">Role Focus</app-badge>
          </div>
          <div class="py-4">
            @defer (on viewport) {
              <app-chart
                [chartConfig]="hiringForecastByRoleChart()"
                (chartClick)="onChartClick($event)"
              ></app-chart>
            } @placeholder {
              <div class="h-[340px] flex items-center justify-center bg-slate-50/50 dark:bg-slate-900/20 rounded-2xl border border-dashed border-slate-200 dark:border-slate-700">
                <span class="text-xs font-bold text-slate-400 uppercase tracking-widest animate-pulse">Analyzing Role Gaps...</span>
              </div>
            }
          </div>
        </app-card>
      </div>

      <!-- Monthly Trend -->
      <app-card [@slideInUp]>
        <div appCardHeader class="flex items-center justify-between">
          <div class="flex items-center gap-3">
            <div class="w-1.5 h-6 bg-amber-400 rounded-full"></div>
            <h2 class="text-xl font-black text-slate-900 dark:text-white tracking-tight">Accumulative Hiring Trend</h2>
          </div>
          <app-badge variant="success">Steady Pace</app-badge>
        </div>
        <div class="py-4">
          @defer (on viewport) {
            <app-chart
              [chartConfig]="monthlyHiringTrendChart()"
              (chartClick)="onChartClick($event)"
            ></app-chart>
          } @placeholder {
            <div class="h-[340px] flex items-center justify-center bg-slate-50/50 dark:bg-slate-900/20 rounded-2xl border border-dashed border-slate-200 dark:border-slate-700">
              <span class="text-xs font-bold text-slate-400 uppercase tracking-widest animate-pulse">Computing Velocity...</span>
            </div>
          }
        </div>
      </app-card>

      <!-- Intelligence Matrix -->
      <app-card [@slideInUp]>
        <div appCardHeader class="flex items-center gap-3">
          <div class="w-1.5 h-8 bg-slate-400 rounded-full"></div>
          <div>
            <h2 class="text-xl font-black text-slate-900 dark:text-white tracking-tight">Intelligence Matrix</h2>
            <p class="text-xs font-medium text-slate-400 mt-0.5">Key factors influencing this forecast</p>
          </div>
        </div>
        <div class="pb-4">
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">

            <div class="group p-6 bg-slate-50 dark:bg-slate-900/40 border border-slate-200 dark:border-slate-700/50 rounded-2xl transition-all hover:bg-white dark:hover:bg-slate-800 hover:border-indigo-500/30 hover:shadow-lg">
              <div class="w-8 h-8 bg-indigo-100 dark:bg-indigo-900/50 rounded-xl flex items-center justify-center mb-3">
                <span class="text-sm">📊</span>
              </div>
              <p class="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] mb-1">Primary Driver</p>
              <h4 class="text-base font-bold text-slate-900 dark:text-white mb-1">Historical Turnover</h4>
              <p class="text-xs font-medium text-slate-500 dark:text-slate-400 leading-relaxed">
                Average 2.3% monthly velocity based on 24-month horizon
              </p>
            </div>

            <div class="group p-6 bg-slate-50 dark:bg-slate-900/40 border border-slate-200 dark:border-slate-700/50 rounded-2xl transition-all hover:bg-white dark:hover:bg-slate-800 hover:border-emerald-500/30 hover:shadow-lg">
              <div class="w-8 h-8 bg-emerald-100 dark:bg-emerald-900/50 rounded-xl flex items-center justify-center mb-3">
                <span class="text-sm">🚀</span>
              </div>
              <p class="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] mb-1">Expansion</p>
              <h4 class="text-base font-bold text-slate-900 dark:text-white mb-1">Growth Benchmarks</h4>
              <p class="text-xs font-medium text-slate-500 dark:text-slate-400 leading-relaxed">
                Targeting {{ growthRateDisplay }}% Net Expansion over next fiscal year
              </p>
            </div>

            <div class="group p-6 bg-slate-50 dark:bg-slate-900/40 border border-slate-200 dark:border-slate-700/50 rounded-2xl transition-all hover:bg-white dark:hover:bg-slate-800 hover:border-amber-500/30 hover:shadow-lg">
              <div class="w-8 h-8 bg-amber-100 dark:bg-amber-900/50 rounded-xl flex items-center justify-center mb-3">
                <span class="text-sm">📅</span>
              </div>
              <p class="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] mb-1">Capacity</p>
              <h4 class="text-base font-bold text-slate-900 dark:text-white mb-1">Planned Transfers</h4>
              <p class="text-xs font-medium text-slate-500 dark:text-slate-400 leading-relaxed">
                {{ departuresDisplay }} known exits/promotions mapped to cycles
              </p>
            </div>

            <div class="group p-6 bg-slate-50 dark:bg-slate-900/40 border border-slate-200 dark:border-slate-700/50 rounded-2xl transition-all hover:bg-white dark:hover:bg-slate-800 hover:border-purple-500/30 hover:shadow-lg">
              <div class="w-8 h-8 bg-purple-100 dark:bg-purple-900/50 rounded-xl flex items-center justify-center mb-3">
                <span class="text-sm">🌊</span>
              </div>
              <p class="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] mb-1">Volatility</p>
              <h4 class="text-base font-bold text-slate-900 dark:text-white mb-1">Seasonal Flow</h4>
              <p class="text-xs font-medium text-slate-500 dark:text-slate-400 leading-relaxed">
                High-confidence peaks identified in Q2/Q4 hiring cycles
              </p>
            </div>

          </div>
        </div>
      </app-card>

    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HiringComponent implements OnInit {
  private dataService = inject(DataService);

  readonly today = new Date();

  // Loading state
  isLoading = signal<boolean>(true);

  // String intermediaries for app-input (which handles strings)
  growthRateStr = '5.0';
  turnoverStr = '2.3';
  departuresStr = '45';

  // Display values for Intelligence Matrix
  growthRateDisplay = 5.0;
  departuresDisplay = 45;

  // Parsed numeric values
  private growthRate = 5.0;
  private turnoverAssumption = 2.3;
  private plannedDepartures = 45;

  // Forecast dates
  forecastStartDate: Date | null = null;
  forecastEndDate: Date | null = null;

  // KPI Signals
  totalPredictedHires = signal<number>(0);
  averageMonthlyHires = signal<number>(0);
  criticalRolesAtRisk = signal<number>(0);
  averageConfidenceLevel = signal<number>(0);

  // Charts
  hiringForecastByDepartmentChart = signal<ChartConfig>({
    type: 'bar',
    title: '12-Month Hiring Forecast by Department',
    data: { categories: [], values: [] },
    height: '340px',
  });

  hiringForecastByRoleChart = signal<ChartConfig>({
    type: 'bar',
    title: '12-Month Hiring Forecast by Role',
    data: { categories: [], values: [] },
    height: '340px',
  });

  monthlyHiringTrendChart = signal<ChartConfig>({
    type: 'line',
    title: 'Monthly Hiring Trend (12 Months)',
    data: { categories: [], values: [] },
    height: '340px',
  });

  // Select options
  departmentOptions: SelectOption[] = [
    { value: 'all', label: 'All Departments' },
    { value: 'engineering', label: 'Engineering' },
    { value: 'product', label: 'Product' },
    { value: 'operations', label: 'Operations' },
    { value: 'sales', label: 'Sales' },
    { value: 'hr', label: 'Human Resources' },
    { value: 'finance', label: 'Finance' },
    { value: 'marketing', label: 'Marketing' },
  ];

  scenarioOptions: SelectOption[] = [
    { value: 'conservative', label: 'Conservative Growth' },
    { value: 'moderate', label: 'Moderate Growth' },
    { value: 'aggressive', label: 'Aggressive Expansion' },
    { value: 'restructure', label: 'Restructure Mode' },
    { value: 'custom', label: 'Custom' },
  ];

  ngOnInit(): void {
    this.loadHiringData();
  }

  // ─── Input Handlers ────────────────────────────────────────
  onGrowthRateChange(val: string): void {
    const parsed = parseFloat(val);
    if (!isNaN(parsed)) {
      this.growthRate = parsed;
      this.growthRateDisplay = parsed;
    }
  }

  onTurnoverChange(val: string): void {
    const parsed = parseFloat(val);
    if (!isNaN(parsed)) this.turnoverAssumption = parsed;
  }

  onDeparturesChange(val: string): void {
    const parsed = parseInt(val, 10);
    if (!isNaN(parsed)) {
      this.plannedDepartures = parsed;
      this.departuresDisplay = parsed;
    }
  }

  onForecastDateChange(date: Date | [Date, Date]): void {
    if (Array.isArray(date)) {
      this.forecastStartDate = date[0];
      this.forecastEndDate = date[1];
    }
  }

  onDepartmentChange(val: any): void {
    // Could filter chart data by department in a real app
    console.log('Department selected:', val);
  }

  onScenarioChange(val: any): void {
    const presets: Record<string, { growth: number; turnover: number; departures: number }> = {
      conservative: { growth: 2, turnover: 1.5, departures: 20 },
      moderate:     { growth: 5, turnover: 2.3, departures: 45 },
      aggressive:   { growth: 15, turnover: 3.5, departures: 80 },
      restructure:  { growth: -2, turnover: 5.0, departures: 120 },
    };
    const preset = presets[val];
    if (preset) {
      this.growthRateStr = String(preset.growth);
      this.turnoverStr = String(preset.turnover);
      this.departuresStr = String(preset.departures);
      this.growthRate = preset.growth;
      this.growthRateDisplay = preset.growth;
      this.turnoverAssumption = preset.turnover;
      this.plannedDepartures = preset.departures;
      this.departuresDisplay = preset.departures;
    }
  }

  recalculateForecast(): void {
    const total = HiringComponent.calculateTotalHires(
      12000,
      this.growthRate,
      this.turnoverAssumption,
      this.plannedDepartures,
    );
    this.totalPredictedHires.set(total);
    this.averageMonthlyHires.set(Math.round(total / 12));
  }

  static calculateTotalHires(
    count: number,
    growth: number,
    turnover: number,
    planned: number,
  ): number {
    const growthHires = (count * growth) / 100;
    const turnoverHires = (count * turnover) / 100;
    return Math.round(growthHires + turnoverHires + planned);
  }

  onChartClick(event: { name: string; value: unknown }): void {
    console.log('Chart clicked:', event);
  }

  private loadHiringData(): void {
    this.isLoading.set(true);
    forkJoin({
      summary: this.dataService.getDashboardMetrics(),
      forecast: this.dataService.getRecruitmentMetrics(),
    }).subscribe({
      next: (data) => {
        this.totalPredictedHires.set(data.forecast.totalPositions);
        this.averageMonthlyHires.set(Math.round(data.forecast.totalPositions / 12));
        this.criticalRolesAtRisk.set(data.summary.highRiskRoles);
        this.averageConfidenceLevel.set(89);

        this.hiringForecastByDepartmentChart.update((config) => ({
          ...config,
          data: {
            categories: data.forecast.byDepartment.map((d: any) => d.department),
            values: data.forecast.byDepartment.map((d: any) => d.positions),
          },
        }));

        this.hiringForecastByRoleChart.update((config) => ({
          ...config,
          data: {
            categories: data.forecast.byRole.map((r: any) => r.role),
            values: data.forecast.byRole.map((r: any) => r.positions),
          },
        }));

        this.monthlyHiringTrendChart.update((config) => ({
          ...config,
          data: {
            categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
            values: [18, 20, 22, 25, 28, 32, 30, 28, 25, 22, 20, 18],
          },
        }));

        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading hiring data:', err);
        this.isLoading.set(false);
      },
    });
  }
}
