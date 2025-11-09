import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'accounts',
    pathMatch: 'full',
  },
  {
    path: 'accounts',
    loadComponent: () =>
      import('./account-list/account-list.component').then(m => m.AccountListComponent),
  },
  {
    path: 'accounts/:id',
    loadComponent: () =>
      import('./account-detail/account-detail.component').then(m => m.AccountDetailComponent),
  },
];
