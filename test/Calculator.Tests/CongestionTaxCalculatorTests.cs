using System.Globalization;
using congestion.calculator;
using FluentAssertions;

namespace Calculator.Tests;

public class CongestionTaxCalculatorTests
{
    private static readonly Car Car = new Car();

    [Theory]
    [InlineData(new[] { "2013-01-14 21:00:00" }, 0)]
    [InlineData(new[] { "2013-02-07 06:33:27" }, 13)]
    [InlineData(new[] { "2013-02-07 07:33:27" }, 18)]
    [InlineData(new[] { "2013-02-07 08:23:27" }, 13)]
    [InlineData(new[] { "2013-02-07 06:23:27", "2013-02-07 15:27:00" }, 21)]
    [InlineData(
        new[]
        {
            "2013-02-08 06:27:00", "2013-02-08 06:20:27", "2013-02-08 14:35:00", "2013-02-08 15:29:00",
            "2013-02-08 15:47:00", "2013-02-08 16:01:00", "2013-02-08 16:48:00", "2013-02-08 17:49:00",
            "2013-02-08 18:29:00", "2013-02-08 18:35:00"
        }, 60)]
    [InlineData(new[] { "2013-03-26 14:25:00" }, 8)]
    [InlineData(new[] { "2013-03-28 14:07:27" }, 0)]
    public void should_calculate_tax_as_expected(string[] dateStrings, int expectedTax)
    {
        var dates = dateStrings.Select(DateTime.Parse).ToArray();
        var calculator = new CongestionTaxCalculator();
        var calculatedTax = calculator.GetTax(Car, dates);

        calculatedTax.Should().Be(expectedTax);
    }

    [Fact]
    public void should_return_0_for_weekends()
    {
        var calculator = new CongestionTaxCalculator();
        var date = new DateTime(2013, 2, 5, 8, 45, 0);
        var dayOfWeek = new GregorianCalendar().GetDayOfWeek(date);
        var dateDiff = DayOfWeek.Saturday - dayOfWeek;
        var saturday = date.AddDays(dateDiff);
        var sunday = date.AddDays(dateDiff + 1);
        var calculatedTax = calculator.GetTax(Car, new[] { saturday });

        calculatedTax.Should().Be(0);
        calculatedTax = calculator.GetTax(Car, new[] { sunday });
        calculatedTax.Should().Be(0);
    }

    [Fact]
    public void should_calculate_0_for_motorBike()
    {
        var calculator = new CongestionTaxCalculator();
        var tax = calculator.GetTax(new Motorbike(), new[] { DateTime.Parse("2013-02-08 8:35") });
        tax.Should().Be(0);
    }

    [Fact]
    public void should_calculate_0_for_empty_or_null_dates()
    {
        var calculator = new CongestionTaxCalculator();
        var tax = calculator.GetTax(new Car(), Array.Empty<DateTime>());
        tax.Should().Be(0);

        tax = calculator.GetTax(new Car(), null);
        tax.Should().Be(0);
    }

    [Fact]
    public void should_throw_exception_when_dates_are_for_more_than_one_day()
    {
        var calculator = new CongestionTaxCalculator();
        var act = () => calculator.GetTax(new Car(),
            new[] { DateTime.Parse("2013-02-08 8:35"), DateTime.Parse("2013-02-09 8:35") });
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void should_calculate_0_for_null_vehicle()
    {
        var calculator = new CongestionTaxCalculator();
        var tax = calculator.GetTax(null, new []{DateTime.Parse("2013-02-08 8:35")});
        tax.Should().Be(0);

    }
}