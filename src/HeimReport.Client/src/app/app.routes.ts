import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'login',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
  },
   {
    path: 'dashboard',
    loadChildren: () => import('./features/dashboard/dashboard.routes').then(m => m.DASHBOARD_ROUTES)
  },
  {
    path: 'employees',
    loadChildren: () => import('./features/employees/employees.routes').then(m => m.EMPLOYEES_ROUTES)
  },
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  }
];