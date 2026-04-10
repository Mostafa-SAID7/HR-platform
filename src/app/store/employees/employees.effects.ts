import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { catchError, map, switchMap, retry } from 'rxjs/operators';
import * as EmployeeActions from './employees.actions';
import { DataService } from '../../services/data.service';

@Injectable()
export class EmployeeEffects {
  private actions$ = inject(Actions);
  private dataService = inject(DataService);

  readonly loadEmployees$ = createEffect(() =>
    this.actions$.pipe(
      ofType(EmployeeActions.loadEmployees),
      switchMap(() =>
        this.dataService.getEmployees().pipe(
          retry(3),
          map((employees) => EmployeeActions.loadEmployeesSuccess({ employees })),
          catchError((error) =>
            of(
              EmployeeActions.loadEmployeesFailure({
                error: error.message || 'Failed to load employees',
              }),
            ),
          ),
        )
      ),
    ),
  );

  readonly addEmployee$ = createEffect(() =>
    this.actions$.pipe(
      ofType(EmployeeActions.addEmployee),
      switchMap(({ employee }) =>
        of(employee).pipe(
          retry(2),
          switchMap(() => of(EmployeeActions.loadEmployees())),
          catchError((error) =>
            of(
              EmployeeActions.loadEmployeesFailure({
                error: error.message || 'Failed to add employee',
              }),
            ),
          ),
        ),
      ),
    ),
  );

  readonly updateEmployee$ = createEffect(() =>
    this.actions$.pipe(
      ofType(EmployeeActions.updateEmployee),
      switchMap(({ employee }) =>
        of(employee).pipe(
          retry(2),
          switchMap(() => of(EmployeeActions.loadEmployees())),
          catchError((error) =>
            of(
              EmployeeActions.loadEmployeesFailure({
                error: error.message || 'Failed to update employee',
              }),
            ),
          ),
        ),
      ),
    ),
  );

  readonly deleteEmployee$ = createEffect(() =>
    this.actions$.pipe(
      ofType(EmployeeActions.deleteEmployee),
      switchMap(({ id }) =>
        of(id).pipe(
          retry(2),
          switchMap(() => of(EmployeeActions.loadEmployees())),
          catchError((error) =>
            of(
              EmployeeActions.loadEmployeesFailure({
                error: error.message || 'Failed to delete employee',
              }),
            ),
          ),
        ),
      ),
    ),
  );
}
