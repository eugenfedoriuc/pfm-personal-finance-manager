using Pfm.Domain.Exceptions;

namespace Pfm.Application.Common;

/// <summary>
/// One calendar month, expressed as the half-open day range <c>[Start, EndExclusive)</c>. Every
/// month-scoped query in the app goes through this type so the boundaries are defined exactly once.
/// </summary>
public sealed record MonthRange
{
    public const int MinYear = 1900;
    public const int MaxYear = 2999;

    private MonthRange(int year, int month)
    {
        Year = year;
        Month = month;
        Start = new DateOnly(year, month, 1);
        EndExclusive = Start.AddMonths(1);
    }

    public int Year { get; }

    /// <summary>1 to 12.</summary>
    public int Month { get; }

    /// <summary>First day of the month, inclusive.</summary>
    public DateOnly Start { get; }

    /// <summary>First day of the following month, exclusive.</summary>
    public DateOnly EndExclusive { get; }

    /// <exception cref="ValidationException">The year or the month is out of range.</exception>
    public static MonthRange Of(int year, int month)
    {
        if (year is < MinYear or > MaxYear)
        {
            throw new ValidationException("year", $"Das Jahr muss zwischen {MinYear} und {MaxYear} liegen.");
        }

        if (month is < 1 or > 12)
        {
            throw new ValidationException("month", "Der Monat muss zwischen 1 und 12 liegen.");
        }

        return new MonthRange(year, month);
    }
}
