import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { SummaryApiService } from '../../core/api/summary-api.service';
import { MonthlySummary } from '../../core/models/summary';
import { MonthStateService } from '../../core/state/month-state.service';

/**
 * The monthly summary, reloaded whenever the selected month changes. Shared by the dashboard and
 * the category detail screen, since both are scoped to the same month and the brief explicitly
 * has category detail reuse the summary instead of adding a second endpoint.
 */
@Injectable({ providedIn: 'root' })
export class SummaryStore {
  private readonly api = inject(SummaryApiService);
  private readonly monthState = inject(MonthStateService);

  private readonly _summary = signal<MonthlySummary | null>(null);
  private readonly _loading = signal(false);
  private readonly _loadError = signal(false);

  readonly summary = this._summary.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly loadError = this._loadError.asReadonly();
  readonly isReady = computed(() => !this._loading() && !this._loadError() && this._summary() !== null);

  constructor() {
    effect(() => {
      const { year, month } = this.monthState.selected();
      this.load(year, month);
    });
  }

  reload(): void {
    const { year, month } = this.monthState.selected();
    this.load(year, month);
  }

  private load(year: number, month: number): void {
    this._loading.set(true);
    this._loadError.set(false);

    this.api.get(year, month).subscribe({
      next: (summary) => {
        this._summary.set(summary);
        this._loading.set(false);
      },
      error: () => {
        this._loadError.set(true);
        this._loading.set(false);
      },
    });
  }
}
