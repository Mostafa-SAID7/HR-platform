import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
  ChangeDetectionStrategy,
  signal,
  computed,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { FormsModule } from '@angular/forms';
import { IconComponent } from '../icon/icon.component';
import { I18nService } from '../../services/i18n.service';
import { fadeIn, slideInUp } from '../../shared/animations';

export interface ColumnDefinition {
  key: string;
  label: string;
  width?: string;
  sortable?: boolean;
  template?: (value: any, row: any) => string;
}

export interface SortState {
  column: string | null;
  direction: 'asc' | 'desc';
}

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [CommonModule, ScrollingModule, FormsModule, IconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  animations: [fadeIn, slideInUp],
  template: `
    <div
      [@slideInUp]
      class="flex flex-col h-full bg-white dark:bg-slate-800 rounded-lg shadow-md dark:shadow-lg border border-slate-200 dark:border-slate-700 overflow-hidden"
    >
      <!-- Table Header with Controls -->
      <div
        class="flex items-center justify-between px-6 py-4 border-b border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800"
      >
        <div class="flex items-center gap-4">
          <h3 class="text-lg font-semibold text-slate-900 dark:text-white">
            {{ title }}
          </h3>
          <span class="text-sm font-medium text-slate-500 dark:text-slate-400 px-2 py-1 bg-slate-100 dark:bg-slate-700 rounded-md">
            {{ loading ? '...' : filteredData().length }} {{ i18n.translate('common.records') }}
          </span>
        </div>
        <div class="flex items-center gap-2">
          <select
            [(ngModel)]="pageSizeInternal"
            (change)="onPageSizeChange()"
            class="px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-md bg-white dark:bg-slate-700 text-slate-900 dark:text-white text-sm focus:ring-2 focus:ring-indigo-500 outline-none transition-all"
            aria-label="Items per page"
          >
            <option [value]="10">10 {{ i18n.translate('common.records') }}</option>
            <option [value]="25">25 {{ i18n.translate('common.records') }}</option>
            <option [value]="50">50 {{ i18n.translate('common.records') }}</option>
            <option [value]="100">100 {{ i18n.translate('common.records') }}</option>
          </select>
        </div>
      </div>

      <!-- Table Body -->
      <div class="flex-1 flex flex-col min-h-0 relative">
        <!-- Loading Overlay -->
        <div 
          *ngIf="loading" 
          [@fadeIn]
          class="absolute inset-0 z-20 bg-white/50 dark:bg-slate-800/50 backdrop-blur-[1px] flex items-center justify-center"
        >
          <div class="flex flex-col items-center gap-3">
            <div class="w-10 h-10 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin"></div>
            <p class="text-sm font-bold text-indigo-600 dark:text-indigo-400">Loading data...</p>
          </div>
        </div>

        <!-- Column Headers -->
        <div
          class="flex bg-slate-100 dark:bg-slate-800/80 border-b border-slate-200 dark:border-slate-700 sticky top-0 z-10"
        >
          <div class="w-12 px-4 py-3 flex items-center justify-center">
            <input
              type="checkbox"
              [checked]="allSelected()"
              (change)="toggleSelectAll()"
              [attr.aria-label]="i18n.translate('common.select_all')"
              class="w-4 h-4 rounded text-indigo-600 focus:ring-indigo-500 bg-white dark:bg-slate-700 border-slate-300 dark:border-slate-600 transition-all"
            />
          </div>
          <div
            *ngFor="let column of columns"
            [style.width]="column.width || 'auto'"
            class="flex-1 px-4 py-3 text-left rtl:text-right text-xs font-bold uppercase tracking-wider text-slate-500 dark:text-slate-400"
          >
            <button
              *ngIf="column.sortable"
              (click)="onSort(column.key)"
              class="flex items-center gap-2 hover:text-slate-900 dark:hover:text-white transition-all group"
              [attr.aria-label]="i18n.translate('common.sort_by') + ' ' + column.label"
            >
              {{ column.label }}
              <span
                class="transition-opacity"
                [class.opacity-100]="sortState().column === column.key"
                [class.opacity-0]="sortState().column !== column.key"
                [class.group-hover:opacity-50]="sortState().column !== column.key"
              >
                <app-icon *ngIf="sortState().column === column.key" [name]="sortState().direction === 'desc' ? 'chevron-down' : 'chevron-up'" size="xs"></app-icon>
              </span>
            </button>
            <span *ngIf="!column.sortable">{{ column.label }}</span>
          </div>
          <div class="w-12 px-4 py-3"></div>
        </div>

        <!-- Virtual Scrolled Viewport -->
        <cdk-virtual-scroll-viewport
          [itemSize]="56"
          class="flex-1 h-full scrollbar-thin scrollbar-thumb-slate-300 dark:scrollbar-thumb-slate-600"
        >
          <!-- Skeleton Rows -->
          <ng-container *ngIf="loading && paginatedData().length === 0">
            <div *ngFor="let i of [1,2,3,4,5,6]" class="flex items-center border-b border-slate-200 dark:border-slate-700 h-[56px] px-4 animate-pulse">
               <div class="w-12 flex justify-center"><div class="w-4 h-4 bg-slate-200 dark:bg-slate-700 rounded"></div></div>
               <div *ngFor="let col of columns" class="flex-1 px-4"><div class="h-4 bg-slate-200 dark:bg-slate-700 rounded w-3/4"></div></div>
               <div class="w-12"></div>
            </div>
          </ng-container>

          <ng-container *cdkVirtualFor="let row of paginatedData(); trackBy: trackById">
            <div
              [@fadeIn]
              class="flex items-center border-b border-slate-200 dark:border-slate-700 hover:bg-indigo-50/50 dark:hover:bg-indigo-900/10 transition-colors bg-white dark:bg-slate-800"
              [class.bg-indigo-50/30]="isRowSelected(row.id)"
              [class.dark:bg-indigo-900/5]="isRowSelected(row.id)"
            >
              <div class="w-12 px-4 py-4 flex items-center justify-center">
                <input
                  type="checkbox"
                  [checked]="isRowSelected(row.id)"
                  (change)="toggleRowSelection(row.id)"
                  [attr.aria-label]="'Select row ' + row.id"
                  class="w-4 h-4 rounded text-indigo-600 focus:ring-indigo-500 bg-white dark:bg-slate-700 border-slate-300 dark:border-slate-600 transition-all"
                />
              </div>
              <div
                *ngFor="let column of columns"
                [style.width]="column.width || 'auto'"
                class="flex-1 px-4 py-4 text-sm font-medium text-slate-700 dark:text-slate-200 truncate"
              >
                <ng-container *ngIf="column.template; else defaultVal">
                  <div [innerHTML]="column.template(row[column.key], row)"></div>
                </ng-container>
                <ng-template #defaultVal>
                  {{ row[column.key] }}
                </ng-template>
              </div>
              <div class="w-12 px-4 py-4 flex items-center justify-center">
                <button
                  (click)="toggleRowExpanded(row.id)"
                  class="p-1 rounded-full hover:bg-slate-100 dark:hover:bg-slate-700 text-slate-400 hover:text-indigo-600 dark:hover:text-indigo-400 transition-all transform"
                  [attr.aria-label]="i18n.translate('common.expand')"
                  [class.rotate-180]="isRowExpanded(row.id)"
                >
                  <app-icon name="chevron-down" size="sm"></app-icon>
                </button>
              </div>
            </div>

            <!-- Expanded Details -->
            <div
              *ngIf="isRowExpanded(row.id)"
              [@fadeIn]
              class="bg-slate-50 dark:bg-slate-900/50 px-12 py-6 border-b border-slate-200 dark:border-slate-700 animate-in fade-in slide-in-from-top-2 duration-200"
            >
              <ng-container *ngIf="expandedRowTemplate">
                <ng-container
                  *ngTemplateOutlet="expandedRowTemplate; context: { $implicit: row }"
                ></ng-container>
              </ng-container>
              <div
                *ngIf="!expandedRowTemplate"
                class="grid grid-cols-2 md:grid-cols-3 gap-6"
              >
                <div *ngFor="let column of columns" class="space-y-1">
                  <span class="text-xs font-bold text-slate-400 uppercase tracking-tighter">{{ column.label }}</span>
                  <p class="text-sm text-slate-700 dark:text-slate-300">
                    {{ column.template ? '' : row[column.key] }}
                    <span *ngIf="column.template" [innerHTML]="column.template(row[column.key], row)"></span>
                  </p>
                </div>
              </div>
            </div>
          </ng-container>
        <!-- Empty State -->
        <div *ngIf="!loading && paginatedData().length === 0" class="flex flex-col items-center justify-center p-12 text-slate-500 dark:text-slate-400 h-full" [@fadeIn]>
          <app-icon name="document-magnifying-glass" class="w-12 h-12 text-slate-400 dark:text-slate-500 mb-4"></app-icon>
          <p class="text-lg font-medium text-slate-900 dark:text-slate-200">{{ i18n.translate('common.no_records') }}</p>
          <p class="text-sm mt-1">Try adjusting your filters or search query.</p>
        </div>
        </cdk-virtual-scroll-viewport>
      </div>

      <!-- Pagination Footer -->
      <footer
        class="flex items-center justify-between px-6 py-4 border-t border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-900/80 backdrop-blur-sm"
      >
        <div class="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
          {{ i18n.translate('common.showing') }} {{ currentPageStart() + 1 }} {{ i18n.translate('common.to') }} {{ currentPageEnd() }} {{ i18n.translate('common.of') }}
          <span class="text-slate-900 dark:text-white">{{ filteredData().length }}</span>
        </div>
        <div class="flex items-center gap-3">
          <div class="flex items-center gap-1">
            <button
              (click)="previousPage()"
              [disabled]="currentPage() === 0"
              class="p-2 border border-slate-300 dark:border-slate-600 rounded-md bg-white dark:bg-slate-800 text-slate-700 dark:text-slate-300 hover:bg-indigo-50 dark:hover:bg-indigo-900/20 disabled:opacity-30 disabled:cursor-not-allowed transition-all shadow-sm"
              [attr.aria-label]="i18n.translate('common.previous')"
            >
              <app-icon name="chevron-left" size="sm"></app-icon>
            </button>
            
            <div class="flex items-center px-4 py-2 bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 rounded-md text-sm font-bold text-indigo-600 dark:text-indigo-400 shadow-sm">
              {{ currentPage() + 1 }} / {{ totalPages() }}
            </div>

            <button
              (click)="nextPage()"
              [disabled]="currentPage() >= totalPages() - 1"
              class="p-2 border border-slate-300 dark:border-slate-600 rounded-md bg-white dark:bg-slate-800 text-slate-700 dark:text-slate-300 hover:bg-indigo-50 dark:hover:bg-indigo-900/20 disabled:opacity-30 disabled:cursor-not-allowed transition-all shadow-sm"
              [attr.aria-label]="i18n.translate('common.next')"
            >
              <app-icon name="chevron-right" size="sm"></app-icon>
            </button>
          </div>
        </div>
      </footer>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      height: 100%;
    }
    cdk-virtual-scroll-viewport {
      scrollbar-width: thin;
    }
  `]
})
export class DataTableComponent implements OnInit, OnDestroy {
  @Input() title = 'Data Table';
  @Input() loading = false;
  @Input() columns: ColumnDefinition[] = [];
  @Input() data: any[] = [];
  @Input() expandedRowTemplate: any;

