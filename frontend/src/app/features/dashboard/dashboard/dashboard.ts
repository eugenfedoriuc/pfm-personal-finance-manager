import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ChartConfiguration } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { toDateOnlyString } from '../../../core/dates/date-only';
import { CategoryBreakdownItem } from '../../../core/models/summary';
import { currentMonth } from '../../../core/state/month';
import { MonthStateService } from '../../../core/state/month-state.service';
import { BudgetStatus } from '../../../shared/budget-status/budget-status';
import { Money } from '../../../shared/money/money';
import { MonthSwitcher } from '../../../shared/month-switcher/month-switcher';
import { BudgetFormDialog } from '../../budgets/budget-form-dialog/budget-form-dialog';
import { SummaryStore } from '../summary-store';

@Component({
  selector: 'app-dashboard',
  imports: [
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressBarModule,
    BaseChartDirective,
    BudgetStatus,
    Money,
    MonthSwitcher,
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Dashboard {
  private readonly summaryStore = inject(SummaryStore);
  private readonly monthState = inject(MonthStateService);
  private readonly dialog = inject(MatDialog);

  protected readonly loading = this.summaryStore.loading;
  protected readonly loadError = this.summaryStore.loadError;
  protected readonly summary = this.summaryStore.summary;

  protected readonly budgetByCategory = computed(() => {
    const items = this.summary()?.budgetComparison ?? [];
    return new Map(items.map((item) => [item.categoryId, item]));
  });

  protected readonly chartData = computed<ChartConfiguration<'line'>['data']>(() => {
    const summary = this.summary();
    if (!summary) {
      return { labels: [], datasets: [] };
    }

    const selected = this.monthState.selected();
    const current = currentMonth();
    const isCurrentMonth = selected.year === current.year && selected.month === current.month;
    const todayIso = toDateOnlyString(new Date());

    const points = isCurrentMonth
      ? summary.dailyExpenses.filter((day) => day.date <= todayIso)
      : summary.dailyExpenses;

    return {
      labels: points.map((day) => Number(day.date.slice(8))),
      datasets: [
        {
          label: 'Ausgaben (kumuliert)',
          data: points.map((day) => day.cumulative),
          fill: true,
          tension: 0.3,
          // The last point (today, or the month's end) gets a visible marker; the rest stay bare.
          pointRadius: points.map((_, index) => (index === points.length - 1 ? 5 : 0)),
          pointHoverRadius: 5,
        },
      ],
    };
  });

  protected readonly chartOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    scales: { y: { beginAtZero: true } },
    plugins: { legend: { display: false } },
  };

  protected reload(): void {
    this.summaryStore.reload();
  }

  protected isTopCategory(categoryId: string): boolean {
    return this.summary()?.topExpenseCategory?.categoryId === categoryId;
  }

  protected openBudgetDialog(item: CategoryBreakdownItem): void {
    const { year, month } = this.monthState.selected();

    this.dialog
      .open(BudgetFormDialog, {
        data: {
          category: { id: item.categoryId, name: item.name, icon: item.icon, color: item.color, type: 'Expense' },
          year,
          month,
        },
      })
      .afterClosed()
      .subscribe((created) => {
        if (created) {
          this.summaryStore.reload();
        }
      });
  }
}
