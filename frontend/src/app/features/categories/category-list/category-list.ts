import { ChangeDetectionStrategy, Component, OnInit, computed, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Category } from '../../../core/models/category';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmDialog } from '../../../shared/confirm-dialog/confirm-dialog';
import { LoadingIndicator } from '../../../shared/loading-indicator/loading-indicator';
import { CategoryFormDialog, CategoryFormDialogData } from '../category-form-dialog/category-form-dialog';
import { CategoryRow } from '../category-row/category-row';
import { CategoryStore } from '../category-store';

@Component({
  selector: 'app-category-list',
  imports: [MatButtonModule, MatCardModule, MatIconModule, MatProgressBarModule, CategoryRow, LoadingIndicator],
  templateUrl: './category-list.html',
  styleUrl: './category-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoryList implements OnInit {
  private readonly store = inject(CategoryStore);
  private readonly dialog = inject(MatDialog);
  private readonly notifications = inject(NotificationService);

  protected readonly loading = this.store.loading;
  protected readonly loadError = this.store.loadError;
  protected readonly isEmpty = this.store.isEmpty;
  protected readonly incomeCategories = this.store.incomeCategories;
  protected readonly expenseCategories = this.store.expenseCategories;
  protected readonly showInitialLoader = computed(() => this.loading() && this.store.categories().length === 0);

  ngOnInit(): void {
    this.store.load();
  }

  protected reload(): void {
    this.store.load();
  }

  protected openCreateDialog(): void {
    this.openFormDialog({ mode: 'create' });
  }

  protected openEditDialog(category: Category): void {
    this.openFormDialog({ mode: 'edit', category });
  }

  protected openDeleteDialog(category: Category): void {
    this.dialog
      .open(ConfirmDialog, {
        data: {
          title: 'Kategorie löschen',
          message: `Soll "${category.name}" wirklich gelöscht werden?`,
        },
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (confirmed) {
          this.store.delete(category.id).subscribe({
            next: () => this.notifications.success('Kategorie gelöscht.'),
            error: () => undefined,
          });
        }
      });
  }

  private openFormDialog(data: CategoryFormDialogData): void {
    this.dialog
      .open(CategoryFormDialog, { data })
      .afterClosed()
      .subscribe((saved) => {
        if (saved) {
          this.notifications.success(data.mode === 'edit' ? 'Kategorie aktualisiert.' : 'Kategorie erstellt.');
        }
      });
  }
}
