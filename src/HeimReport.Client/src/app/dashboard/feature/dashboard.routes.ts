import { Routes } from '@angular/router';

export const DASHBOARD_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./overview-page/overview-page.component').then(m => m.OverviewPageComponent)
  }
];