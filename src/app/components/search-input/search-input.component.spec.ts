import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SearchInputComponent, SearchResult } from './search-input.component';
import { FormsModule } from '@angular/forms';
import { vi } from 'vitest';

describe('SearchInputComponent', () => {
  let component: SearchInputComponent;
  let fixture: ComponentFixture<SearchInputComponent>;

  beforeEach(async () => {
    vi.useFakeTimers();
    await TestBed.configureTestingModule({
      imports: [SearchInputComponent, FormsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(SearchInputComponent);
    component = fixture.componentInstance;
    component.data = [
      { id: '1', name: 'Alice Smith', department: 'Engineering' },
      { id: '2', name: 'Bob Jones', department: 'HR' },
      { id: '3', name: 'Charlie Alice', department: 'Sales' }
    ];
    component.searchableFields = ['name', 'department'];
    fixture.detectChanges();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should debounce search and perform full text search', async () => {
    const emitSpy = vi.spyOn(component.search, 'emit');
    
    component.onSearchChange('alice');
    // Fast forward enough for debounceTime(300)
    vi.advanceTimersByTime(300);
    
    expect(emitSpy).toHaveBeenCalledTimes(1);
    const results = emitSpy.mock.calls[0][0] as SearchResult[];
    expect(results).toBeDefined();
    expect(results.length).toBe(2); 
    // Alice Smith and Charlie Alice both match 'alice'
  });

  it('should handle field-specific search (field:value) directly', async () => {
    const emitSpy = vi.spyOn(component.search, 'emit');
    
    component.onSearchChange('department:HR');
    vi.advanceTimersByTime(300);
    
    expect(emitSpy).toHaveBeenCalledTimes(1);
    const results = emitSpy.mock.calls[0][0] as SearchResult[];
    expect(results).toBeDefined();
    expect(results.length).toBe(1);
    expect(results[0]['name']).toBe('Bob Jones');
  });

  it('should clear search input when requested', () => {
    const emptySpy = vi.spyOn(component.search, 'emit');
    component.searchQuery.set('Alice');
    component.clearSearch();
    
    expect(component.searchQuery()).toBe('');
    expect(emptySpy).toHaveBeenCalledWith(component.data); // Emits full dataset
  });

  it('should provide suggestions if search matches substring', () => {
    component.onSearchChange('en');
    expect(component.suggestions().length).toBeGreaterThan(0);
    expect(component.suggestions()).toContain('Engineering');
    expect(component.suggestions()).toContain('department:'); // Suggests field
  });
});
