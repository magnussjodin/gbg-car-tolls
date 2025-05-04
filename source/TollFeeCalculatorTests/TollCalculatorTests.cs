namespace TollFeeCalculatorTests;

using System.Runtime.CompilerServices;
using TollFeeCalculator;

public class TollCalculatorTests
{
    private const int INVALID_TOLL_FEE = -1;

    [Fact]
    public void GetListOfDailyTollFees_ShouldReturnFeesInAnOrderedListOfDates()
    {
        var tollCalculator = new TollCalculator();
        var vehicleType = GetVehicleTypeThatHasFee();
        var timeStamps = new DateTime[]
        {
            new DateTime(2013, 1, 2, 0, 0, 0),
            new DateTime(2013, 1, 3, 0, 0, 0),
            new DateTime(2013, 1, 1, 0, 0, 0),
        };
        var expectedDailyFees = new List<DailyFee>
        {
            new DailyFee { Date = new DateTime(2013, 1, 1), Fee = TollCalculator.MIN_TOLL_FEE },
            new DailyFee { Date = new DateTime(2013, 1, 2), Fee = TollCalculator.MIN_TOLL_FEE },
            new DailyFee { Date = new DateTime(2013, 1, 3), Fee = TollCalculator.MIN_TOLL_FEE },
        };

        var dailyFees = tollCalculator.GetListOfDailyTollFees(vehicleType, timeStamps);
        
        for (var i = 0; i < dailyFees.Count; i++)
        {
            Assert.Equal(dailyFees[i].Date, expectedDailyFees[i].Date);
        }
    }
        
    [Fact]
    public void GetListOfDailyTollFees_ShouldAddFeesFromDifferentDatesToThoseDates()
    {
        var tollCalculator = new TollCalculator();
        var vehicleType = GetVehicleTypeThatHasFee();
        var timeStamps = new DateTime[]
        {
            new DateTime(2013, 1, 3, 6, 30, 0), // 2013-01-03 => 13

            new DateTime(2013, 1, 2, 14, 45, 0), // 2013-01-02 => 8
            new DateTime(2013, 1, 2, 6, 0, 0), // 2013-01-02 => 8

            new DateTime(2013, 1, 3, 8, 0, 0), // 2013-01-03 => 13
            new DateTime(2013, 1, 1, 10, 0, 0), // 2013-01-01 => 0

            new DateTime(2013, 1, 2, 16, 45, 0), // 2013-01-02 => 18
            new DateTime(2013, 1, 2, 0, 0, 0), // 2013-01-02 => 0
        };
        var expectedDailyFees = new List<DailyFee>
        {
            new DailyFee { Date = new DateTime(2013, 1, 1), Fee = TollCalculator.MIN_TOLL_FEE }, // 0
            new DailyFee { Date = new DateTime(2013, 1, 2), Fee = 2 * TollCalculator.LEVEL_1_TOLL_FEE + TollCalculator.LEVEL_3_TOLL_FEE }, // 8 + 8 + 18
            new DailyFee { Date = new DateTime(2013, 1, 3), Fee = 2 * TollCalculator.LEVEL_2_TOLL_FEE }, // 13 + 13
        };

        var dailyFees = tollCalculator.GetListOfDailyTollFees(vehicleType, timeStamps);
        
        for (var i = 0; i < dailyFees.Count; i++)
        {
            Assert.Equal(dailyFees[i].Date, expectedDailyFees[i].Date);
            Assert.Equal(dailyFees[i].Fee, expectedDailyFees[i].Fee);
        }
    }
        
    [Fact]
    public void GetDailyTollFee_ShouldReturn0ForAnEmptyTimeStampArray()
    {
        var tollCalculator = new TollCalculator();
        var vehicleType = GetVehicleTypeThatHasFee();
        var timeStamps = new DateTime[]{};
        var fee = INVALID_TOLL_FEE;
        var expectedFee = TollCalculator.MIN_TOLL_FEE;

        try
        {
            fee = tollCalculator.GetDailyTollFee(vehicleType, timeStamps);
        }
        catch (System.Exception)
        {
        }

        Assert.Equal(expectedFee, fee);
    }
        
    [Fact]
    public void GetDailyTollFee_ShouldThrowExceptionIfNotAllTimeStampsFromSameDate()
    {
        var tollCalculator = new TollCalculator();
        var vehicleType = GetVehicleTypeThatHasFee();
        var timeStamps = new DateTime[]
        {
            new DateTime(2013, 1, 2, 0, 0, 0),
            new DateTime(2013, 1, 2, 16, 45, 0),
            new DateTime(2013, 1, 3, 6, 30, 0) // Different date
        };
        var fee = INVALID_TOLL_FEE;
        var expectedFee = INVALID_TOLL_FEE;

        try
        {
            fee = tollCalculator.GetDailyTollFee(vehicleType, timeStamps);
        }
        catch (System.Exception ex)
        {
            Assert.IsType<ArgumentException>(ex);
            Assert.Equal(TollCalculator.SEVERAL_DATES_EXCEPTION_MESSAGE, ex.Message);
        }

        Assert.Equal(expectedFee, fee);
    }
        
