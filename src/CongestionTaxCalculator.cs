using System;
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
        var sortedDates = dates.ToList();
        sortedDates.Sort();
        if (sortedDates.Last().Date != sortedDates.First().Date)
        {
            throw new ArgumentOutOfRangeException(nameof(dates), "all dates must be on one day");
        }
        
        DateTime intervalStart = dates[0];
        var intervalFee = GetTollFee(intervalStart, vehicle);
        int totalFee = 0;
        foreach (DateTime date in sortedDates)
        {
            int nextFee = GetTollFee(date, vehicle);

            int minutes = (date - intervalStart).Minutes;

            if (minutes <= 60)
            {
                if (totalFee > 0) totalFee -= intervalFee;
                if (nextFee > intervalFee) intervalFee = nextFee;
                totalFee += intervalFee;
            }
            else
            {
                intervalStart = date;
                intervalFee = nextFee;
                totalFee += nextFee;
            }
        }
        if (totalFee > 60) totalFee = 60;
        return totalFee;
    }

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