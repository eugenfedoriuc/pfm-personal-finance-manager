import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { CategoryApiService } from '../../core/api/category-api.service';
import { createMinDurationLoading } from '../../core/utils/min-duration-loading';
import { Category, CreateCategoryRequest, UpdateCategoryRequest } from '../../core/models/category';
import { sortByName } from './category-options';

/** Signal-based state for the categories screen: one load, kept in sync by every mutation. */
@Injectable({ providedIn: 'root' })
export class CategoryStore {
  private readonly api = inject(CategoryApiService);

  private readonly _categories = signal<Category[]>([]);
  private readonly loadingCtl = createMinDurationLoading();
  private readonly _loadError = signal(false);

  readonly categories = this._categories.asReadonly();
  readonly loading = this.loadingCtl.loading;
  readonly loadError = this._loadError.asReadonly();
  readonly isEmpty = computed(() => !this.loading() && !this._loadError() && this._categories().length === 0);
  readonly incomeCategories = computed(() => this._categories().filter((category) => category.type === 'Income'));
  readonly expenseCategories = computed(() => this._categories().filter((category) => category.type === 'Expense'));

  /** Safe to call from every feature that needs the category list: a second call is a no-op. */
  load(): void {
    if (this._categories().length > 0 || this.loading()) {
      return;
    }

    this.loadingCtl.start();
    this._loadError.set(false);

    this.api.getAll().subscribe({
      next: (categories) => {
        this._categories.set(sortByName(categories));
        this.loadingCtl.stop();
      },
      error: () => {
        this._loadError.set(true);
        this.loadingCtl.stop();
      },
    });
  }

  create(request: CreateCategoryRequest): Observable<Category> {
    return this.api
      .create(request)
      .pipe(tap((created) => this._categories.set(sortByName([...this._categories(), created]))));
  }

  update(id: string, request: UpdateCategoryRequest): Observable<Category> {
    return this.api.update(id, request).pipe(
      tap((updated) =>
        this._categories.set(sortByName(this._categories().map((category) => (category.id === id ? updated : category)))),
      ),
    );
  }

  delete(id: string): Observable<void> {
    return this.api
      .delete(id)
      .pipe(tap(() => this._categories.set(this._categories().filter((category) => category.id !== id))));
  }
}
