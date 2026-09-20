using FluentValidation;
using Pfm.Application.Common;

namespace Pfm.Application.Transactions;

public sealed class TransactionRequestValidator : AbstractValidator<TransactionRequest>
{
    public const int MaxDescriptionLength = 200;

    public TransactionRequestValidator()
    {
        RuleFor(request => request.Type)
            .IsInEnum().WithMessage("Der Typ muss \"Income\" oder \"Expense\" sein.");

        RuleFor(request => request.Amount).Money("Der Betrag");

        RuleFor(request => request.Date)
            .Must(date => date.Year is >= MonthRange.MinYear and <= MonthRange.MaxYear)
            .WithMessage($"Das Datum muss zwischen {MonthRange.MinYear} und {MonthRange.MaxYear} liegen.");

        RuleFor(request => request.CategoryId)
            .NotEmpty().WithMessage("Es muss eine Kategorie ausgewählt werden.");

        RuleFor(request => request.Description)
            .MaximumLength(MaxDescriptionLength)
            .WithMessage($"Die Notiz darf höchstens {MaxDescriptionLength} Zeichen lang sein.");
    }
}
