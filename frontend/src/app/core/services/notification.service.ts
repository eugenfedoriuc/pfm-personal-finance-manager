import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

/** Toasts for the outcome of user-triggered actions. HTTP failures already surface via the global
 * error interceptor, so this is only used for success confirmations (create/update/delete etc). */
@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly snackBar = inject(MatSnackBar);

  success(message: string): void {
    this.snackBar.open(message, undefined, {
      duration: 3500,
      panelClass: 'pfm-snackbar--success',
    });
  }
}