    [Fact]
    public void GetDailyTollFee_ShouldTakeMaxFeeInEachStarted60MinutePeriod()
    {
        var tollCalculator = new TollCalculator();
        var vehicleType = GetVehicleTypeThatHasFee();

        // A period with maximum fee for the last time stamp
        var timeStamps = new DateTime[]
        {
            new DateTime(2013, 1, 2, 6, 29, 0), // 8
            new DateTime(2013, 1, 2, 6, 30, 0), // 13
            new DateTime(2013, 1, 2, 7, 0, 0), // 18, the maximum fee in this time period
        };

        int fee = INVALID_TOLL_FEE;
        var expectedFee = TollCalculator.LEVEL_3_TOLL_FEE; // 18 for 1st period
        try
        {
            fee = tollCalculator.GetDailyTollFee(vehicleType, timeStamps);
        }
        catch (System.Exception)
        {
        }
        Assert.Equal(expectedFee, fee);

        // A period with maximum fee for the last time stamp
        timeStamps = new DateTime[]
        {
            new DateTime(2013, 1, 2, 7, 59, 0), // 18, the maximum fee in this time period
            new DateTime(2013, 1, 2, 8, 0, 0), // 13
            new DateTime(2013, 1, 2, 8, 30, 0), // 8
        };

        fee = INVALID_TOLL_FEE;
        try
        {
            fee = tollCalculator.GetDailyTollFee(vehicleType, timeStamps);
        }
        catch (System.Exception)
        {
        }
        Assert.Equal(expectedFee, fee);
    }
        
    [Fact]
    public void GetDailyTollFee_ShouldReturnAllMaxFeesAddedUp()
    {
        var tollCalculator = new TollCalculator();
        var vehicleType = GetVehicleTypeThatHasFee();

        // A period with maximum fee for the last time stamp
        var timeStamps = new DateTime[]
        {
            new DateTime(2013, 1, 2, 6, 29, 0), // 8
            new DateTime(2013, 1, 2, 6, 30, 0), // 13
            new DateTime(2013, 1, 2, 7, 0, 0), // 18, the maximum fee in 1st time period

            new DateTime(2013, 1, 2, 7, 59, 0), // 18, the maximum fee in 2nd time period
            new DateTime(2013, 1, 2, 8, 0, 0), // 13
            new DateTime(2013, 1, 2, 8, 30, 0), // 8

            new DateTime(2013, 1, 2, 14, 44, 0), // 8, the maximum fee in 3rd time period
        };

        int fee = INVALID_TOLL_FEE;
        // 18 for 1st and 2nd period, 8 for 3rd period
        var expectedFee = TollCalculator.LEVEL_3_TOLL_FEE + TollCalculator.LEVEL_3_TOLL_FEE + TollCalculator.LEVEL_1_TOLL_FEE; 
        try
        {
            fee = tollCalculator.GetDailyTollFee(vehicleType, timeStamps);
        }
        catch (System.Exception)
        {
        }

        Assert.Equal(expectedFee, fee);
    }
        
    [Fact]
    public void GetDailyTollFee_ShouldReturn60AsDailyMaxFee()
    {
        var tollCalculator = new TollCalculator();
        var vehicleType = GetVehicleTypeThatHasFee();

        // A period with maximum fee for the last time stamp
        var timeStamps = new DateTime[]
        {
            new DateTime(2013, 1, 2, 6, 29, 0), // 8
            new DateTime(2013, 1, 2, 7, 0, 0), // 18, the maximum fee in 1st time period

            new DateTime(2013, 1, 2, 7, 59, 0), // 18, the maximum fee in 2nd time period
            new DateTime(2013, 1, 2, 8, 30, 0), // 8

            new DateTime(2013, 1, 2, 14, 44, 0), // 8, the maximum fee in 3rd time period

            new DateTime(2013, 1, 2, 16, 59, 0), // 18, the maximum fee in 4th time period
        };

        int fee = INVALID_TOLL_FEE;
        var expectedFee = TollCalculator.MAX_DAILY_TOLL_FEE; // The sum of all max fees exceeds 60
        try
        {
            fee = tollCalculator.GetDailyTollFee(vehicleType, timeStamps);
        }
        catch (System.Exception)
        {
        }
        
        Assert.Equal(expectedFee, fee);
    }
        
