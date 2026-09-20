import { TransactionType } from './transaction-type';

export interface Category {
  id: string;
  name: string;
  type: TransactionType;
  /** Name of a Material Symbol, e.g. "shopping_cart". */
  icon: string;
  /** Hex triplet, e.g. "#EF6C00". */
  color: string;
}

export interface CreateCategoryRequest {
  name: string;
  type: TransactionType;
  icon: string;
  color: string;
}

/** The type cannot be changed once a category exists (transactions and budgets depend on it). */
export interface UpdateCategoryRequest {
  name: string;
  icon: string;
  color: string;
}