  @Output() sortChange = new EventEmitter<SortState>();
  @Output() rowSelectionChange = new EventEmitter<string[]>();
  @Output() rowExpanded = new EventEmitter<string>();

  public i18n = inject(I18nService);

  // Signals
  pageSizeInternal = signal(25);
  currentPage = signal(0);
  sortState = signal<SortState>({ column: null, direction: 'asc' });
  selectedRows = signal<Set<string>>(new Set());
  expandedRows = signal<Set<string>>(new Set());

  // Computed
  filteredData = computed(() => {
    const data = this.data;
    if (!data || data.length === 0) return [];
    
    // Create copy for sorting
    const result = [...data];

    // Apply sorting
    const sort = this.sortState();
    if (sort.column !== null) {
      const column = sort.column;
      result.sort((a, b) => {
        const aVal = a[column];
        const bVal = b[column];

        if (aVal === bVal) return 0;
        const comparison = aVal < bVal ? -1 : 1;
        return sort.direction === 'asc' ? comparison : -comparison;
      });
    }

    return result;
  });

  paginatedData = computed(() => {
    const filtered = this.filteredData();
    const start = this.currentPage() * this.pageSizeInternal();
    return filtered.slice(start, start + this.pageSizeInternal());
  });

  totalPages = computed(() => {
    return Math.ceil(this.filteredData().length / this.pageSizeInternal()) || 1;
  });

