import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/** Rejects zero, negative and empty values. Pair with `maxTwoDecimals` for a full amount check. */
export const positiveAmount: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = control.value as number | null;
  return value === null || value > 0 ? null : { notPositive: true };
};

/** Rejects amounts with more than two decimal places, e.g. 12.345. */
export const maxTwoDecimals: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = control.value as number | null;
  return value === null || Math.round(value * 100) / 100 === value ? null : { tooManyDecimals: true };
};
