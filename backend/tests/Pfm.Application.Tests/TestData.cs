using Pfm.Domain;
using Pfm.Domain.Entities;

namespace Pfm.Application.Tests;

/// <summary>Entity factories with sensible defaults, so each test only states what it cares about.</summary>
internal static class TestData
{
    public const string ExpenseCategoryId = "aaaaaaaaaaaaaaaaaaaaaaa1";
    public const string IncomeCategoryId = "aaaaaaaaaaaaaaaaaaaaaaa2";

    public static Category Category(
        string id = ExpenseCategoryId,
        string name = "Miete",
        TransactionType type = TransactionType.Expense) => new()
    {
        Id = id,
        Name = name,
        Type = type,
        Icon = type == TransactionType.Income ? "payments" : "home",
        Color = type == TransactionType.Income ? "#2E7D32" : "#1565C0"
    };

    public static Transaction Transaction(
        string id = "bbbbbbbbbbbbbbbbbbbbbbb1",
        TransactionType type = TransactionType.Expense,
        decimal amount = 900m,
        string date = "2025-03-04",
        string categoryId = ExpenseCategoryId,
        string? description = null) => new()
    {
        Id = id,
        Type = type,
        Amount = amount,
        Date = DateOnly.Parse(date),
        CategoryId = categoryId,
        Description = description
    };

    public static Budget Budget(
        string id = "ccccccccccccccccccccccc1",
        string categoryId = ExpenseCategoryId,
        int year = 2025,
        int month = 3,
        decimal limit = 1000m) => new()
    {
        Id = id,
        CategoryId = categoryId,
        Year = year,
        Month = month,
        Limit = limit
    };
}
