using Pfm.Application.Transactions;
using Pfm.Domain;

namespace Pfm.Application.Tests.Transactions;

public sealed class TransactionRequestValidatorTests
{
    private readonly TransactionRequestValidator _validator = new();

    [Fact]
    public void AcceptsAValidRequest()
    {
        var result = _validator.Validate(Request());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void RejectsAnAmountThatIsNotPositive(decimal amount)
    {
        var result = _validator.Validate(Request(amount: amount));

        Assert.Contains(result.Errors, failure => failure.PropertyName == "Amount");
    }

    [Fact]
    public void RejectsAnAmountWithMoreThanTwoDecimalPlaces()
    {
        var result = _validator.Validate(Request(amount: 12.345m));

        Assert.Contains(result.Errors, failure => failure.PropertyName == "Amount");
    }

    [Fact]
    public void AcceptsAnAmountWithExactlyTwoDecimalPlaces()
    {
        var result = _validator.Validate(Request(amount: 12.34m));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void RejectsATypeOutsideTheEnum()
    {
        var result = _validator.Validate(Request(type: (TransactionType)99));

        Assert.Contains(result.Errors, failure => failure.PropertyName == "Type");
    }

    [Fact]
    public void RejectsAMissingCategory()
    {
        var result = _validator.Validate(Request(categoryId: ""));

        Assert.Contains(result.Errors, failure => failure.PropertyName == "CategoryId");
    }

    [Fact]
    public void RejectsADescriptionLongerThanTheLimit()
    {
        var result = _validator.Validate(
            Request(description: new string('a', TransactionRequestValidator.MaxDescriptionLength + 1)));

        Assert.Contains(result.Errors, failure => failure.PropertyName == "Description");
    }

    [Fact]
    public void AcceptsAMissingDescription()
    {
        var result = _validator.Validate(Request(description: null));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void RejectsADateOutsideTheSupportedWindow()
    {
        var result = _validator.Validate(Request(date: new DateOnly(1899, 12, 31)));

        Assert.Contains(result.Errors, failure => failure.PropertyName == "Date");
    }

    private static TransactionRequest Request(
        TransactionType type = TransactionType.Expense,
        decimal amount = 900m,
        DateOnly? date = null,
        string categoryId = TestData.ExpenseCategoryId,
        string? description = "Märzmiete") =>
        new(type, amount, date ?? new DateOnly(2025, 3, 4), categoryId, description);
}
