/** What one category contributed to the income or expenses of a month. */
export interface CategoryBreakdownItem {
  categoryId: string;
  name: string;
  icon: string;
  color: string;
  amount: number;
  transactionCount: number;
  /** Share of the month's total, 0 to 100. */
  percentage: number;
}

/**
 * Budget against reality for one expense category. `limit`, `remaining` and `overBy` are all
 * null together when the category has no budget for the month.
 */
export interface BudgetComparisonItem {
  categoryId: string;
  categoryName: string;
  limit: number | null;
  spent: number;
  remaining: number | null;
  isOverBudget: boolean;
  overBy: number | null;
}

/** Expenses of a single day, plus the running total since the first of the month. */
export interface DailyExpenseItem {
  date: string;
  amount: number;
  cumulative: number;
}

export interface MonthlySummary {
  year: number;
  month: number;
  totalIncome: number;
  totalExpenses: number;
  balance: number;
  incomeBreakdown: CategoryBreakdownItem[];
  expenseBreakdown: CategoryBreakdownItem[];
  budgetComparison: BudgetComparisonItem[];
  topExpenseCategory: CategoryBreakdownItem | null;
  dailyExpenses: DailyExpenseItem[];
}
