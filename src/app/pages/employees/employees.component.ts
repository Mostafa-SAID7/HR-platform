import { Component, OnInit, OnDestroy, signal, inject, computed, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Store } from '@ngrx/store';
import { DataTableComponent, SearchInputComponent, FilterPanelComponent, CardComponent, type ColumnDefinition, type SearchResult, type FilterCriteria } from '../../components';
import { DataService } from '../../services/data.service';
import { Employee } from '../../store/employees/employees.state';
import { AppState } from '../../store/app.state';
import { selectAllEmployees } from '../../store/employees/employees.selectors';
import { loadEmployees } from '../../store/employees/employees.actions';
import { fadeIn, slideInUp, routeAnimation } from '../../shared/animations';

@Component({
  selector: 'app-employees',
  standalone: true,
  imports: [
    CommonModule,
    DataTableComponent,
    SearchInputComponent,
    FilterPanelComponent,
    CardComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  animations: [fadeIn, slideInUp],
  template: `
    <div class="page-container" [@fadeIn]>
      <!-- Balanced Page Header -->
      <div class="flex flex-col lg:flex-row lg:items-end justify-between gap-8 mb-4" [@slideInUp]>
        <div class="space-y-2">
          <h1 class="text-4xl font-black text-slate-900 dark:text-white tracking-tighter">
            Employee Intelligence
          </h1>
          <p class="text-slate-500 dark:text-slate-400 text-lg font-medium leading-relaxed max-w-2xl">
            Real-time workforce monitoring and performance distribution across global regions.
          </p>
        </div>
        
        <!-- Summary Stats Ribbon -->
        <div class="flex flex-wrap items-center gap-4">
          <div class="bg-indigo-50 dark:bg-indigo-900/40 border border-indigo-100 dark:border-indigo-800/30 px-6 py-3 rounded-2xl shadow-sm">
            <p class="text-[10px] font-black uppercase tracking-widest text-indigo-400 mb-1">Total Force</p>
            <div class="flex items-baseline gap-1">
              <span class="text-2xl font-black text-indigo-700 dark:text-indigo-300">{{ employees().length }}</span>
              <span class="text-xs font-bold text-indigo-400">Headcount</span>
            </div>
          </div>
          
          <div class="bg-emerald-50 dark:bg-emerald-900/40 border border-emerald-100 dark:border-emerald-800/30 px-6 py-3 rounded-2xl shadow-sm">
            <p class="text-[10px] font-black uppercase tracking-widest text-emerald-400 mb-1">Active Now</p>
            <div class="flex items-baseline gap-1">
              <span class="text-2xl font-black text-emerald-700 dark:text-emerald-300">{{ activeEmployeesCount() }}</span>
              <span class="text-xs font-bold text-emerald-400">Synced</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Sophisticated Filtering Interface -->
      <div class="grid grid-cols-1 lg:grid-cols-4 gap-8 mt-10">
        <!-- Sidebar Filter Panel -->
        <div class="lg:col-span-1" [@slideInUp]>
          <app-filter-panel
            (filterChange)="onFilterChange($event)"
          ></app-filter-panel>
        </div>

        <!-- Main Workspace -->
        <div class="lg:col-span-3 space-y-8">
          <!-- Search & Export Controls -->
          <app-card>
            <div class="flex flex-col md:flex-row items-center gap-6">
              <div class="flex-1 w-full">
                <app-search-input
                  placeholder="Universal search: Name, Role, Department..."
                  [data]="employees()"
                  [searchableFields]="['name', 'email', 'department', 'role']"
                  (search)="onSearch($event)"
                  (queryChange)="onSearchQueryChange($event)"
                ></app-search-input>
              </div>
              <button class="bg-slate-900 dark:bg-indigo-600 text-white px-6 py-3.5 rounded-2xl font-bold text-sm shadow-xl shadow-indigo-600/20 hover:scale-105 active:scale-95 transition-all flex items-center gap-2">
                 <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" /></svg>
                 Export Report
              </button>
            </div>
          </app-card>

          <!-- Core Data Grid -->
          <div class="h-[700px] animate-in fade-in slide-in-from-bottom-4 duration-1000">
            <app-data-table
              [title]="'Synchronized Workforce View'"
              [loading]="isLoading()"
              [data]="filteredEmployees()"
              [columns]="tableColumns"
            ></app-data-table>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class EmployeesComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Signals
  isLoading = signal<boolean>(true);
  employees = signal<Employee[]>([]);
  searchResults = signal<Employee[]>([]);
  filteredEmployees = computed(() => {
    // If we have a non-empty search query but no search results, we should show empty list
    // If we have no search query, we use all employees
    const query = this.searchQuery();
    let dataToFilter = this.employees();
    
    if (query) {
      dataToFilter = this.searchResults();
    }
    
    return this.applyFilters(dataToFilter);
  });

  // Filter state
  currentFilters = signal<FilterCriteria>({});

  // Table columns
  tableColumns: ColumnDefinition[] = [
    { key: 'name', label: 'Name', width: '200px', sortable: true },
    { key: 'email', label: 'Email', width: '200px', sortable: true },
    { key: 'department', label: 'Department', width: '150px', sortable: true },
    { key: 'role', label: 'Role', width: '150px', sortable: true },
    { key: 'employmentStatus', label: 'Status', width: '120px', sortable: true },
    {
      key: 'performanceScore',
      label: 'Performance',
      width: '120px',
      sortable: true,
      template: (value: number) => `${value}/100`,
    },
    {
      key: 'hireDate',
      label: 'Hire Date',
      width: '120px',
      sortable: true,
      template: (value: string) => new Date(value).toLocaleDateString(),
    },
  ];

  // Computed stats
  activeEmployeesCount = computed(() =>
    this.filteredEmployees().filter((e) => e.employmentStatus === 'active').length,
  );

  onLeaveCount = computed(() =>
    this.filteredEmployees().filter((e) => e.employmentStatus === 'on-leave').length,
  );

  averagePerformance = computed(() => {
    const employees = this.filteredEmployees();
    const withScores = employees.filter((e) => e.performanceScore !== undefined);
    if (withScores.length === 0) return 0;
    const sum = withScores.reduce((acc, e) => acc + (e.performanceScore || 0), 0);
    return Math.round(sum / withScores.length);
  });

  uniqueDepartments = computed(() => {
    const departments = new Set(this.employees().map((e) => e.department));
    return departments.size;
  });

  searchQuery = signal<string>('');

  constructor(private store: Store<AppState>) {}

  ngOnInit(): void {
    // Dispatch load action
    this.store.dispatch(loadEmployees());

    // Load employees from store
    this.store
      .select(selectAllEmployees)
      .pipe(takeUntil(this.destroy$))
      .subscribe((employees: Employee[]) => {
        this.employees.set(employees);
        // Simulate a slight loading delay for premium feel
        setTimeout(() => this.isLoading.set(false), 800);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSearch(results: SearchResult[]): void {
    this.searchResults.set(results as Employee[]);
  }

  onSearchQueryChange(query: string): void {
    this.searchQuery.set(query);
  }

  onFilterChange(filters: FilterCriteria): void {
    this.currentFilters.set(filters);
  }

  private applyFilters(data: Employee[]): Employee[] {
    const filters = this.currentFilters();
    if (!filters || Object.keys(filters).length === 0) return data;

    return data.filter((employee) => {
      // Department filter
      if (filters.department && filters.department.length > 0) {
        if (!filters.department.includes(employee.department)) {
          return false;
        }
      }

      // Region filter
      if (filters.region && filters.region.length > 0) {
        if (!filters.region.includes(employee.region)) {
          return false;
        }
      }

      // Status filter (Case-insensitive normalization)
      const statusFilters = filters.employmentStatus || filters.status;
      if (statusFilters && statusFilters.length > 0) {
        const normalizedEmployeeStatus = employee.employmentStatus.toLowerCase();
        const hasMatch = statusFilters.some(s => s.toLowerCase().replace(' ', '-') === normalizedEmployeeStatus);
        if (!hasMatch) return false;
      }

      // Performance score range filter
      if (employee.performanceScore !== undefined) {
        const min = filters.performanceScoreMin ?? 0;
        const max = filters.performanceScoreMax ?? 100;
        if (employee.performanceScore < min || employee.performanceScore > max) {
          return false;
        }
      }

      return true;
    });
  }
}
