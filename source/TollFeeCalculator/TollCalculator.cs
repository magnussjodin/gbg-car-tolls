using System;
using System.Globalization;
using TollFeeCalculator;

public class TollCalculator
{
    /**
     * Constants used in toll fee calculation
     */
    public const int MIN_TOLL_FEE = 0;
    public const int LEVEL_1_TOLL_FEE = 8;
    public const int LEVEL_2_TOLL_FEE = 13;
    public const int LEVEL_3_TOLL_FEE = 18;
    public const int MAX_DAILY_TOLL_FEE = 60;
    public const int MAX_INTERVAL_MINUTES = 59;
    

    /**
     * Calculate the total toll fee for one day
     *
     * @param vehicle - the vehicle
     * @param timeStamps - date and time of all passes on one day
     * @return - the total toll fee for that day
     */
    public int GetDailyTollFee(Vehicle vehicle, DateTime[] timeStampArray)
    {
        // Check if the array is empty
        if (timeStampArray.Length == 0) return MIN_TOLL_FEE;
        
        // Ensure the time stamps are sorted in ascending order
        var timeStamps = timeStampArray.OrderBy(timeStamp => timeStamp); 

        // Check if the timestamps cover more than one date
        if (timeStamps.Any(timeStamp => timeStamp.Date != timeStamps.First().Date))
        {
            throw new ArgumentException("All time stamps must be from the same date.");
        }

        // Check if the vehicle or the date is toll free
        if (IsVehicleOrDateTollFree(vehicle, timeStamps.First())) return MIN_TOLL_FEE;

        // ---- Calculate the total toll fee ----
        var intervalStartTimeStamp = timeStamps.First().AddMinutes(-MAX_INTERVAL_MINUTES-1); // Start value guaranteed to be before the first time stamp
        var intervalMaxFee = MIN_TOLL_FEE;
        var totalFee = MIN_TOLL_FEE;
        foreach (DateTime timeStamp in timeStamps)
        {
            int fee = GetTimelyTollFee(timeStamp);

            // TimeStamp is outside the 60 minute interval.
            // Add the max fee of the last interval to the total fee and start a new interval
            if ((timeStamp - intervalStartTimeStamp).TotalMinutes > MAX_INTERVAL_MINUTES)
            {
                totalFee += intervalMaxFee;
                totalFee = Math.Min(totalFee, MAX_DAILY_TOLL_FEE); // Cap the total fee to 60

                // Max fee is reached, no need to calculate further
                if (totalFee == MAX_DAILY_TOLL_FEE)
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
        totalFee = Math.Min(totalFee, MAX_DAILY_TOLL_FEE); // Cap the total fee to 60

        return totalFee;
    }

    private bool IsVehicleOrDateTollFree(Vehicle vehicle, DateTime timeStamp) => IsVehicleTollFree(vehicle) || IsDateTollFree(timeStamp);

    private bool IsVehicleTollFree(Vehicle vehicle)
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
        // Check if the vehicle or the date is toll free
        if (IsVehicleOrDateTollFree(vehicle, timeStamp)) return MIN_TOLL_FEE;

        return GetTimelyTollFee(timeStamp);
    }

    private int GetTimelyTollFee(DateTime timeStamp)
    {
        int hour = timeStamp.Hour;
        int minute = timeStamp.Minute;

        if (hour == 6 && minute <= 29) return LEVEL_1_TOLL_FEE;
        else if (hour == 6) return LEVEL_2_TOLL_FEE;
        else if (hour == 7) return LEVEL_3_TOLL_FEE;
        else if (hour == 8 && minute <= 29) return LEVEL_2_TOLL_FEE;
        else if (hour >= 8 && hour <= 14) return LEVEL_1_TOLL_FEE;
        else if (hour == 15 && minute <= 29) return LEVEL_2_TOLL_FEE;
        else if (hour >= 15 && hour <= 16) return LEVEL_3_TOLL_FEE;
        else if (hour == 17) return LEVEL_2_TOLL_FEE;
        else if (hour == 18 && minute <= 29) return LEVEL_1_TOLL_FEE;
        else return MIN_TOLL_FEE;
    }

    private Boolean IsDateTollFree(DateTime timeStamp)
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