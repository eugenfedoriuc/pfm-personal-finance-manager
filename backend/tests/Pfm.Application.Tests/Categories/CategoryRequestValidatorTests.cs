using Pfm.Application.Categories;
using Pfm.Domain;

namespace Pfm.Application.Tests.Categories;

public sealed class CategoryRequestValidatorTests
{
    private readonly CreateCategoryRequestValidator _createValidator = new();
    private readonly UpdateCategoryRequestValidator _updateValidator = new();

    [Fact]
    public void Create_AcceptsAValidRequest()
    {
        var result = _createValidator.Validate(
            new CreateCategoryRequest("Lebensmittel", TransactionType.Expense, "shopping_cart", "#ef6c00"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_RejectsABlankName(string name)
    {
        var result = _createValidator.Validate(
            new CreateCategoryRequest(name, TransactionType.Expense, "shopping_cart", "#EF6C00"));

        Assert.Contains(result.Errors, failure => failure.PropertyName == "Name");
    }

    [Fact]
    public void Create_RejectsANameLongerThanFiftyCharacters()
    {
        var result = _createValidator.Validate(
            new CreateCategoryRequest(new string('a', 51), TransactionType.Expense, "shopping_cart", "#EF6C00"));

        Assert.Contains(result.Errors, failure => failure.PropertyName == "Name");
    }

    [Fact]
    public void Create_RejectsATypeOutsideTheEnum()
    {
        var result = _createValidator.Validate(
            new CreateCategoryRequest("Lebensmittel", (TransactionType)99, "shopping_cart", "#EF6C00"));

        Assert.Contains(result.Errors, failure => failure.PropertyName == "Type");
    }

    [Theory]
    [InlineData("Shopping_Cart")]
    [InlineData("shopping cart")]
    [InlineData("")]
    public void Create_RejectsSomethingThatIsNotAMaterialSymbolName(string icon)
    {
        var result = _createValidator.Validate(
            new CreateCategoryRequest("Lebensmittel", TransactionType.Expense, icon, "#EF6C00"));

        Assert.Contains(result.Errors, failure => failure.PropertyName == "Icon");
    }

    [Theory]
    [InlineData("EF6C00")]
    [InlineData("#EF6C0")]
    [InlineData("#EF6C000")]
    [InlineData("rot")]
    public void Create_RejectsAColourThatIsNotASixDigitHexTriplet(string color)
    {
        var result = _createValidator.Validate(
            new CreateCategoryRequest("Lebensmittel", TransactionType.Expense, "shopping_cart", color));

        Assert.Contains(result.Errors, failure => failure.PropertyName == "Color");
    }

    [Fact]
    public void Update_AppliesTheSameNameIconAndColourRules()
    {
        var result = _updateValidator.Validate(new UpdateCategoryRequest("", "Shopping", "rot"));

        Assert.Equal(
            ["Name", "Icon", "Color"],
            result.Errors.Select(failure => failure.PropertyName).Distinct());
    }
}
