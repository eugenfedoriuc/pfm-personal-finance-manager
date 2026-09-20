using FluentValidation;

namespace Pfm.Application.Common;

/// <summary>Rules shared by every monetary field.</summary>
internal static class MoneyRules
{
    /// <summary>Upper bound that keeps a single value from dwarfing any sum built from it.</summary>
    private const decimal Maximum = 1_000_000_000m;

    /// <summary>
    /// Applies the shared money rules. <paramref name="label"/> is the German noun phrase the
    /// messages start with, for example "Der Betrag".
    /// </summary>
    public static IRuleBuilderOptions<T, decimal> Money<T>(this IRuleBuilder<T, decimal> rule, string label) =>
        rule.GreaterThan(0).WithMessage($"{label} muss größer als 0 sein.")
            .LessThan(Maximum).WithMessage($"{label} muss kleiner als {Maximum:N0} sein.")
            .Must(value => decimal.Round(value, 2) == value)
            .WithMessage($"{label} darf höchstens zwei Nachkommastellen haben.");
}
