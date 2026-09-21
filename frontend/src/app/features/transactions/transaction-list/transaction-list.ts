import { ChangeDetectionStrategy, Component, OnInit, computed, effect, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MonthStateService } from '../../../core/state/month-state.service';
import { Transaction, TransactionQuery } from '../../../core/models/transaction';
import { TransactionType } from '../../../core/models/transaction-type';
import { ConfirmDialog } from '../../../shared/confirm-dialog/confirm-dialog';
import { MonthSwitcher } from '../../../shared/month-switcher/month-switcher';
import { TransactionTable } from '../../../shared/transaction-table/transaction-table';
import { CategoryStore } from '../../categories/category-store';
import {
  TransactionFormDialog,
  TransactionFormDialogData,
} from '../transaction-form-dialog/transaction-form-dialog';
import { TransactionStore } from '../transaction-store';

const TYPE_LABELS: Record<TransactionType, string> = { Income: 'Einnahme', Expense: 'Ausgabe' };

@Component({
  selector: 'app-transaction-list',
  imports: [
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatProgressBarModule,
    MatSelectModule,
    MonthSwitcher,
    TransactionTable,
  ],
  templateUrl: './transaction-list.html',
  styleUrl: './transaction-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TransactionList implements OnInit {
  private readonly store = inject(TransactionStore);
  private readonly categoryStore = inject(CategoryStore);
  private readonly monthState = inject(MonthStateService);
  private readonly dialog = inject(MatDialog);

  protected readonly typeLabels = TYPE_LABELS;

  protected readonly loading = this.store.loading;
  protected readonly loadError = this.store.loadError;
  protected readonly isEmpty = this.store.isEmpty;
  protected readonly transactions = this.store.transactions;

  protected readonly typeFilter = signal<TransactionType | ''>('');
  protected readonly categoryFilter = signal<string>('');

  protected readonly filterCategories = computed(() => {
    const type = this.typeFilter();
    const categories = this.categoryStore.categories();
    return type ? categories.filter((category) => category.type === type) : categories;
  });

  private readonly currentQuery = computed<TransactionQuery>(() => {
    const { year, month } = this.monthState.selected();
    const type = this.typeFilter() || undefined;
    const categoryId = this.categoryFilter() || undefined;
    return { year, month, type, categoryId };
  });

  constructor() {
    effect(() => this.store.load(this.currentQuery()));
  }

  ngOnInit(): void {
    this.categoryStore.load();
  }

  protected onTypeFilterChange(type: TransactionType | ''): void {
    this.typeFilter.set(type);

    const categoryId = this.categoryFilter();
    const stillMatches = this.filterCategories().some((category) => category.id === categoryId);
    if (!stillMatches) {
      this.categoryFilter.set('');
    }
  }

  protected reload(): void {
    this.store.load(this.currentQuery());
  }

  protected openCreateDialog(): void {
    this.openFormDialog({ mode: 'create' });
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
          this.store.delete(transaction.id).subscribe({ next: () => this.reload(), error: () => undefined });
        }
      });
  }

  private openFormDialog(data: TransactionFormDialogData): void {
    this.dialog
      .open(TransactionFormDialog, { data })
      .afterClosed()
      .subscribe((saved) => {
        if (saved) {
          this.reload();
        }
      });
  }
}
