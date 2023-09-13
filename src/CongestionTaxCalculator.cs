using System;
using System.Collections.Generic;
using System.Linq;

namespace congestion.calculator;

public class CongestionTaxCalculator
{
    private static readonly VehicleType[] TollFreeVehicles = new []
    {
        VehicleType.Motorcycle,
        VehicleType.Bus,
        VehicleType.Emergency,
        VehicleType.Diplomat,
        VehicleType.Foreign,
        VehicleType.Military
    };

    private TimeOnly _6_00 = new TimeOnly(6, 0);
    private TimeOnly _6_30 = new TimeOnly(6, 30);
    private TimeOnly _7_00 = new TimeOnly(7, 0);
    private TimeOnly _8_00 = new TimeOnly(8, 0);
    private TimeOnly _8_30 = new TimeOnly(8, 30);
    private TimeOnly _15_00 = new TimeOnly(15, 0);
    private TimeOnly _15_30 = new TimeOnly(15, 30);
    private TimeOnly _17_00 = new TimeOnly(17, 0);
    private TimeOnly _18_00 = new TimeOnly(18, 0);
    private TimeOnly _18_30 = new TimeOnly(18, 30);


    /// <summary>Calculate the total toll fee for one day</summary>
    /// <param name="vehicle">the vehicle</param>
    /// <param name="dates">date and time of all passes on one day</param>
    /// <exception cref="ArgumentOutOfRangeException">if all of dates are not from one day</exception>
    /// <returns>the total congestion tax for that day</returns>
    public int GetTax(Vehicle vehicle, DateTime[] dates)
    {
        if (dates == null || dates.Length == 0)
        {
            return 0;
        }
        if (dates.Max().Date != dates.Min().Date)
        {
            throw new ArgumentOutOfRangeException(nameof(dates), "all dates must be on one day");
        }
        var maxFee = 60;
        var singleChargeIntervalInMinutes = 60;

        int totalFee = 0;
        var intervalBuckets = GroupDatesByInterval(dates, singleChargeIntervalInMinutes);
        foreach (var interval in intervalBuckets)
        {
            totalFee += GetTollFeeOfInterval(interval, vehicle);
            if (totalFee >= maxFee) break;
        }
        if (totalFee > maxFee) totalFee = maxFee;
        return totalFee;
    }

    /// <summary>
    /// Group dates by putting all dates that are near to each other within <paramref name="intervalLengthMinutes"/>
    /// starting from the minimum date in the <paramref name="dates"/> list
    /// </summary>
    /// <param name="dates">dates to be grouped</param>
    /// <param name="intervalLengthMinutes">interval that dates are grouped by it</param>
    /// <returns></returns>
    private static List<List<DateTime>> GroupDatesByInterval(IEnumerable<DateTime> dates, int intervalLengthMinutes)
    {
        var sortedDates = dates.ToList();
        sortedDates.Sort();
        var intervalBuckets = new List<List<DateTime>>();
        var bucket = new List<DateTime>();
        DateTime intervalStart = sortedDates.First();
        foreach (var dateTime in sortedDates)
        {
            if ((dateTime - intervalStart).TotalMinutes < intervalLengthMinutes)
            {
                bucket.Add(dateTime);
            }
            else
            {
                intervalBuckets.Add(bucket);
                intervalStart = dateTime;
                bucket = new List<DateTime> { dateTime };
            }
        }
        // add last bucket
        intervalBuckets.Add(bucket);

        return intervalBuckets;
    }

    private int GetTollFeeOfInterval(IEnumerable<DateTime> intervalDateTimes, Vehicle vehicle) =>
        intervalDateTimes.Max(time => GetTollFee(time, vehicle));

    private bool IsTollFreeVehicle(Vehicle vehicle)
    {
        if (vehicle == null) return true;
        var vehicleType = vehicle.GetVehicleType();
        return TollFreeVehicles.Contains(vehicleType);
    }

    public int GetTollFee(DateTime date, Vehicle vehicle)
    {
        if (IsTollFreeDate(date) || IsTollFreeVehicle(vehicle)) return 0;

        var time = TimeOnly.FromTimeSpan(date.TimeOfDay);
        if (time >= _6_00 && time < _6_30) return 8;
        if (time >= _6_30 && time < _7_00) return 13;
        if (time >= _7_00 && time < _8_00) return 18;
        if (time >= _8_00 && time < _8_30) return 13;
        if (time >= _8_30 && time < _15_00) return 8;
        if (time >= _15_00 && time < _15_30) return 13;
        if (time >= _15_30 && time < _17_00) return 18;
        if (time >= _17_00 && time < _18_00) return 13;
        if (time >= _18_00 && time < _18_30) return 8;
        return 0;
    }

    private Boolean IsTollFreeDate(DateTime date)
    {
        int year = date.Year;
        int month = date.Month;
        int day = date.Day;

        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) return true;

        // enhancement : may be later we can store holidays in the database
        if (year == 2013)
        {
            if ((month == 1 && day == 1) ||
                (month == 3 && (day == 28 || day == 29)) ||
                (month == 4 && (day == 1 || day == 30)) ||
                (month == 5 && (day == 1 || day == 8 || day == 9)) ||
                (month == 6 && (day == 5 || day == 6 || day == 21)) ||
                (month == 7) ||
                (month == 11 && day == 1) ||
                (month == 12 && (day == 24 || day == 25 || day == 26 || day == 31)))
            {
                return true;
            }
        }
        return false;
    }
}