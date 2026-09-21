import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { fromDateOnlyString, toDateOnlyString } from '../../../core/dates/date-only';
import { maxTwoDecimals, positiveAmount } from '../../../core/forms/amount-validators';
import { applyServerErrors } from '../../../core/forms/apply-server-errors';
import { Transaction } from '../../../core/models/transaction';
import { TransactionType } from '../../../core/models/transaction-type';
import { CategoryStore } from '../../categories/category-store';
import { TransactionStore } from '../transaction-store';

export type TransactionFormDialogData = { mode: 'create' } | { mode: 'edit'; transaction: Transaction };

const TYPE_LABELS: Record<TransactionType, string> = { Income: 'Einnahme', Expense: 'Ausgabe' };
const MAX_DESCRIPTION_LENGTH = 200;

/** Create/edit dialog: changing the type re-filters the category options and clears a mismatch. */
@Component({
  selector: 'app-transaction-form-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDatepickerModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    MatSelectModule,
  ],
  templateUrl: './transaction-form-dialog.html',
  styleUrl: './transaction-form-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TransactionFormDialog {
  protected readonly data = inject<TransactionFormDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<TransactionFormDialog, boolean>);
  private readonly store = inject(TransactionStore);
  private readonly categoryStore = inject(CategoryStore);
  private readonly fb = inject(NonNullableFormBuilder);

  protected readonly types: readonly TransactionType[] = ['Expense', 'Income'];
  protected readonly typeLabels = TYPE_LABELS;
  protected readonly maxDescriptionLength = MAX_DESCRIPTION_LENGTH;
  protected readonly submitting = signal(false);

  protected readonly isEditMode = this.data.mode === 'edit';
  private readonly existing = this.data.mode === 'edit' ? this.data.transaction : null;

  protected readonly form = this.fb.group({
    type: this.existing?.type ?? ('Expense' as TransactionType),
    amount: new FormControl<number | null>(this.existing?.amount ?? null, [
      Validators.required,
      positiveAmount,
      maxTwoDecimals,
    ]),
    date: this.existing ? fromDateOnlyString(this.existing.date) : new Date(),
    categoryId: [this.existing?.category.id ?? '', Validators.required],
    description: [this.existing?.description ?? '', Validators.maxLength(MAX_DESCRIPTION_LENGTH)],
  });

  protected readonly filteredCategories = computed(() => {
    const type = this.currentType();
    return this.categoryStore.categories().filter((category) => category.type === type);
  });

  private readonly currentType = signal(this.form.controls.type.value);

  constructor() {
    this.form.controls.type.valueChanges.pipe(takeUntilDestroyed()).subscribe((type) => {
      this.currentType.set(type);

      const categoryId = this.form.controls.categoryId.value;
      const stillMatches = this.categoryStore.categories().some((c) => c.id === categoryId && c.type === type);
      if (!stillMatches) {
        this.form.controls.categoryId.setValue('');
      }
    });
  }

  protected submit(): void {
    if (this.form.invalid || this.submitting()) {
      return;
    }

    this.submitting.set(true);
    const { type, amount, date, categoryId, description } = this.form.getRawValue();
    const trimmedDescription = description.trim();
    const request = {
      type,
      amount: amount!,
      date: toDateOnlyString(date),
      categoryId,
      description: trimmedDescription.length === 0 ? null : trimmedDescription,
    };

    const request$ = this.existing
      ? this.store.update(this.existing.id, request)
      : this.store.create(request);

    request$.subscribe({
      next: () => this.dialogRef.close(true),
      error: (error: unknown) => {
        this.submitting.set(false);
        applyServerErrors(this.form, error);
      },
    });
  }
}
