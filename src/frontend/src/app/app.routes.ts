import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/finance',
    pathMatch: 'full',
  },
  {
    path: 'finance',
    loadChildren: () => import('./modules/finance/finance.module').then((m) => m.FinanceModule),
  },
];
