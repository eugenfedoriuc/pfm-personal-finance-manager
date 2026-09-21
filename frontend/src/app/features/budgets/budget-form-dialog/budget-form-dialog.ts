import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { BudgetApiService } from '../../../core/api/budget-api.service';
import { maxTwoDecimals, positiveAmount } from '../../../core/forms/amount-validators';
import { applyServerErrors } from '../../../core/forms/apply-server-errors';
import { Category } from '../../../core/models/category';

export interface BudgetFormDialogData {
  category: Category;
  year: number;
  month: number;
}

/** Quick single-category budget creation, opened from the dashboard's "Budget erstellen" action. */
@Component({
  selector: 'app-budget-form-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './budget-form-dialog.html',
  styleUrl: './budget-form-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BudgetFormDialog {
  protected readonly data = inject<BudgetFormDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<BudgetFormDialog, boolean>);
  private readonly budgetApi = inject(BudgetApiService);
  private readonly fb = inject(NonNullableFormBuilder);

  protected readonly submitting = signal(false);

  protected readonly form = this.fb.group({
    limit: new FormControl<number | null>(null, [Validators.required, positiveAmount, maxTwoDecimals]),
  });

  protected submit(): void {
    const limit = this.form.controls.limit.value;
    if (this.form.invalid || this.submitting() || limit === null) {
      return;
    }

    this.submitting.set(true);
    this.budgetApi
      .upsert({ categoryId: this.data.category.id, year: this.data.year, month: this.data.month, limit })
      .subscribe({
        next: () => this.dialogRef.close(true),
        error: (error: unknown) => {
          this.submitting.set(false);
          applyServerErrors(this.form, error);
        },
      });
  }
}
