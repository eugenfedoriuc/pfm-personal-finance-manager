/** Converts between a `DateOnly` string ("yyyy-MM-dd") and a local `Date` at midnight. */
export function toDateOnlyString(date: Date): string {
  const year = date.getFullYear().toString().padStart(4, '0');
  const month = (date.getMonth() + 1).toString().padStart(2, '0');
  const day = date.getDate().toString().padStart(2, '0');
  return `${year}-${month}-${day}`;
}

export function fromDateOnlyString(value: string): Date {
  const [year, month, day] = value.split('-').map(Number);
  return new Date(year, month - 1, day);
}

/**
 * Formats a `DateOnly` string as "dd.MM.yyyy" by rearranging the digits directly, without ever
 * going through a `Date` — parsing "yyyy-MM-dd" as a `Date` and formatting it in the browser's
 * local time zone can roll the day over near a time zone boundary.
 */
export function formatDateOnly(value: string): string {
  const [year, month, day] = value.split('-');
  return `${day}.${month}.${year}`;
}
