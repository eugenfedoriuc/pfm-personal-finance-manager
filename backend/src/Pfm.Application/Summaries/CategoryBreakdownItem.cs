namespace Pfm.Application.Summaries;

/// <summary>What one category contributed to the income or expenses of a month.</summary>
/// <param name="CategoryId">Id of the category.</param>
/// <param name="Name">Name of the category.</param>
/// <param name="Icon">Material Symbol of the category.</param>
/// <param name="Color">Accent colour of the category.</param>
/// <param name="Amount">Sum of the category's transactions in the month.</param>
/// <param name="TransactionCount">How many transactions make up <paramref name="Amount"/>.</param>
/// <param name="Percentage">Share of the month's total, rounded to two decimals.</param>
public sealed record CategoryBreakdownItem(
    string CategoryId,
    string Name,
    string Icon,
    string Color,
    decimal Amount,
    int TransactionCount,
    decimal Percentage);
