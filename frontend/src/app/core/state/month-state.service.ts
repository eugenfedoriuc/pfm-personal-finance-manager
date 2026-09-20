import { toSignal } from '@angular/core/rxjs-interop';
import { Injectable, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { map } from 'rxjs';
import { MonthValue, addMonths, currentMonth, formatMonthParam, parseMonthParam } from './month';

/**
 * The month selected across the whole app, kept in the `?month=YYYY-MM` query parameter so it
 * survives navigation and reloads. Every month-scoped feature reads `selected` and calls `set`
 * instead of keeping its own copy.
 */
@Injectable({ providedIn: 'root' })
export class MonthStateService {
  private readonly router = inject(Router);

  private readonly monthParam = toSignal(
    this.router.routerState.root.queryParamMap.pipe(map((params) => params.get('month'))),
    { initialValue: this.router.routerState.snapshot.root.queryParamMap.get('month') },
  );

  readonly selected = computed<MonthValue>(() => parseMonthParam(this.monthParam()) ?? currentMonth());

  next(): void {
    this.set(addMonths(this.selected(), 1));
  }

  previous(): void {
    this.set(addMonths(this.selected(), -1));
  }

  set(value: MonthValue): void {
    void this.router.navigate([], {
      queryParams: { month: formatMonthParam(value) },
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });
  }
}
