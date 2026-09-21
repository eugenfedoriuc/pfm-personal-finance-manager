import { HttpErrorResponse } from '@angular/common/http';
import { FormGroup } from '@angular/forms';
import { ProblemDetails } from '../models/problem-details';

/**
 * Maps a failed request onto the form that made it: field errors from a 400 go onto the matching
 * control; a 409 (e.g. a duplicate name) has no field to blame, so it goes onto `fallbackControl`.
 * Returns whether the error was recognised and applied.
 */
export function applyServerErrors(form: FormGroup, error: unknown, fallbackControl?: string): boolean {
  if (!(error instanceof HttpErrorResponse)) {
    return false;
  }

  const problem = error.error as ProblemDetails | null;

  if (error.status === 400 && problem?.errors) {
    for (const [field, messages] of Object.entries(problem.errors)) {
      form.get(field)?.setErrors({ server: messages[0] });
    }
    return true;
  }

  if (error.status === 409 && fallbackControl) {
    form.get(fallbackControl)?.setErrors({ server: problem?.detail ?? 'Dieser Wert wird bereits verwendet.' });
    return true;
  }

  return false;
}