    [Fact]
    public void GetDailyTollFee_ShouldHandleUnorderedTimeStampsCorrectly()
    {
        var tollCalculator = new TollCalculator();
        var vehicleType = GetVehicleTypeThatHasFee();

        // A period with maximum fee for the last time stamp
        var timeStamps = new DateTime[]
        {
            new DateTime(2013, 1, 2, 7, 0, 0), // 18, the maximum fee in 1st time period
            new DateTime(2013, 1, 2, 7, 59, 0), // 18, the maximum fee in 2nd time period
            new DateTime(2013, 1, 2, 6, 29, 0), // 8, 1st period
            new DateTime(2013, 1, 2, 14, 44, 0), // 8, the maximum fee in 3rd time period
            new DateTime(2013, 1, 2, 8, 30, 0), // 8, 2nd period
            new DateTime(2013, 1, 2, 8, 0, 0), // 13, 2nd period
            new DateTime(2013, 1, 2, 6, 30, 0), // 13, 1st period
        };

        int fee = INVALID_TOLL_FEE;
        // 18 for 1st and 2nd period, 8 for 3rd period
        var expectedFee = TollCalculator.LEVEL_3_TOLL_FEE + TollCalculator.LEVEL_3_TOLL_FEE + TollCalculator.LEVEL_1_TOLL_FEE; 
        try
        {
            fee = tollCalculator.GetDailyTollFee(vehicleType, timeStamps);
        }
        catch (System.Exception)
        {
        }
        
        Assert.Equal(expectedFee, fee);
    }
        
    [Fact]
    public void IsVehicleOrDateTollFree_ShouldBeFalseIfDateHasNoFee()
    {
        var tollCalculator = new TollCalculator();
        var vehicleType = GetVehicleTypeThatHasFee();
        bool? expectedAnswer = true, answer = null;

        // Saturdays and Sundays should have no fee
        var aSaturday = Enumerable.Range(1, 7)
            .Select(day => new DateTime(2013, 2, 1).AddDays(day - 1))
            .First(date => date.DayOfWeek == DayOfWeek.Saturday); // First Saturday of February 2013
        try
        {
            answer = tollCalculator.IsVehicleOrDateTollFree(vehicleType, aSaturday);    
        }
        catch (System.Exception)
        {
        }
        Assert.Equal(expectedAnswer, answer);

        var aSunday = aSaturday.AddDays(1); // First Sunday of February 2013
        try
        {
            answer = tollCalculator.IsVehicleOrDateTollFree(vehicleType, aSunday);    
        }
        catch (System.Exception)
        {
        }
        Assert.Equal(expectedAnswer, answer);

        // Some weekdays have no fee
        foreach (var weekDay in GetWeekDaysThatHasNoFee())
        {
            try
            {
                answer = tollCalculator.IsVehicleOrDateTollFree(vehicleType, weekDay);    
            }
            catch (System.Exception)
            {
            }
            Assert.Equal(expectedAnswer, answer);
        }
    }
    
    [Theory]
    [MemberData(nameof(GetVehicleAndHasFeeTestData))]
    public void GetTimelyTollFee_ShouldReturnFeeOnlyIfVehicleTypeHasFee(Vehicle vehicle, bool hasFee)
    {
        var tollCalculator = new TollCalculator();
        var timeStamp = GetTimeStampThatHasFee();

        var fee = tollCalculator.GetTimelyTollFee(vehicle.VehicleType, timeStamp);

        Assert.Equal(hasFee, fee > TollCalculator.MIN_TOLL_FEE);
    }
    
    [Theory]
    [MemberData(nameof(GetTimelyTollFeeTestData))]
    public void GetTimelyTollFee_ShouldReturnFeeAccordingToTimePeriodOfDay(DateTime startTimeStamp, DateTime endTimeStamp, int fee)
    {
        var tollCalculator = new TollCalculator();
        var vehicleType = GetVehicleTypeThatHasFee();

        // Check if the fee is valid for all minutes in the time period
        for (var timeStamp = startTimeStamp; timeStamp <= endTimeStamp; timeStamp = timeStamp.AddMinutes(1))
        {
            var actualFee = tollCalculator.GetTimelyTollFee(vehicleType, timeStamp);
            Assert.Equal(fee, actualFee);
        }
    }

    private VehicleType GetVehicleTypeThatHasFee()
    {
        return GetVehicleAndHasFeeTestData()
            .Select(item => new { Vehicle = (Vehicle)item[0], HasFee = (bool)item[1] })
            .First(item => item.HasFee)
            .Vehicle
            .VehicleType;
    }

