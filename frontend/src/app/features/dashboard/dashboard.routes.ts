import { Routes } from '@angular/router';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';

export const DASHBOARD_ROUTES: Routes = [
  {
    path: '',
    // Chart.js is sizeable, so it is only pulled in once the dashboard route actually loads.
    providers: [provideCharts(withDefaultRegisterables())],
    loadComponent: () => import('./dashboard/dashboard').then((m) => m.Dashboard),
  },
  {
    path: 'categories/:id',
    loadComponent: () => import('./category-detail/category-detail').then((m) => m.CategoryDetail),
  },
];
