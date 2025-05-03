using System;
using System.Globalization;
using TollFeeCalculator;

public class TollCalculator
{

    /**
     * Calculate the total toll fee for one day
     *
     * @param vehicle - the vehicle
     * @param timeStamps - date and time of all passes on one day
     * @return - the total toll fee for that day
     */

    public int GetDailyTollFee(Vehicle vehicle, DateTime[] timeStamps)
    {
        if (timeStamps.Length == 0) return 0;
        
        if (timeStamps.Any(timeStamp => timeStamp.Date != timeStamps[0].Date))
        {
            throw new ArgumentException("All time stamps must be from the same date.");
        }

        var intervalStartTimeStamp = timeStamps.First().AddMinutes(-60); // Start value guaranteed to be before the first time stamp
        var intervalMaxFee = 0;
        var totalFee = 0;
        foreach (DateTime timeStamp in timeStamps)
        {
            int fee = GetTimelyTollFee(timeStamp, vehicle);

            // TimeStamp is outside the 60 minute interval.
            // Add the max fee of the last interval to the total fee and start a new interval
            if ((timeStamp - intervalStartTimeStamp).TotalMinutes >= 60)
            {
                totalFee += intervalMaxFee;
                totalFee = Math.Min(totalFee, 60); // Cap the total fee to 60

                // Max fee is reached, no need to calculate further
                if (totalFee == 60)
                {
                    return totalFee;
                }

                intervalStartTimeStamp = timeStamp;
                intervalMaxFee = fee;
            }
            // TimeStamp is inside the 60 minute interval.
            // Calculate the max fee
            else            
            {
                intervalMaxFee = Math.Max(intervalMaxFee, fee);
            }
        }

        totalFee += intervalMaxFee;
        totalFee = Math.Min(totalFee, 60); // Cap the total fee to 60

        return totalFee;
    }

    private bool IsTollFreeVehicle(Vehicle vehicle)
    {
        if (vehicle == null) return true;
        String vehicleType = vehicle.GetVehicleType();
        return vehicleType.Equals(TollFreeVehicles.Motorbike.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Tractor.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Emergency.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Diplomat.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Foreign.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Military.ToString());
    }

    public int GetTimelyTollFee(DateTime timeStamp, Vehicle vehicle)
    {
        if (IsTollFreeDate(timeStamp) || IsTollFreeVehicle(vehicle)) return 0;

        int hour = timeStamp.Hour;
        int minute = timeStamp.Minute;

        if (hour == 6 && minute <= 29) return 8;
        else if (hour == 6) return 13;
        else if (hour == 7) return 18;
        else if (hour == 8 && minute <= 29) return 13;
        else if (hour >= 8 && hour <= 14) return 8;
        else if (hour == 15 && minute <= 29) return 13;
        else if (hour >= 15 && hour <= 16) return 18;
        else if (hour == 17) return 13;
        else if (hour == 18 && minute <= 29) return 8;
        else return 0;
    }

    private Boolean IsTollFreeDate(DateTime timeStamp)
    {
        int year = timeStamp.Year;
        int month = timeStamp.Month;
        int day = timeStamp.Day;

        if (timeStamp.DayOfWeek == DayOfWeek.Saturday || timeStamp.DayOfWeek == DayOfWeek.Sunday) return true;

        if (year == 2013)
        {
            if (month == 1 && day == 1 ||
                month == 3 && (day == 28 || day == 29) ||
                month == 4 && (day == 1 || day == 30) ||
                month == 5 && (day == 1 || day == 8 || day == 9) ||
                month == 6 && (day == 5 || day == 6 || day == 21) ||
                month == 7 ||
                month == 11 && day == 1 ||
                month == 12 && (day == 24 || day == 25 || day == 26 || day == 31))
            {
                return true;
            }
        }
        return false;
    }

    private enum TollFreeVehicles
    {
        Motorbike = 0,
        Tractor = 1,
        Emergency = 2,
        Diplomat = 3,
        Foreign = 4,
        Military = 5
    }
}