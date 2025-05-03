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
    public const string SEVERAL_DATES_EXCEPTION_MESSAGE = "All time stamps must be from the same date.";

    private static readonly VehicleType[] TollFreeVehicleTypes =
    [
        VehicleType.Motorbike,
        VehicleType.Tractor,
        VehicleType.Emergency,
        VehicleType.Diplomat,
        VehicleType.Foreign,
        VehicleType.Military
    ];

    /**
     * Calculate the total toll fee for one day
     *
     * @param vehicleType - the vehicle type
     * @param timeStamps - date and time of all passes on one day
     * @return - the total toll fee for that day
     */
    public int GetDailyTollFee(VehicleType vehicleType, DateTime[] timeStampArray)
    {
        // Check if the array is empty
        if (timeStampArray.Length == 0) return MIN_TOLL_FEE;
        
        // Ensure the time stamps are sorted in ascending order
        var timeStamps = timeStampArray.OrderBy(timeStamp => timeStamp); 

        // Check if the timestamps cover more than one date
        if (timeStamps.Any(timeStamp => timeStamp.Date != timeStamps.First().Date))
        {
            throw new ArgumentException(SEVERAL_DATES_EXCEPTION_MESSAGE);
        }

        // Check if the vehicle or the date is toll free
        if (IsVehicleOrDateTollFree(vehicleType, timeStamps.First())) return MIN_TOLL_FEE;

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

    /**
     * Calculate the toll fee for one timestamp
     *
     * @param vehicleType - the vehicle type
     * @param timeStamp - One date and time for a pass
     * @return - the toll fee for that timestamp
     */
    public int GetTimelyTollFee(VehicleType vehicleType, DateTime timeStamp)
    {
        // Check if the vehicle or the date is toll free
        if (IsVehicleOrDateTollFree(vehicleType, timeStamp)) return MIN_TOLL_FEE;

        return GetTimelyTollFee(timeStamp);
    }

    /**
     * Checks if the Vehicle or Date should be toll free
     */
    public bool IsVehicleOrDateTollFree(VehicleType vehicleType, DateTime date) => IsVehicleTollFree(vehicleType) || IsDateTollFree(date);

    private bool IsVehicleTollFree(VehicleType vehicleType) => TollFreeVehicleTypes.Contains(vehicleType);

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

    private Boolean IsDateTollFree(DateTime date)
    {
        int year = date.Year;
        int month = date.Month;
        int day = date.Day;

        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) return true;

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
}