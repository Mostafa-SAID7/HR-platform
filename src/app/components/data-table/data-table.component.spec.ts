import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DataTableComponent } from './data-table.component';
import { I18nService } from '../../services/i18n.service';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { vi } from 'vitest';

describe('DataTableComponent', () => {
  let component: DataTableComponent;
  let fixture: ComponentFixture<DataTableComponent>;
  let i18nMock: any;

  beforeEach(async () => {
    i18nMock = { translate: vi.fn((key) => key) };

    await TestBed.configureTestingModule({
      imports: [DataTableComponent, ScrollingModule, BrowserAnimationsModule],
      providers: [
        { provide: I18nService, useValue: i18nMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DataTableComponent);
    component = fixture.componentInstance;
    
    // Setup basic input data
    component.columns = [
      { key: 'name', label: 'Name', sortable: true },
      { key: 'department', label: 'Department', sortable: true }
    ];
    component.data = [
      { id: '1', name: 'Zane', department: 'HR' },
      { id: '2', name: 'Alice', department: 'Engineering' },
      { id: '3', name: 'Bob', department: 'Engineering' }
    ];
    
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should correctly format and expose paginatedData length', () => {
    component.pageSizeInternal.set(2);
    expect(component.paginatedData().length).toBe(2);
    expect(component.totalPages()).toBe(2);
  });

  it('should go to the next page and previous page', () => {
    component.pageSizeInternal.set(2);
    component.nextPage();
    expect(component.currentPage()).toBe(1);
    expect(component.paginatedData().length).toBe(1); // 1 item on last page
    
    component.previousPage();
    expect(component.currentPage()).toBe(0);
  });

  it('should sort data when onSort is called', () => {
    component.onSort('name');
    const sortedOnce = component.filteredData();
    expect(sortedOnce[0].name).toBe('Alice'); // ASC sort

    component.onSort('name');
    const sortedTwice = component.filteredData();
    expect(sortedTwice[0].name).toBe('Zane'); // DESC sort
  });

  it('should emit row selection events when toggles are clicked', () => {
    const emitSpy = vi.spyOn(component.rowSelectionChange, 'emit');
    component.toggleRowSelection('2');
    expect(component.isRowSelected('2')).toBe(true);
    expect(emitSpy).toHaveBeenCalledWith(['2']);
    
    component.toggleSelectAll();
    expect(component.allSelected()).toBe(true);
  });

  it('should pass accessibility checks', async () => {
    const { checkA11y } = await import('../../testing/a11y-utils');
    await checkA11y(fixture);
  });
});
