import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { forkJoin } from 'rxjs';
import { BudgetApiService } from '../../../core/api/budget-api.service';
import { SummaryApiService } from '../../../core/api/summary-api.service';
import { maxTwoDecimals, positiveAmount } from '../../../core/forms/amount-validators';
import { applyServerErrors } from '../../../core/forms/apply-server-errors';
import { Category } from '../../../core/models/category';
import { NotificationService } from '../../../core/services/notification.service';
import { MonthStateService } from '../../../core/state/month-state.service';
import { createMinDurationLoading } from '../../../core/utils/min-duration-loading';
import { BudgetStatus } from '../../../shared/budget-status/budget-status';
import { LoadingIndicator } from '../../../shared/loading-indicator/loading-indicator';
import { Money } from '../../../shared/money/money';
import { MonthSwitcher } from '../../../shared/month-switcher/month-switcher';
import { CategoryStore } from '../../categories/category-store';

interface BudgetRow {
  category: Category;
  spent: number;
  isOverBudget: boolean;
  overBy: number | null;
  saving: boolean;
  group: FormGroup<{ limit: FormControl<number | null> }>;
}

/** One row per expense category, with a per-row save enabled only while that row is dirty. */
@Component({
  selector: 'app-budget-list',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressBarModule,
    BudgetStatus,
    LoadingIndicator,
    Money,
    MonthSwitcher,
  ],
  templateUrl: './budget-list.html',
  styleUrl: './budget-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BudgetList {
  private readonly budgetApi = inject(BudgetApiService);
  private readonly summaryApi = inject(SummaryApiService);
  private readonly categoryStore = inject(CategoryStore);
  private readonly monthState = inject(MonthStateService);
  private readonly notifications = inject(NotificationService);

  private readonly loadingCtl = createMinDurationLoading();

  protected readonly loading = this.loadingCtl.loading;
  protected readonly loadError = signal(false);
  protected readonly rows = signal<BudgetRow[]>([]);
  protected readonly showInitialLoader = computed(() => this.loading() && this.rows().length === 0);

  constructor() {
    this.categoryStore.load();

    effect(() => {
      const { year, month } = this.monthState.selected();
      const categories = this.categoryStore.expenseCategories();
      if (categories.length > 0) {
        this.loadMonth(year, month, categories);
      }
    });
  }

  protected reload(): void {
    const { year, month } = this.monthState.selected();
    this.loadMonth(year, month, this.categoryStore.expenseCategories());
  }

  protected saveRow(row: BudgetRow): void {
    const limit = row.group.controls.limit.value;
    if (row.group.invalid || row.group.pristine || limit === null || row.saving) {
      return;
    }

    row.saving = true;
    this.rows.set([...this.rows()]);

    const { year, month } = this.monthState.selected();
    this.budgetApi.upsert({ categoryId: row.category.id, year, month, limit }).subscribe({
      next: () => {
        row.saving = false;
        row.isOverBudget = row.spent > limit;
        row.overBy = Math.max(0, row.spent - limit);
        row.group.markAsPristine();
        this.rows.set([...this.rows()]);
        this.notifications.success(`Budget f\u00fcr ${row.category.name} gespeichert.`);
      },
      error: (error: unknown) => {
        row.saving = false;
        this.rows.set([...this.rows()]);
        applyServerErrors(row.group, error);
      },
    });
  }

  private loadMonth(year: number, month: number, categories: readonly Category[]): void {
    this.loadingCtl.start();
    this.loadError.set(false);

    forkJoin({
      budgets: this.budgetApi.getForMonth(year, month),
      summary: this.summaryApi.get(year, month),
    }).subscribe({
      next: ({ budgets, summary }) => {
        this.rows.set(
          categories.map((category) => {
            const budget = budgets.find((b) => b.category.id === category.id);
            const comparison = summary.budgetComparison.find((item) => item.categoryId === category.id);

            return {
              category,
              spent: comparison?.spent ?? 0,
              isOverBudget: comparison?.isOverBudget ?? false,
              overBy: comparison?.overBy ?? null,
              saving: false,
              group: new FormGroup({
                limit: new FormControl<number | null>(budget?.limit ?? null, [
                  Validators.required,
                  positiveAmount,
                  maxTwoDecimals,
                ]),
              }),
            };
          }),
        );
        this.loadingCtl.stop();
      },
      error: () => {
        this.loadError.set(true);
        this.loadingCtl.stop();
      },
    });
  }
}
