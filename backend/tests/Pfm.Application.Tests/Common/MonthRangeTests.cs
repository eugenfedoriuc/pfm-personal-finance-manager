using Pfm.Application.Common;
using Pfm.Domain.Exceptions;

namespace Pfm.Application.Tests.Common;

public sealed class MonthRangeTests
{
    [Fact]
    public void Of_BuildsAHalfOpenRangeOverTheMonth()
    {
        var march = MonthRange.Of(2025, 3);

        Assert.Equal(new DateOnly(2025, 3, 1), march.Start);
        Assert.Equal(new DateOnly(2025, 4, 1), march.EndExclusive);
    }

    [Fact]
    public void Of_RollsOverIntoTheNextYearForDecember()
    {
        var december = MonthRange.Of(2025, 12);

        Assert.Equal(new DateOnly(2025, 12, 1), december.Start);
        Assert.Equal(new DateOnly(2026, 1, 1), december.EndExclusive);
    }

    [Fact]
    public void Of_EndsAfterTheLeapDayInFebruary()
    {
        var february = MonthRange.Of(2024, 2);

        Assert.Equal(new DateOnly(2024, 3, 1), february.EndExclusive);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public void Of_RejectsAMonthOutsideOneToTwelve(int month)
    {
        var exception = Assert.Throws<ValidationException>(() => MonthRange.Of(2025, month));

        Assert.Equal("month", exception.PropertyName);
    }

    [Theory]
    [InlineData(1899)]
    [InlineData(3000)]
    public void Of_RejectsAYearOutsideTheSupportedWindow(int year)
    {
        var exception = Assert.Throws<ValidationException>(() => MonthRange.Of(year, 3));

        Assert.Equal("year", exception.PropertyName);
    }
}
