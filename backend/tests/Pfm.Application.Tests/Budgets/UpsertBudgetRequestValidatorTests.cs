using Pfm.Application.Budgets;

namespace Pfm.Application.Tests.Budgets;

public sealed class UpsertBudgetRequestValidatorTests
{
    private readonly UpsertBudgetRequestValidator _validator = new();

    [Fact]
    public void AcceptsAValidRequest()
    {
        var result = _validator.Validate(Request());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RejectsALimitThatIsNotPositive(decimal limit)
    {
        var result = _validator.Validate(Request(limit: limit));

        Assert.Contains(result.Errors, failure => failure.PropertyName == "Limit");
    }

    [Fact]
    public void RejectsALimitWithMoreThanTwoDecimalPlaces()
    {
        var result = _validator.Validate(Request(limit: 300.001m));

        Assert.Contains(result.Errors, failure => failure.PropertyName == "Limit");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void RejectsAMonthOutsideOneToTwelve(int month)
    {
        var result = _validator.Validate(Request(month: month));

        Assert.Contains(result.Errors, failure => failure.PropertyName == "Month");
    }

    [Fact]
    public void RejectsAMissingCategory()
    {
        var result = _validator.Validate(Request(categoryId: ""));

        Assert.Contains(result.Errors, failure => failure.PropertyName == "CategoryId");
    }

    private static UpsertBudgetRequest Request(
        string categoryId = TestData.ExpenseCategoryId,
        int year = 2025,
        int month = 3,
        decimal limit = 300m) => new(categoryId, year, month, limit);
}
