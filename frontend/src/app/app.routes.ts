import { Routes } from '@angular/router';

// Dashboard, transactions and budgets are added in later phases; categories is the only feature
// built so far, so it is also the default route.
export const routes: Routes = [
  { path: '', redirectTo: 'categories', pathMatch: 'full' },
  {
    path: 'categories',
    loadChildren: () => import('./features/categories/categories.routes').then((m) => m.CATEGORIES_ROUTES),
  },
  { path: '**', redirectTo: 'categories' },
];
