using FluentValidation;

namespace Pfm.Application.Categories;

/// <summary>
/// Validation rules for the category endpoints. Messages are German because the API returns them
/// to the German UI unchanged.
/// </summary>
public sealed class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(request => request.Name).CategoryName();
        RuleFor(request => request.Type).IsInEnum().WithMessage("Der Typ muss \"Income\" oder \"Expense\" sein.");
        RuleFor(request => request.Icon).CategoryIcon();
        RuleFor(request => request.Color).CategoryColor();
    }
}

/// <inheritdoc cref="CreateCategoryRequestValidator"/>
public sealed class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(request => request.Name).CategoryName();
        RuleFor(request => request.Icon).CategoryIcon();
        RuleFor(request => request.Color).CategoryColor();
    }
}

internal static class CategoryRules
{
    public const int MaxNameLength = 50;

    private const string IconPattern = "^[a-z][a-z0-9_]{0,39}$";
    private const string ColorPattern = "^#[0-9A-Fa-f]{6}$";

    public static IRuleBuilderOptions<T, string> CategoryName<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Der Name darf nicht leer sein.")
            .MaximumLength(MaxNameLength).WithMessage($"Der Name darf höchstens {MaxNameLength} Zeichen lang sein.");

    public static IRuleBuilderOptions<T, string> CategoryIcon<T>(this IRuleBuilder<T, string> rule) =>
        rule.Matches(IconPattern).WithMessage("Das Symbol muss ein Material-Symbol-Name sein, z. B. \"shopping_cart\".");

    public static IRuleBuilderOptions<T, string> CategoryColor<T>(this IRuleBuilder<T, string> rule) =>
        rule.Matches(ColorPattern).WithMessage("Die Farbe muss ein Hex-Wert im Format \"#RRGGBB\" sein.");
}
