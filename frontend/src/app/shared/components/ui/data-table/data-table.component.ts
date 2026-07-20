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
import { I18nService } from '@app/core';
import { fadeIn, slideInUp } from '@app/shared/animations';

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
  templateUrl: './data-table.component.html',
  styleUrl: './data-table.component.css'
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
