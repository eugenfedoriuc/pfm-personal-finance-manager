import { Category } from '../../core/models/category';

/** Fixed icon choices for the category form, picked from Material Symbols. */
export interface IconOption {
  value: string;
  label: string;
}

export const CATEGORY_ICONS: readonly IconOption[] = [
  { value: 'shopping_cart', label: 'Einkaufen' },
  { value: 'home', label: 'Miete' },
  { value: 'restaurant', label: 'Restaurant' },
  { value: 'sports_esports', label: 'Freizeit' },
  { value: 'directions_car', label: 'Transport' },
  { value: 'savings', label: 'Sparen' },
  { value: 'local_hospital', label: 'Gesundheit' },
  { value: 'school', label: 'Bildung' },
  { value: 'flight', label: 'Reisen' },
  { value: 'checkroom', label: 'Kleidung' },
  { value: 'pets', label: 'Haustiere' },
  { value: 'payments', label: 'Gehalt' },
];

/** Fixed colour palette for the category form, as uppercase hex triplets. */
export const CATEGORY_COLORS: readonly string[] = [
  '#2E7D32',
  '#1565C0',
  '#EF6C00',
  '#6A1B9A',
  '#00838F',
  '#AD1457',
  '#C62828',
  '#4E342E',
  '#37474F',
  '#9E9D24',
];

export function sortByName(categories: readonly Category[]): Category[] {
  return [...categories].sort((a, b) => a.name.localeCompare(b.name, 'de-AT'));
}
