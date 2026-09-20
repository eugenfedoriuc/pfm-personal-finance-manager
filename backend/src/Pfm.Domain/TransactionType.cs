namespace Pfm.Domain;

/// <summary>
/// Distinguishes money coming in from money going out. A category carries the type it can be
/// used for, and a transaction must match the type of its category.
/// </summary>
public enum TransactionType
{
    Income = 1,
    Expense = 2
}
