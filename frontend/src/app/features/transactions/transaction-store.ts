import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { TransactionApiService } from '../../core/api/transaction-api.service';
import { Transaction, TransactionQuery, TransactionRequest } from '../../core/models/transaction';

/**
 * Signal-based state for the transactions screen. Unlike categories, a mutation can move a
 * transaction in or out of the current month/type/category filter, so create/update/delete only
 * perform the request; the component reloads the current query afterwards.
 */
@Injectable({ providedIn: 'root' })
export class TransactionStore {
  private readonly api = inject(TransactionApiService);

  private readonly _transactions = signal<Transaction[]>([]);
  private readonly _loading = signal(false);
  private readonly _loadError = signal(false);

  readonly transactions = this._transactions.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly loadError = this._loadError.asReadonly();
  readonly isEmpty = computed(() => !this._loading() && !this._loadError() && this._transactions().length === 0);

  load(query: TransactionQuery): void {
    this._loading.set(true);
    this._loadError.set(false);

    this.api.get(query).subscribe({
      next: (transactions) => {
        this._transactions.set(transactions);
        this._loading.set(false);
      },
      error: () => {
        this._loadError.set(true);
        this._loading.set(false);
      },
    });
  }

  create(request: TransactionRequest): Observable<Transaction> {
    return this.api.create(request);
  }

  update(id: string, request: TransactionRequest): Observable<Transaction> {
    return this.api.update(id, request);
  }

  delete(id: string): Observable<void> {
    return this.api.delete(id);
  }
}
