import { ChangeDetectionStrategy, Component, OnInit, computed, effect, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { map } from 'rxjs';
import { CategoryBreakdownItem } from '../../../core/models/summary';
import { Transaction, TransactionQuery } from '../../../core/models/transaction';
import { MonthStateService } from '../../../core/state/month-state.service';
import { ConfirmDialog } from '../../../shared/confirm-dialog/confirm-dialog';
import { Money } from '../../../shared/money/money';
import { MonthSwitcher } from '../../../shared/month-switcher/month-switcher';
import { BudgetStatus } from '../../../shared/budget-status/budget-status';
import { TransactionTable } from '../../../shared/transaction-table/transaction-table';
import { BudgetFormDialog } from '../../budgets/budget-form-dialog/budget-form-dialog';
import { CategoryStore } from '../../categories/category-store';
import {
  TransactionFormDialog,
  TransactionFormDialogData,
} from '../../transactions/transaction-form-dialog/transaction-form-dialog';
import { TransactionStore } from '../../transactions/transaction-store';
import { SummaryStore } from '../summary-store';

/** Reuses the monthly summary and a category-filtered transaction query; no dedicated endpoint. */
@Component({
  selector: 'app-category-detail',
  imports: [
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressBarModule,
    BudgetStatus,
    Money,
    MonthSwitcher,
    TransactionTable,
  ],
  templateUrl: './category-detail.html',
  styleUrl: './category-detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoryDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly summaryStore = inject(SummaryStore);
  private readonly transactionStore = inject(TransactionStore);
  private readonly categoryStore = inject(CategoryStore);
  private readonly monthState = inject(MonthStateService);
  private readonly dialog = inject(MatDialog);

  private readonly categoryId = toSignal(
    this.route.paramMap.pipe(map((params) => params.get('id') ?? '')),
    { requireSync: true },
  );

  protected readonly loading = this.transactionStore.loading;
  protected readonly loadError = this.transactionStore.loadError;
  protected readonly isEmpty = this.transactionStore.isEmpty;
  protected readonly transactions = this.transactionStore.transactions;

  protected readonly category = computed(
    () => this.categoryStore.categories().find((category) => category.id === this.categoryId()) ?? null,
  );

  protected readonly breakdown = computed<CategoryBreakdownItem | null>(() => {
    const summary = this.summaryStore.summary();
    if (!summary) {
      return null;
    }

    const id = this.categoryId();
    return (
      summary.incomeBreakdown.find((item) => item.categoryId === id) ??
      summary.expenseBreakdown.find((item) => item.categoryId === id) ??
      null
    );
  });

  protected readonly budgetComparison = computed(
    () => this.summaryStore.summary()?.budgetComparison.find((item) => item.categoryId === this.categoryId()) ?? null,
  );

  private readonly currentQuery = computed<TransactionQuery>(() => {
    const { year, month } = this.monthState.selected();
    return { year, month, categoryId: this.categoryId() };
  });

  constructor() {
    effect(() => this.transactionStore.load(this.currentQuery()));
  }

  ngOnInit(): void {
    this.categoryStore.load();
  }

  protected reload(): void {
    this.transactionStore.load(this.currentQuery());
  }

  protected openEditDialog(transaction: Transaction): void {
    this.openFormDialog({ mode: 'edit', transaction });
  }

  protected openDeleteDialog(transaction: Transaction): void {
    this.dialog
      .open(ConfirmDialog, {
        data: {
          title: 'Transaktion löschen',
          message: `Soll die Transaktion "${transaction.category.name}" wirklich gelöscht werden?`,
        },
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (confirmed) {
          this.transactionStore.delete(transaction.id).subscribe({
            next: () => this.onMutated(),
            error: () => undefined,
          });
        }
      });
  }

  protected openBudgetDialog(): void {
    const category = this.category();
    if (!category) {
      return;
    }

    const { year, month } = this.monthState.selected();

    this.dialog
      .open(BudgetFormDialog, { data: { category, year, month } })
      .afterClosed()
      .subscribe((created) => {
        if (created) {
          this.summaryStore.reload();
        }
      });
  }

  private openFormDialog(data: TransactionFormDialogData): void {
    this.dialog
      .open(TransactionFormDialog, { data })
      .afterClosed()
      .subscribe((saved) => {
        if (saved) {
          this.onMutated();
        }
      });
  }

  private onMutated(): void {
    this.reload();
    this.summaryStore.reload();
  }
}
