using System;
using System.Collections.Generic;
using System.Linq;
using congestion.calculator;

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

    /// <summary>Calculate the total toll fee for one day</summary>
    /// <param name="vehicle">the vehicle</param>
    /// <param name="dates">date and time of all passes on one day</param>
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
            if ((dateTime - intervalStart).Minutes < intervalLengthMinutes)
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

        return intervalBuckets;
    }

    private int GetTollFeeOfInterval(IEnumerable<DateTime> intervalDateTimes, Vehicle vehicle) =>
        intervalDateTimes.Max(time => GetTollFee(time, vehicle));

    private bool IsTollFreeVehicle(Vehicle vehicle)
    {
        if (vehicle == null) return false;
        var vehicleType = vehicle.GetVehicleType();
        return TollFreeVehicles.Contains(vehicleType);
    }

    public int GetTollFee(DateTime date, Vehicle vehicle)
    {
        if (IsTollFreeDate(date) || IsTollFreeVehicle(vehicle)) return 0;

        int hour = date.Hour;
        int minute = date.Minute;

        if (hour == 6 && minute >= 0 && minute <= 29) return 8;
        else if (hour == 6 && minute >= 30 && minute <= 59) return 13;
        else if (hour == 7 && minute >= 0 && minute <= 59) return 18;
        else if (hour == 8 && minute >= 0 && minute <= 29) return 13;
        else if (hour >= 8 && hour <= 14 && minute >= 30 && minute <= 59) return 8;
        else if (hour == 15 && minute >= 0 && minute <= 29) return 13;
        else if (hour == 15 && minute >= 0 || hour == 16 && minute <= 59) return 18;
        else if (hour == 17 && minute >= 0 && minute <= 59) return 13;
        else if (hour == 18 && minute >= 0 && minute <= 29) return 8;
        else return 0;
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