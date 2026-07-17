import { Routes } from '@angular/router';

export const PREDICTIVE_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./predictive-page/predictive-page').then(m => m.PredictivePageComponent)
  }
];