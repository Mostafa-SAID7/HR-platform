import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EmployeesComponent, Employee } from './employees.component';
import { provideMockStore } from '@ngrx/store/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { Component } from '@angular/core';

@Component({ selector: 'app-data-table', template: '', standalone: true })
class MockDataTableComponent {}
@Component({ selector: 'app-search-input', template: '', standalone: true })
class MockSearchInputComponent {}
@Component({ selector: 'app-filter-panel', template: '', standalone: true })
class MockFilterPanelComponent {}
@Component({ selector: 'app-card', template: '', standalone: true })
class MockCardComponent {}
@Component({ selector: 'app-badge', template: '', standalone: true })
class MockBadgeComponent {}

describe('EmployeesComponent', () => {
  let component: EmployeesComponent;
  let fixture: ComponentFixture<EmployeesComponent>;

  const mockEmployees: Employee[] = [
    {
      id: '1',
      name: 'John Doe',
      email: 'john@example.com',
      department: 'Engineering',
      region: 'Europe',
      role: 'Senior Engineer',
      employmentStatus: 'active',
      performanceScore: 85,
      hireDate: '2020-01-15',
    },
    {
      id: '2',
      name: 'Jane Smith',
      email: 'jane@example.com',
      department: 'Sales',
      region: 'Middle East',
      role: 'Sales Manager',
      employmentStatus: 'active',
      performanceScore: 78,
      hireDate: '2019-06-20',
    },
    {
      id: '3',
      name: 'Bob Johnson',
      email: 'bob@example.com',
      department: 'Engineering',
      region: 'Europe',
      role: 'Junior Engineer',
      employmentStatus: 'on-leave',
      performanceScore: 72,
      hireDate: '2021-03-10',
    },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EmployeesComponent, NoopAnimationsModule],
      providers: [
        provideMockStore({
          initialState: {
            employees: {
              ids: ['1', '2', '3'],
              entities: {
                '1': mockEmployees[0],
                '2': mockEmployees[1],
                '3': mockEmployees[2],
              },
              loading: false,
              error: null,
              selectedEmployeeId: null,
            },
          },
        }),
      ],
    })
    .overrideComponent(EmployeesComponent, {
      remove: { imports: [
        // Real components to remove from imports
      ] },
      add: { imports: [
        MockDataTableComponent,
        MockSearchInputComponent,
        MockFilterPanelComponent,
        MockCardComponent,
        MockBadgeComponent
      ] }
    })
    .compileComponents();

    fixture = TestBed.createComponent(EmployeesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display employee count', () => {
    component.employees.set(mockEmployees);
    fixture.detectChanges();

    expect(component.filteredEmployees().length).toBe(3);
  });

  it('should calculate active employees count correctly', () => {
    component.employees.set(mockEmployees);
    fixture.detectChanges();

    expect(component.activeEmployeesCount()).toBe(2);
  });

  it('should calculate on leave count correctly', () => {
    component.employees.set(mockEmployees);
    fixture.detectChanges();

    expect(component.onLeaveCount()).toBe(1);
  });

  it('should calculate average performance score', () => {
    component.employees.set(mockEmployees);
    fixture.detectChanges();

    const avg = component.averagePerformance();
    expect(avg).toBe(78); // (85 + 78 + 72) / 3 = 78.33, rounded to 78
  });

  it('should count unique departments', () => {
    component.employees.set(mockEmployees);
    fixture.detectChanges();

    expect(component.uniqueDepartments()).toBe(2); // Engineering and Sales
  });

  it('should filter employees by department', () => {
    component.employees.set(mockEmployees);
    component.currentFilters.set({
      department: ['Engineering'],
      region: [],
      status: [],
      performanceScoreRange: [0, 100],
      hireDateRange: [],
    });
    fixture.detectChanges();

    expect(component.filteredEmployees().length).toBe(2);
  });

  it('should filter employees by status', () => {
    component.employees.set(mockEmployees);
    component.currentFilters.set({
      department: [],
      region: [],
      status: ['active'],
      performanceScoreRange: [0, 100],
      hireDateRange: [],
    });
    fixture.detectChanges();

    expect(component.filteredEmployees().length).toBe(2);
  });

  it('should filter employees by performance score range', () => {
    component.employees.set(mockEmployees);
    component.currentFilters.set({
      department: [],
      region: [],
      status: [],
      performanceScoreRange: [75, 100],
      hireDateRange: [],
    });
    fixture.detectChanges();

    expect(component.filteredEmployees().length).toBe(2); // 85 and 78
  });

  it('should handle search results', () => {
    component.employees.set(mockEmployees);
    const searchResults = [mockEmployees[0]];
    component.onSearch(searchResults);
    fixture.detectChanges();

    expect(component.filteredEmployees().length).toBe(1);
  });

  it('should apply multiple filters simultaneously', () => {
    component.employees.set(mockEmployees);
    component.currentFilters.set({
      department: ['Engineering'],
      region: [],
      status: ['active'],
      performanceScoreRange: [0, 100],
      hireDateRange: [],
    });
    fixture.detectChanges();

    expect(component.filteredEmployees().length).toBe(1); // Only John Doe
  });

  it('should pass accessibility checks', async () => {
    const { checkA11y } = await import('../../testing/a11y-utils');
    fixture.detectChanges();
    await checkA11y(fixture);
  });
});
