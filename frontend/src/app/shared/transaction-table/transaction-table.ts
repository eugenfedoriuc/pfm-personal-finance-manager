import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { formatDateOnly } from '../../core/dates/date-only';
import { Transaction } from '../../core/models/transaction';
import { Money } from '../money/money';

/**
 * A transactions table, reused by the full transactions list and by the category detail screen
 * (which hides the category column, since every row already belongs to the same category).
 */
@Component({
  selector: 'app-transaction-table',
  imports: [MatButtonModule, MatIconModule, MatTableModule, Money],
  templateUrl: './transaction-table.html',
  styleUrl: './transaction-table.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TransactionTable {
  readonly transactions = input.required<readonly Transaction[]>();
  readonly showCategory = input(true);
  readonly edit = output<Transaction>();
  readonly delete = output<Transaction>();

  protected readonly formatDate = formatDateOnly;

  protected readonly displayedColumns = computed(() =>
    this.showCategory()
      ? ['date', 'category', 'description', 'amount', 'actions']
      : ['date', 'description', 'amount', 'actions'],
  );

  protected signedAmount(transaction: Transaction): number {
    return transaction.type === 'Expense' ? -transaction.amount : transaction.amount;
  }
}
