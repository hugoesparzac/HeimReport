import { Routes } from '@angular/router';

export const EMPLOYEES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./employees-page/employees-page').then(m => m.EmployeesPageComponent)
  },
   {
    path: 'register',
    loadComponent: () => import('./register-employee-page/register-employee-page').then(m => m.RegisterEmployeePageComponent)
  },
  {
    path: 'details',
    loadComponent: () => import('./employee-detail-page/employee-detail-page').then(m => m.EmployeeDetailPageComponent)
  }
];