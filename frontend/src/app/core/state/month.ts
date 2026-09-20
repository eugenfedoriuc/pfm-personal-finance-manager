/** The month a screen is scoped to. Matches the `?month=YYYY-MM` query parameter. */
export interface MonthValue {
  year: number;
  month: number;
}

const MONTH_PARAM_PATTERN = /^(\d{4})-(\d{2})$/;

export function currentMonth(): MonthValue {
  const today = new Date();
  return { year: today.getFullYear(), month: today.getMonth() + 1 };
}

export function parseMonthParam(value: string | null): MonthValue | null {
  const match = value ? MONTH_PARAM_PATTERN.exec(value) : null;
  if (!match) {
    return null;
  }

  const year = Number(match[1]);
  const month = Number(match[2]);
  return month >= 1 && month <= 12 ? { year, month } : null;
}

export function formatMonthParam(value: MonthValue): string {
  return `${value.year.toString().padStart(4, '0')}-${value.month.toString().padStart(2, '0')}`;
}

export function addMonths(value: MonthValue, delta: number): MonthValue {
  // Zero-based month arithmetic lets Date roll the year over in both directions.
  const zeroBased = value.month - 1 + delta;
  const date = new Date(value.year, zeroBased, 1);
  return { year: date.getFullYear(), month: date.getMonth() + 1 };
}
