using FluentValidation;
using Pfm.Application.Common;

namespace Pfm.Application.Budgets;

public sealed class UpsertBudgetRequestValidator : AbstractValidator<UpsertBudgetRequest>
{
    public UpsertBudgetRequestValidator()
    {
        RuleFor(request => request.CategoryId)
            .NotEmpty().WithMessage("Es muss eine Kategorie ausgewählt werden.");

        RuleFor(request => request.Year)
            .InclusiveBetween(MonthRange.MinYear, MonthRange.MaxYear)
            .WithMessage($"Das Jahr muss zwischen {MonthRange.MinYear} und {MonthRange.MaxYear} liegen.");

        RuleFor(request => request.Month)
            .InclusiveBetween(1, 12).WithMessage("Der Monat muss zwischen 1 und 12 liegen.");

        RuleFor(request => request.Limit).Money("Das Limit");
    }
}
