import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';
import { ProblemDetails } from '../models/problem-details';

/**
 * Turns a failing request into a snackbar message. Field-level validation errors (400 with an
 * `errors` object) are left to the form that made the request, which maps them onto the matching
 * controls — showing a toast for those as well would just repeat what the form already displays.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse) {
        const problem = error.error as ProblemDetails | null;

        if (!problem?.errors) {
          snackBar.open(describe(error, problem), 'Schließen', { duration: 6000, panelClass: 'pfm-snackbar--error' });
        }
      }

      return throwError(() => error);
    }),
  );
};

function describe(error: HttpErrorResponse, problem: ProblemDetails | null): string {
  if (error.status === 0) {
    return 'Der Server ist nicht erreichbar.';
  }

  return problem?.detail ?? problem?.title ?? 'Die Anfrage konnte nicht verarbeitet werden.';
}
