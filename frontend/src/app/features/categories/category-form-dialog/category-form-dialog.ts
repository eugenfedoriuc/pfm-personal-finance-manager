import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { Category } from '../../../core/models/category';
import { ProblemDetails } from '../../../core/models/problem-details';
import { TransactionType } from '../../../core/models/transaction-type';
import { CATEGORY_COLORS, CATEGORY_ICONS } from '../category-options';
import { CategoryStore } from '../category-store';

export type CategoryFormDialogData = { mode: 'create' } | { mode: 'edit'; category: Category };

const TYPE_LABELS: Record<TransactionType, string> = { Income: 'Einnahme', Expense: 'Ausgabe' };

/** Create/edit dialog: name, type (fixed once created), icon and colour from the fixed sets. */
@Component({
  selector: 'app-category-form-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    MatSelectModule,
  ],
  templateUrl: './category-form-dialog.html',
  styleUrl: './category-form-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoryFormDialog {
  protected readonly data = inject<CategoryFormDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<CategoryFormDialog, boolean>);
  private readonly store = inject(CategoryStore);

  protected readonly icons = CATEGORY_ICONS;
  protected readonly colors = CATEGORY_COLORS;
  protected readonly types: readonly TransactionType[] = ['Expense', 'Income'];
  protected readonly typeLabels = TYPE_LABELS;
  protected readonly submitting = signal(false);

  protected readonly isEditMode = this.data.mode === 'edit';
  private readonly existing = this.data.mode === 'edit' ? this.data.category : null;

  private readonly fb = inject(NonNullableFormBuilder);

  protected readonly form = this.fb.group({
    name: [this.existing?.name ?? '', [Validators.required, Validators.maxLength(50)]],
    type: [
      { value: this.existing?.type ?? ('Expense' as TransactionType), disabled: this.isEditMode },
      Validators.required,
    ],
    icon: [this.existing?.icon ?? this.icons[0].value, Validators.required],
    color: [this.existing?.color ?? this.colors[0], Validators.required],
  });

  protected selectColor(color: string): void {
    this.form.controls.color.setValue(color);
  }

  protected submit(): void {
    if (this.form.invalid || this.submitting()) {
      return;
    }

    this.submitting.set(true);
    const { name, type, icon, color } = this.form.getRawValue();

    const request$ = this.existing
      ? this.store.update(this.existing.id, { name, icon, color })
      : this.store.create({ name, type, icon, color });

    request$.subscribe({
      next: () => this.dialogRef.close(true),
      error: (error: unknown) => {
        this.submitting.set(false);
        this.applyServerErrors(error);
      },
    });
  }

  private applyServerErrors(error: unknown): void {
    if (!(error instanceof HttpErrorResponse)) {
      return;
    }

    const problem = error.error as ProblemDetails | null;

    if (error.status === 400 && problem?.errors) {
      for (const [field, messages] of Object.entries(problem.errors)) {
        this.form.get(field)?.setErrors({ server: messages[0] });
      }
      return;
    }

    if (error.status === 409) {
      this.form.controls.name.setErrors({ server: problem?.detail ?? 'Dieser Name wird bereits verwendet.' });
    }
  }
}
