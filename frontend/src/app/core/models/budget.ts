import { Category } from './category';

export interface Budget {
  id: string;
  category: Category;
  year: number;
  month: number;
  limit: number;
}

/** Identifies the budget by category, year and month: the same request creates or replaces it. */
export interface UpsertBudgetRequest {
  categoryId: string;
  year: number;
  month: number;
  limit: number;
}
