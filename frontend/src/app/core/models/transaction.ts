import { Category } from './category';
import { TransactionType } from './transaction-type';

export interface Transaction {
  id: string;
  type: TransactionType;
  amount: number;
  /** Calendar day of the booking, formatted as yyyy-MM-dd. */
  date: string;
  category: Category;
  description: string | null;
}

/** Shared by create and update: both operations take the same fields. */
export interface TransactionRequest {
  type: TransactionType;
  amount: number;
  date: string;
  categoryId: string;
  description: string | null;
}

export interface TransactionQuery {
  year: number;
  month: number;
  type?: TransactionType;
  categoryId?: string;
}
