import { Routes } from '@angular/router';

export const AUTH_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./login-page/login-page').then(m => m.LoginPageComponent)
  }
];