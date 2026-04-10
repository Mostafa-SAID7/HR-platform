import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FilterPanelComponent } from './filter-panel.component';
import { FormsModule } from '@angular/forms';
import { vi } from 'vitest';

describe('FilterPanelComponent', () => {
  let component: FilterPanelComponent;
  let fixture: ComponentFixture<FilterPanelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FilterPanelComponent, FormsModule],
    }).compileComponents();

    // Mock localStorage
    const store: any = {};
    const mockLocalStorage = {
      getItem: (key: string): string | null => {
        return key in store ? store[key] : null;
      },
      setItem: (key: string, value: string) => {
        store[key] = `${value}`;
      },
      clear: () => {
        Object.keys(store).forEach(key => delete store[key]);
      }
    };
    Object.defineProperty(window, 'localStorage', {
      value: mockLocalStorage
    });

    fixture = TestBed.createComponent(FilterPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should emit filter criteria when change triggered', () => {
    const emitSpy = vi.spyOn(component.filterChange, 'emit');
    component.selectedDepartments.set(['Engineering']);
    component.onFilterChange();
    
    expect(emitSpy).toHaveBeenCalled();
    const emittedData = emitSpy.mock.calls[0][0];
    expect(emittedData).toBeDefined();
    expect(emittedData?.department).toEqual(['Engineering']);
  });

  it('should toggle advanced filters flag', () => {
    expect(component.showAdvanced()).toBe(false);
    component.toggleAdvanced();
    expect(component.showAdvanced()).toBe(true);
  });

  it('should save to presets successfully', () => {
    // Mock the prompt
    vi.spyOn(window, 'prompt').mockReturnValue('My Preset');
    
    component.selectedDepartments.set(['HR']);
    component.savePreset();

    expect(component.presets().length).toBe(1);
    expect(component.presets()[0].name).toBe('My Preset');
    expect(component.presets()[0].criteria.department).toEqual(['HR']);
    
    // Check LS
    const ls = JSON.parse(localStorage.getItem('filterPresets') || '[]');
    expect(ls.length).toBe(1);
    expect(ls[0].name).toBe('My Preset');
  });

  it('should load preset correctly', () => {
    const emitSpy = vi.spyOn(component.filterChange, 'emit');
    const mockPreset = {
        id: '123',
        name: 'Test',
        criteria: { region: ['Europe'] },
        createdAt: new Date().toISOString()
    };

    component.loadPreset(mockPreset);
    
    expect(component.selectedRegions()).toEqual(['Europe']);
    expect(emitSpy).toHaveBeenCalledWith(mockPreset.criteria);
  });
});
