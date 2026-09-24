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
import { ThemeService } from '../../../core/services/theme.service';
import { NotificationService } from '../../../core/services/notification.service';
import { BudgetStatus } from '../../../shared/budget-status/budget-status';
import { LoadingIndicator } from '../../../shared/loading-indicator/loading-indicator';
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
    LoadingIndicator,
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
  private readonly themeService = inject(ThemeService);
  private readonly dialog = inject(MatDialog);
  private readonly notifications = inject(NotificationService);

  // Chart.js reads plain color strings, not CSS custom properties, so the dark-mode palette is
  // spelled out here to match the `--mat-sys-*` overrides in styles.scss.
  private readonly chartPalette = computed(() => {
    const dark = this.themeService.mode() === 'dark';
    return {
      line: dark ? '#ff8f5c' : '#d04a02',
      fill: dark ? 'rgba(255, 143, 92, 0.18)' : 'rgba(208, 74, 2, 0.12)',
      grid: dark ? 'rgba(211, 201, 191, 0.16)' : 'rgba(77, 70, 64, 0.12)',
      text: dark ? '#d3c9bf' : '#4d4640',
    };
  });

  protected readonly loading = this.summaryStore.loading;
  protected readonly loadError = this.summaryStore.loadError;
  protected readonly summary = this.summaryStore.summary;
  protected readonly showInitialLoader = computed(() => this.loading() && this.summary() === null);

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
    const palette = this.chartPalette();

    return {
      labels: points.map((day) => Number(day.date.slice(8))),
      datasets: [
        {
          label: 'Ausgaben (kumuliert)',
          data: points.map((day) => day.cumulative),
          fill: true,
          tension: 0.3,
          borderColor: palette.line,
          backgroundColor: palette.fill,
          pointBackgroundColor: palette.line,
          pointBorderColor: palette.line,
          // A marker on every day spending actually changed, plus the last point (today, or the
          // month's end) even if it's flat, so there's always somewhere to see the current total.
          pointRadius: points.map((day, index) => {
            if (index === points.length - 1) {
              return 5;
            }
            const changed = index === 0 ? day.cumulative !== 0 : day.cumulative !== points[index - 1].cumulative;
            return changed ? 3 : 0;
          }),
          pointHoverRadius: 5,
          // Keeps every point (including the bare, radius-0 ones) hoverable for its tooltip.
          pointHitRadius: 10,
        },
      ],
    };
  });

  protected readonly chartOptions = computed<ChartConfiguration<'line'>['options']>(() => {
    const palette = this.chartPalette();
    return {
      responsive: true,
      maintainAspectRatio: false,
      // Lets the tooltip follow the mouse to the nearest point along x, instead of only firing
      // when the cursor sits exactly on top of a (mostly invisible, radius-0) point.
      interaction: { mode: 'index', intersect: false },
      scales: {
        x: { ticks: { color: palette.text }, grid: { color: palette.grid } },
        y: { beginAtZero: true, ticks: { color: palette.text }, grid: { color: palette.grid } },
      },
      plugins: { legend: { display: false } },
    };
  });

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
          this.notifications.success('Budget erstellt.');
        }
      });
  }
}