  currentPageStart = computed(() => {
    return this.currentPage() * this.pageSizeInternal();
  });

  currentPageEnd = computed(() => {
    return Math.min(this.currentPageStart() + this.pageSizeInternal(), this.filteredData().length);
  });

  allSelected = computed(() => {
    const paginated = this.paginatedData();
    if (paginated.length === 0) return false;
    return paginated.every((row) => this.selectedRows().has(row.id));
  });

  ngOnInit(): void {
    // Reset page if data changes
    this.currentPage.set(0);
  }

  ngOnDestroy(): void {
    // Cleanup
  }

  trackById(index: number, item: any): string {
    return item.id;
  }

  onSort(column: string): void {
    const current = this.sortState();
    let direction: 'asc' | 'desc' = 'asc';

    if (current.column === column && current.direction === 'asc') {
      direction = 'desc';
    }

    this.sortState.set({ column, direction });
    this.sortChange.emit(this.sortState());
  }

  onPageSizeChange(): void {
    this.currentPage.set(0);
  }

  nextPage(): void {
    if (this.currentPage() < this.totalPages() - 1) {
      this.currentPage.update((p) => p + 1);
    }
  }

  previousPage(): void {
    if (this.currentPage() > 0) {
      this.currentPage.update((p) => p - 1);
    }
  }

  toggleRowSelection(rowId: string): void {
    const selected = new Set(this.selectedRows());
    if (selected.has(rowId)) {
      selected.delete(rowId);
    } else {
      selected.add(rowId);
    }
    this.selectedRows.set(selected);
    this.rowSelectionChange.emit(Array.from(selected));
  }

  toggleSelectAll(): void {
    const paginated = this.paginatedData();
    const selected = new Set(this.selectedRows());

    if (this.allSelected()) {
      paginated.forEach((row) => selected.delete(row.id));
    } else {
      paginated.forEach((row) => selected.add(row.id));
    }

    this.selectedRows.set(selected);
    this.rowSelectionChange.emit(Array.from(selected));
  }

  isRowSelected(rowId: string): boolean {
    return this.selectedRows().has(rowId);
  }

  toggleRowExpanded(rowId: string): void {
    const expanded = new Set(this.expandedRows());
    if (expanded.has(rowId)) {
      expanded.delete(rowId);
    } else {
      expanded.add(rowId);
    }
    this.expandedRows.set(expanded);
    this.rowExpanded.emit(rowId);
  }

  isRowExpanded(rowId: string): boolean {
    return this.expandedRows().has(rowId);
  }
}
