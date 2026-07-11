import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'login',
    loadChildren: () => import('./auth/feature/auth.routes').then(m => m.AUTH_ROUTES)
  },
   {
    path: 'dashboard',
    loadChildren: () => import('./dashboard/feature/dashboard.routes').then(m => m.DASHBOARD_ROUTES)
  },
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  }
];