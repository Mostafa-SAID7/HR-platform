import { Component, OnInit, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardComponent } from '../../components/card/card.component';
import { BadgeComponent } from '../../components/badge/badge.component';
import { ButtonComponent } from '../../components/button/button.component';
import { ReportService, type ReportConfig, type ReportMetric } from '../../services/report.service';
import { DataService } from '../../services/data.service';
import { fadeIn, slideInUp } from '../../shared/animations';

/**
 * Report Generation and Export Page
 *
 * Allows users to build custom reports, select metrics, date ranges, and filters,
 * then export in multiple formats (PDF, CSV, Excel) with multi-language support.
 *
 * Requirements: 24.1, 24.2, 24.3, 24.4, 26.1, 26.2
 */

interface ReportTemplate {
  id: string;
  name: string;
  description: string;
  metrics: ReportMetric[];
}

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CardComponent,
    BadgeComponent,
  ],
  animations: [fadeIn, slideInUp],
  template: `
    <div class="page-container" [@fadeIn]>
      <!-- Balanced Page Header -->
      <div class="flex flex-col lg:flex-row lg:items-end justify-between gap-8 mb-4" [@slideInUp]>
        <div class="space-y-2">
          <h1 class="text-4xl font-black text-slate-900 dark:text-white tracking-tighter">
            Intelligence Export
          </h1>
          <p class="text-slate-500 dark:text-slate-400 text-lg font-medium leading-relaxed max-w-2xl">
            Surgical reporting tools for data-driven precision. Export insights in executive-ready formats.
          </p>
        </div>
        
        <div *ngIf="isLoading()" class="flex items-center gap-3 bg-indigo-50 dark:bg-indigo-900/40 px-6 py-3 rounded-2xl border border-indigo-100 dark:border-indigo-800/30">
          <div class="w-2 h-2 bg-indigo-600 rounded-full animate-ping"></div>
          <span class="text-sm font-bold text-indigo-600 dark:text-indigo-400 uppercase tracking-widest">Fetching Templates...</span>
        </div>
      </div>

      <!-- Report Builder -->
      <div class="grid grid-cols-1 xl:grid-cols-3 gap-8" [@slideInUp]>
        <div class="xl:col-span-2">
          <app-card>
            <div appCardHeader class="flex items-center justify-between">
              <div class="flex items-center gap-3">
                <div class="w-1.5 h-6 bg-indigo-600 rounded-full"></div>
                <h2 class="text-xl font-black text-slate-900 dark:text-white tracking-tight">Intelligence Configurator</h2>
              </div>
              <div class="flex items-center gap-2 px-3 py-1 bg-indigo-50 dark:bg-indigo-900/40 rounded-full border border-indigo-100 dark:border-indigo-800/30">
                <div class="w-1.5 h-1.5 bg-indigo-500 rounded-full animate-pulse"></div>
                <span class="text-[10px] font-black text-indigo-600 dark:text-indigo-400 uppercase tracking-widest">Live Engine</span>
              </div>
            </div>
            
            <div class="py-8 space-y-12">
              <!-- Logic foundation -->
              <div class="space-y-6">
                <div class="flex items-center gap-4 mb-4">
                  <span class="h-px flex-grow bg-slate-100 dark:bg-slate-800/50"></span>
                  <span class="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em]">Foundation</span>
                  <span class="h-px flex-grow bg-slate-100 dark:bg-slate-800/50"></span>
                </div>
                
                <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
                  <div class="form-group-premium">
                    <label class="form-label-premium">Intelligence Logic Template</label>
                    <div class="form-icon-wrapper">
                      <div class="form-icon-inner">
                        <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                        </svg>
                      </div>
                      <select
                        [(ngModel)]="selectedTemplate"
                        (change)="onTemplateChange()"
                        class="form-select-premium form-input-with-icon"
                      >
                        <option value="">-- Manual Logic Override --</option>
                        <option *ngFor="let template of reportTemplates()" [value]="template.id">
                          {{ template.name }}
                        </option>
                      </select>
                    </div>
                  </div>

                  <div class="form-group-premium">
                    <label class="form-label-premium">Custom Report Header</label>
                    <div class="form-icon-wrapper">
                      <div class="form-icon-inner">
                        <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                        </svg>
                      </div>
                      <input
                        type="text"
                        [(ngModel)]="reportTitle"
                        placeholder="Define intelligence header..."
                        class="form-input-premium form-input-with-icon"
                      />
                    </div>
                  </div>
                </div>
              </div>

              <!-- Temporal Scope -->
              <div class="space-y-6">
                <div class="flex items-center gap-4 mb-4">
                  <span class="h-px flex-grow bg-slate-100 dark:bg-slate-800/50"></span>
                  <span class="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em]">Temporal Scope</span>
                  <span class="h-px flex-grow bg-slate-100 dark:bg-slate-800/50"></span>
                </div>

                <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
                  <div class="form-group-premium">
                    <label class="form-label-premium">Analysis Start Point</label>
                    <div class="form-icon-wrapper">
                      <div class="form-icon-inner">
                        <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                        </svg>
                      </div>
                      <input
                        type="date"
                        [(ngModel)]="startDate"
                        class="form-input-premium form-input-with-icon"
                      />
                    </div>
                  </div>
                  <div class="form-group-premium">
                    <label class="form-label-premium">Analysis End Point</label>
                    <div class="form-icon-wrapper">
                      <div class="form-icon-inner">
                        <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                        </svg>
                      </div>
                      <input
                        type="date"
                        [(ngModel)]="endDate"
                        class="form-input-premium form-input-with-icon"
                      />
                    </div>
                  </div>
                </div>
              </div>

              <!-- Strategic Segments -->
              <div class="space-y-6">
                <div class="flex items-center gap-4 mb-4">
                  <span class="h-px flex-grow bg-slate-100 dark:bg-slate-800/50"></span>
                  <span class="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em]">Strategic Segments</span>
                  <span class="h-px flex-grow bg-slate-100 dark:bg-slate-800/50"></span>
                </div>

                <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
                  <div class="form-icon-wrapper">
                    <div class="form-icon-inner">
                      <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                      </svg>
                    </div>
                    <select
                      [(ngModel)]="selectedDepartment"
                      class="form-select-premium form-input-with-icon"
                    >
                      <option value="">Department: Universal</option>
                      <option value="Engineering">Engineering</option>
                      <option value="Sales">Sales</option>
                      <option value="Marketing">Marketing</option>
                      <option value="HR">HR</option>
                      <option value="Finance">Finance</option>
                      <option value="Operations">Operations</option>
                    </select>
                  </div>

                  <div class="form-icon-wrapper">
                    <div class="form-icon-inner">
                      <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 012 2v2.945M8 3.935V5.5A2.5 2.5 0 0010.5 8h.5a2 2 0 012 2 2 2 0 002 2h1.5a.5.5 0 01.5.5V14a2 2 0 01-2 2h-1a2 2 0 00-2 2v1.5a.5.5 0 01-.5.5H14a2 2 0 01-2-2v-1a2 2 0 00-2-2H7a2 2 0 01-2-2v-1.5a.5.5 0 01.5-.5H6a2 2 0 002-2V3.055" />
                      </svg>
                    </div>
                    <select
                      [(ngModel)]="selectedRegion"
                      class="form-select-premium form-input-with-icon"
                    >
                      <option value="">Region: Universal</option>
                      <option value="Middle East">Middle East</option>
                      <option value="Europe">Europe</option>
                    </select>
                  </div>

                  <div class="form-icon-wrapper">
                    <div class="form-icon-inner">
                      <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
                      </svg>
                    </div>
                    <select
                      [(ngModel)]="selectedStatus"
                      class="form-select-premium form-input-with-icon"
                    >
                      <option value="">Status: Universal</option>
                      <option value="Active">Active Focus</option>
                      <option value="On Leave">Leave Monitor</option>
                      <option value="Inactive">Retention History</option>
                    </select>
                  </div>
                </div>
              </div>

              <!-- Localization & Export Execution -->
              <div class="flex flex-col xl:flex-row xl:items-center justify-between gap-10 pt-10 border-t border-slate-100 dark:border-slate-800/50">
                <div class="flex items-center gap-8">
                  <div class="text-left">
                    <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Export Protocol</p>
                    <div class="flex bg-slate-100 dark:bg-slate-900/60 p-1.5 rounded-2xl border border-slate-200/50 dark:border-slate-700/50">
                      <button
                        (click)="selectedLanguage = 'en'"
                        [class.bg-white]="selectedLanguage === 'en'"
                        [class.dark:bg-slate-800]="selectedLanguage === 'en'"
                        [class.shadow-xl]="selectedLanguage === 'en'"
                        [class.text-indigo-600]="selectedLanguage === 'en'"
                        [class.text-slate-400]="selectedLanguage !== 'en'"
                        class="px-6 py-2.5 rounded-xl text-xs font-black uppercase transition-all duration-300 transform active:scale-95"
                      >
                        EN Protocol
                      </button>
                      <button
                        (click)="selectedLanguage = 'ar'"
                        [class.bg-white]="selectedLanguage === 'ar'"
                        [class.dark:bg-slate-800]="selectedLanguage === 'ar'"
                        [class.shadow-xl]="selectedLanguage === 'ar'"
                        [class.text-indigo-600]="selectedLanguage === 'ar'"
                        [class.text-slate-400]="selectedLanguage !== 'ar'"
                        class="px-6 py-2.5 rounded-xl text-xs font-black uppercase transition-all duration-300 transform active:scale-95"
                      >
                        AR Protocol
                      </button>
                    </div>
                  </div>
                </div>

                <div class="flex flex-wrap gap-4">
                  <button
                    (click)="exportReport('pdf')"
                    class="group relative h-14 pl-14 pr-8 bg-slate-900 dark:bg-indigo-600 text-white rounded-2xl hover:scale-105 active:scale-95 transition-all duration-300 shadow-2xl shadow-indigo-500/20"
                  >
                    <div class="absolute left-4 top-1/2 -translate-y-1/2 w-8 h-8 bg-white/10 rounded-lg flex items-center justify-center group-hover:bg-white/20 transition-colors">
                      <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z" /></svg>
                    </div>
                    <span class="text-[10px] font-black uppercase tracking-widest">Execute PDF</span>
                  </button>

                  <button
                    (click)="exportReport('csv')"
                    class="group relative h-14 pl-14 pr-8 bg-emerald-500 text-white rounded-2xl hover:scale-105 active:scale-95 transition-all duration-300 shadow-2xl shadow-emerald-500/20"
                  >
                    <div class="absolute left-4 top-1/2 -translate-y-1/2 w-8 h-8 bg-white/10 rounded-lg flex items-center justify-center group-hover:bg-white/20 transition-colors">
                      <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
                    </div>
                    <span class="text-[10px] font-black uppercase tracking-widest">Execute CSV</span>
                  </button>

                  <button
                    (click)="exportReport('excel')"
                    class="group relative h-14 pl-14 pr-8 bg-indigo-600 dark:bg-white dark:text-slate-900 text-white rounded-2xl hover:scale-105 active:scale-95 transition-all duration-300 shadow-2xl shadow-indigo-500/20"
                  >
                    <div class="absolute left-4 top-1/2 -translate-y-1/2 w-8 h-8 bg-white/10 dark:bg-slate-900/10 rounded-lg flex items-center justify-center group-hover:bg-white/20 transition-colors">
                      <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" /></svg>
                    </div>
                    <span class="text-[10px] font-black uppercase tracking-widest">Execute XLX</span>
                  </button>
                </div>
              </div>
            </div>
          </app-card>
        </div>

        <!-- Recent Archives -->
        <div class="xl:col-span-1">
          <app-card class="h-full">
            <div appCardHeader class="flex items-center gap-3">
              <div class="w-1.5 h-6 bg-amber-400 rounded-full"></div>
              <h2 class="text-xl font-black text-slate-900 dark:text-white tracking-tight">Recent Intelligence</h2>
            </div>
            
            <div class="py-6 space-y-4">
              <div
                *ngFor="let report of recentReports()"
                class="group p-4 bg-slate-50 dark:bg-slate-900/40 border border-slate-200 dark:border-slate-700/20 rounded-2xl transition-all hover:bg-white dark:hover:bg-slate-800 hover:border-indigo-500/30 hover:shadow-xl hover:shadow-indigo-500/5"
              >
                <div class="flex items-start justify-between mb-2">
                  <h4 class="text-sm font-bold text-slate-900 dark:text-white truncate">
                    {{ report.name }}
                  </h4>
                  <div class="scale-75 origin-right">
                    <app-badge [variant]="report.status === 'completed' ? 'success' : 'info'">
                      {{ report.status }}
                    </app-badge>
                  </div>
                </div>
                <div class="flex items-center justify-between">
                  <div class="flex items-center gap-2">
                    <svg class="w-3 h-3 text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
                    <span class="text-[10px] font-black text-slate-400 uppercase tracking-wider">{{ report.date }}</span>
                  </div>
                  <button class="flex items-center gap-1.5 text-indigo-600 dark:text-indigo-400 text-[10px] font-black uppercase opacity-0 group-hover:opacity-100 transition-all translate-x-2 group-hover:translate-x-0">
                    <span>Retrieve</span>
                    <svg class="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" /></svg>
                  </button>
                </div>
              </div>
            </div>
          </app-card>
        </div>
      </div>

      <!-- Template Catalog -->
      <div class="mt-20 mb-12" [@slideInUp]>
        <div class="flex items-center gap-6 mb-12">
          <div class="flex flex-col">
            <h2 class="text-3xl font-black text-slate-900 dark:text-white tracking-tighter leading-none mb-2">
              Executive Blueprints
            </h2>
            <p class="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em]">Strategic Template Library</p>
          </div>
          <div class="h-px flex-grow bg-slate-100 dark:bg-slate-800/50 rounded-full"></div>
        </div>
        
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          <app-card *ngFor="let template of reportTemplates()" class="group hover:-translate-y-2 hover:shadow-2xl hover:shadow-indigo-500/10 transition-all duration-500">
            <div class="flex flex-col h-full py-2">
              <div class="flex items-start justify-between mb-6">
                <div class="w-12 h-12 rounded-2xl bg-indigo-50 dark:bg-indigo-900/40 flex items-center justify-center text-indigo-600 dark:text-indigo-400 group-hover:scale-110 transition-transform duration-500">
                  <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                  </svg>
                </div>
                <div class="opacity-0 group-hover:opacity-100 transition-opacity">
                  <app-badge variant="success">Protocol Alpha</app-badge>
                </div>
              </div>
              
              <h3 class="text-xl font-bold text-slate-900 dark:text-white mb-3 group-hover:text-indigo-600 transition-colors">
                {{ template.name }}
              </h3>
              <p class="text-sm font-medium text-slate-500 dark:text-slate-400 mb-8 flex-grow leading-relaxed">
                {{ template.description }}
              </p>
              
              <button
                (click)="selectTemplate(template)"
                class="w-full h-12 bg-slate-900 dark:bg-white text-white dark:text-slate-900 rounded-xl hover:bg-indigo-600 dark:hover:bg-indigo-500 hover:text-white transition-all font-black uppercase tracking-widest text-[10px] shadow-lg active:scale-95"
              >
                Initialize Protocol
              </button>
            </div>
          </app-card>
        </div>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReportsComponent implements OnInit {
  private dataService = inject(DataService);
  private reportService = inject(ReportService);

  isLoading = signal<boolean>(true);
  reportTitle = 'HR Analytics Report';
  startDate = '';
  endDate = '';
  selectedTemplate = '';
  selectedDepartment = '';
  selectedRegion = '';
  selectedStatus = '';
  selectedLanguage: 'en' | 'ar' = 'en';

  reportTemplates = signal<ReportTemplate[]>([]);

  recentReports = signal([
    { name: 'Performance Analytics Report', date: '2024-01-15', status: 'completed' },
    { name: 'Workforce Metrics Report', date: '2024-01-14', status: 'completed' },
    { name: 'Turnover Analysis Report', date: '2024-01-13', status: 'completed' },
  ]);

  ngOnInit(): void {
    // Initialize with default date range (last 30 days)
    const today = new Date();
    const thirtyDaysAgo = new Date(today.getTime() - 30 * 24 * 60 * 60 * 1000);

    this.endDate = today.toISOString().split('T')[0];
    this.startDate = thirtyDaysAgo.toISOString().split('T')[0];

    this.loadTemplates();
  }

  private loadTemplates(): void {
    this.isLoading.set(true);
    this.dataService.getReportTemplates().subscribe({
      next: (templates) => {
        this.reportTemplates.set(templates);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading report templates:', err);
        this.isLoading.set(false);
      }
    });
  }

  onTemplateChange(): void {
    const template = this.reportTemplates().find((t) => t.id === this.selectedTemplate);
    if (template) {
      this.reportTitle = template.name;
    }
  }

  selectTemplate(template: ReportTemplate): void {
    this.selectedTemplate = template.id;
    this.reportTitle = template.name;
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  exportReport(format: 'pdf' | 'csv' | 'excel'): void {
    if (!this.reportTitle || !this.startDate || !this.endDate) {
      alert('Please fill in all required fields');
      return;
    }

    const template = this.reportTemplates().find((t) => t.id === this.selectedTemplate);
    const metrics = template?.metrics || [];

    const filters: Record<string, unknown> = {};
    if (this.selectedDepartment) filters['Department'] = this.selectedDepartment;
    if (this.selectedRegion) filters['Region'] = this.selectedRegion;
    if (this.selectedStatus) filters['Status'] = this.selectedStatus;

    const config: ReportConfig = {
      title: this.reportTitle,
      metrics,
      dateRange: {
        startDate: new Date(this.startDate),
        endDate: new Date(this.endDate),
      },
      filters: Object.keys(filters).length > 0 ? filters : undefined,
      language: this.selectedLanguage,
    };

    this.reportService.generateReport(config, format);
  }
}
