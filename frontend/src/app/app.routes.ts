import { Routes } from '@angular/router';

// Dashboard is added in Phase 6; categories, transactions and budgets are already built, so the
// default route points at transactions.
export const routes: Routes = [
  { path: '', redirectTo: 'transactions', pathMatch: 'full' },
  {
    path: 'categories',
    loadChildren: () => import('./features/categories/categories.routes').then((m) => m.CATEGORIES_ROUTES),
  },
  {
    path: 'transactions',
    loadChildren: () => import('./features/transactions/transactions.routes').then((m) => m.TRANSACTIONS_ROUTES),
  },
  {
    path: 'budgets',
    loadChildren: () => import('./features/budgets/budgets.routes').then((m) => m.BUDGETS_ROUTES),
  },
  { path: '**', redirectTo: 'transactions' },
];
