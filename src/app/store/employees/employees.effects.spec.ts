import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Subject, firstValueFrom } from 'rxjs';
import { EmployeeEffects } from './employees.effects';
import * as EmployeeActions from './employees.actions';
import { DataService } from '../../services/data.service';
import { of } from 'rxjs';

describe('EmployeeEffects', () => {
  let employeeEffects: EmployeeEffects;
  let actions$: Subject<any>;
  let mockDataService: any;

  beforeEach(() => {
    actions$ = new Subject();
    mockDataService = {
      getEmployees: () => of([]),
    };

    TestBed.configureTestingModule({
      providers: [
        EmployeeEffects,
        provideMockActions(() => actions$),
        { provide: DataService, useValue: mockDataService },
      ],
    });

    employeeEffects = TestBed.inject(EmployeeEffects);
  });

  it('should be created', () => {
    expect(employeeEffects).toBeTruthy();
  });

  describe('loadEmployees$', () => {
    it('should return loadEmployeesSuccess action on success', async () => {
      const completion = EmployeeActions.loadEmployeesSuccess({ employees: [] });
      
      const resultPromise = firstValueFrom(employeeEffects.loadEmployees$);
      actions$.next(EmployeeActions.loadEmployees());

      const result = await resultPromise;
      expect(result).toEqual(completion);
    });
  });

  describe('addEmployee$', () => {
    it('should handle addEmployee action', async () => {
      const employee = { name: 'Test', role: 'Dev' } as any;
      const resultPromise = firstValueFrom(employeeEffects.addEmployee$);
      
      actions$.next(EmployeeActions.addEmployee({ employee }));

      const result = await resultPromise;
      expect(result).toEqual(EmployeeActions.loadEmployees());
    });
  });

  describe('updateEmployee$', () => {
    it('should handle updateEmployee action', async () => {
      const employee = { id: '1', name: 'Test' } as any;
      const resultPromise = firstValueFrom(employeeEffects.updateEmployee$);
      
      actions$.next(EmployeeActions.updateEmployee({ employee }));

      const result = await resultPromise;
      expect(result).toEqual(EmployeeActions.loadEmployees());
    });
  });

  describe('deleteEmployee$', () => {
    it('should handle deleteEmployee action', async () => {
      const resultPromise = firstValueFrom(employeeEffects.deleteEmployee$);
      
      actions$.next(EmployeeActions.deleteEmployee({ id: '1' }));

      const result = await resultPromise;
      expect(result).toEqual(EmployeeActions.loadEmployees());
    });
  });
});