    private DateTime GetTimeStampThatHasFee()
    {
        return GetTimelyTollFeeTestData()
            .Select(timeData => new { StartTimeDate = (DateTime)timeData[0], Fee = (int)timeData[2] })
            .First(item => item.Fee > TollCalculator.MIN_TOLL_FEE)
            .StartTimeDate;
    }

    public static IEnumerable<object[]> GetVehicleAndHasFeeTestData()
    {
        yield return new object[] { new Car() { LicenseNumber = string.Empty }, true };
        yield return new object[] { new Motorbike() { LicenseNumber = string.Empty }, false };
    }

    public static IEnumerable<object[]> GetTimelyTollFeeTestData()
    {
        // (startTimeStamp, endTimeStamp, fee):
        // The fee should be valid for all minutes >= startTimeStamp and <= endTimeStamp
        yield return new object[] { new DateTime(2013, 1, 2, 0, 0, 0), new DateTime(2013, 1, 2, 5, 59, 0), TollCalculator.MIN_TOLL_FEE };
        yield return new object[] { new DateTime(2013, 1, 2, 6, 0, 0), new DateTime(2013, 1, 2, 6, 29, 0), TollCalculator.LEVEL_1_TOLL_FEE };
        yield return new object[] { new DateTime(2013, 1, 2, 6, 30, 0), new DateTime(2013, 1, 2, 6, 59, 0), TollCalculator.LEVEL_2_TOLL_FEE };
        yield return new object[] { new DateTime(2013, 1, 2, 7, 0, 0), new DateTime(2013, 1, 2, 7, 59, 0), TollCalculator.LEVEL_3_TOLL_FEE };
        yield return new object[] { new DateTime(2013, 1, 2, 8, 0, 0), new DateTime(2013, 1, 2, 8, 29, 0), TollCalculator.LEVEL_2_TOLL_FEE };
        yield return new object[] { new DateTime(2013, 1, 2, 8, 30, 0), new DateTime(2013, 1, 2, 14, 59, 0), TollCalculator.LEVEL_1_TOLL_FEE };
        yield return new object[] { new DateTime(2013, 1, 2, 15, 0, 0), new DateTime(2013, 1, 2, 15, 29, 0), TollCalculator.LEVEL_2_TOLL_FEE };
        yield return new object[] { new DateTime(2013, 1, 2, 15, 30, 0), new DateTime(2013, 1, 2, 16, 59, 0), TollCalculator.LEVEL_3_TOLL_FEE };
        yield return new object[] { new DateTime(2013, 1, 2, 17, 0, 0), new DateTime(2013, 1, 2, 17, 59, 0), TollCalculator.LEVEL_2_TOLL_FEE };
        yield return new object[] { new DateTime(2013, 1, 2, 18, 0, 0), new DateTime(2013, 1, 2, 18, 29, 0), TollCalculator.LEVEL_1_TOLL_FEE };
        yield return new object[] { new DateTime(2013, 1, 2, 18, 30, 0), new DateTime(2013, 1, 2, 23, 59, 0), TollCalculator.MIN_TOLL_FEE };
    }

    public static IEnumerable<DateTime> GetWeekDaysThatHasNoFee()
    {
        yield return new DateTime(2013, 1, 1, 0, 0, 0);
        yield return new DateTime(2013, 3, 28, 0, 0, 0);
        yield return new DateTime(2013, 3, 29, 0, 0, 0);
        yield return new DateTime(2013, 4, 1, 0, 0, 0);
        yield return new DateTime(2013, 4, 30, 0, 0, 0);
        yield return new DateTime(2013, 5, 1, 0, 0, 0);
        yield return new DateTime(2013, 5, 8, 0, 0, 0);
        yield return new DateTime(2013, 5, 9, 0, 0, 0);
        yield return new DateTime(2013, 6, 5, 0, 0, 0);
        yield return new DateTime(2013, 6, 6, 0, 0, 0);
        yield return new DateTime(2013, 6, 21, 0, 0, 0);
        yield return new DateTime(2013, 11, 1, 0, 0, 0);
        yield return new DateTime(2013, 12, 24, 0, 0, 0);
        yield return new DateTime(2013, 12, 25, 0, 0, 0);
        yield return new DateTime(2013, 12, 26, 0, 0, 0);
        yield return new DateTime(2013, 12, 31, 0, 0, 0);
        // Every week day in July
        foreach (var date in Enumerable.Range(1, 31)
            .Select(day => new DateTime(2013, 7, 1).AddDays(day - 1))
            .Where(date => date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday))
        {
            yield return date;
        }
    }
}