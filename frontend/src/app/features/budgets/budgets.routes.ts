import { Routes } from '@angular/router';

export const BUDGETS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./budget-list/budget-list').then((m) => m.BudgetList),
  },
];
