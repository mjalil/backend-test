using congestion.calculator;
using FluentAssertions;

namespace Calculator.Tests;

public class CongestionTaxCalculatorTests
{
    private static readonly Car Car = new Car();

    [Theory]
    [InlineData(new[] { "2013-01-14 21:00:00" }, 0)]
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
}