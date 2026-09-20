namespace Pfm.Application.Summaries;

/// <summary>Expenses of a single day, plus the running total since the first of the month.</summary>
/// <param name="Date">The calendar day.</param>
/// <param name="Amount">Expenses booked on that day, zero if there were none.</param>
/// <param name="Cumulative">Expenses from the first of the month up to and including that day.</param>
public sealed record DailyExpenseItem(DateOnly Date, decimal Amount, decimal Cumulative);
