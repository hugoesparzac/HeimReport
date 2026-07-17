import { Routes } from '@angular/router';

export const EMPLOYEES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./employees-page/employees-page').then(m => m.EmployeesPageComponent)
  }
];