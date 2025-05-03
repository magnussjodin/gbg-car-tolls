namespace TollFeeCalculatorTests;

using System.Runtime.CompilerServices;
using TollFeeCalculator;

public class TollCalculatorTests
{
    private const int INVALID_TOLL_FEE = -1;

    [Fact]
    public void GetDailyTollFee_ShouldReturn0ForAnEmptyTimeStampList()
    {
        var tollCalculator = new TollCalculator();
        var vehicle = GetVehicleThatHasFee();
        var timeStamps = new DateTime[]{};
        var fee = INVALID_TOLL_FEE;
        var expectedFee = TollCalculator.MIN_TOLL_FEE;

        try
        {
            fee = tollCalculator.GetDailyTollFee(vehicle, timeStamps);
        }
        catch (System.Exception)
        {
        }

        Assert.Equal(expectedFee, fee);
    }
        
    [Fact]
    public void GetDailyTollFee_ShouldRaiseExceptionIfNotAllTimeStampsFromSameDate()
    {
        var tollCalculator = new TollCalculator();
        var vehicle = GetVehicleThatHasFee();
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
            fee = tollCalculator.GetDailyTollFee(vehicle, timeStamps);
        }
        catch (System.Exception ex)
        {
            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("All time stamps must be from the same date.", ex.Message);
        }

        Assert.Equal(expectedFee, fee);
    }
        
    [Fact]
    public void GetDailyTollFee_ShouldTakeMaxFeeInEachStarted60MinutePeriod()
    {
        var tollCalculator = new TollCalculator();
        var vehicle = GetVehicleThatHasFee();

        // A period with maximum fee for the last time stamp
        var timeStamps = new DateTime[]
        {
            new DateTime(2013, 1, 2, 6, 29, 0), // 8
            new DateTime(2013, 1, 2, 6, 30, 0), // 13
            new DateTime(2013, 1, 2, 7, 0, 0), // 18, the maximum fee in this time period
        };

        int fee = INVALID_TOLL_FEE;
        var expectedFee = 18;
        try
        {
            fee = tollCalculator.GetDailyTollFee(vehicle, timeStamps);
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
            fee = tollCalculator.GetDailyTollFee(vehicle, timeStamps);
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
        var vehicle = GetVehicleThatHasFee();

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
        var expectedFee = 18 + 18 + 8; // 18 for the first period, 18 for the second period and 8 for the third period
        try
        {
            fee = tollCalculator.GetDailyTollFee(vehicle, timeStamps);
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
        var vehicle = GetVehicleThatHasFee();

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
            fee = tollCalculator.GetDailyTollFee(vehicle, timeStamps);
        }
        catch (System.Exception)
        {
        }
        
        Assert.Equal(expectedFee, fee);
    }
        
    [Fact]
    public void GetTimelyTollFee_ShouldReturn0IfVehicleIsNull()
    {
        var tollCalculator = new TollCalculator();
        Vehicle vehicle = null;
        var timeStamp = GetTimeStampThatHasFee();
        var expectedFee = TollCalculator.MIN_TOLL_FEE;

        var fee = tollCalculator.GetTimelyTollFee(timeStamp, vehicle);

        Assert.Equal(expectedFee, fee);
    }
        
    [Fact]
    public void GetTimelyTollFee_ShouldReturn0IfDateHasNoFee()
    {
        var tollCalculator = new TollCalculator();
        var vehicle = GetVehicleThatHasFee();
        var expectedFee = TollCalculator.MIN_TOLL_FEE;

        // Saturdays and Sundays should have no fee
        var aSaturday = Enumerable.Range(1, 7)
            .Select(day => new DateTime(2013, 2, 1).AddDays(day - 1))
            .First(date => date.DayOfWeek == DayOfWeek.Saturday); // First Saturday of February 2013
        Assert.Equal(expectedFee, tollCalculator.GetTimelyTollFee(aSaturday, vehicle));

        var aSunday = aSaturday.AddDays(1); // First Sunday of February 2013
        Assert.Equal(expectedFee, tollCalculator.GetTimelyTollFee(aSunday, vehicle));

        // Some weekdays have no fee
        foreach (var weekDay in GetWeekDaysThatHasNoFee())
        {
            Assert.Equal(expectedFee, tollCalculator.GetTimelyTollFee(weekDay, vehicle));
        }
    }
    
    [Theory]
    [MemberData(nameof(GetVehicleAndHasFeeTestData))]
    public void GetTimelyTollFee_ShouldReturnFeeOnlyIfVehicleTypeHasFee(Vehicle vehicle, bool hasFee)
    {
        var tollCalculator = new TollCalculator();
        var timeStamp = GetTimeStampThatHasFee();

        var fee = tollCalculator.GetTimelyTollFee(timeStamp, vehicle);

        Assert.Equal(hasFee, fee > TollCalculator.MIN_TOLL_FEE);
    }
    
    [Theory]
    [MemberData(nameof(GetTimelyTollFeeTestData))]
    public void GetTimelyTollFee_ShouldReturnFeeAccordingToTimePeriodOfDay(DateTime startTimeStamp, DateTime endTimeStamp, int fee)
    {
        var tollCalculator = new TollCalculator();
        var vehicle = GetVehicleThatHasFee();

        // Check if the fee is valid for all minutes in the time period
        for (var timeStamp = startTimeStamp; timeStamp <= endTimeStamp; timeStamp = timeStamp.AddMinutes(1))
        {
            var actualFee = tollCalculator.GetTimelyTollFee(timeStamp, vehicle);
            Assert.Equal(fee, actualFee);
        }
    }

    private Vehicle? GetVehicleThatHasFee()
    {
        return GetVehicleAndHasFeeTestData()
            .Select(vehicleAndHasFee => new { Vehicle = vehicleAndHasFee[0] as Vehicle, HasFee = (bool)vehicleAndHasFee[1] })
            .First(item => item.HasFee)
            .Vehicle;
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
        yield return new object[] { new Car(), true };
        yield return new object[] { new Motorbike(), false };
    }

    public static IEnumerable<object[]> GetTimelyTollFeeTestData()
    {
        // (startTimeStamp, endTimeStamp, fee):
        // The fee should be valid for all minutes >= startTimeStamp and <= endTimeStamp
        yield return new object[] { new DateTime(2013, 1, 2, 0, 0, 0), new DateTime(2013, 1, 2, 5, 59, 0), TollCalculator.MIN_TOLL_FEE };
        yield return new object[] { new DateTime(2013, 1, 2, 6, 0, 0), new DateTime(2013, 1, 2, 6, 29, 0), 8 };
        yield return new object[] { new DateTime(2013, 1, 2, 6, 30, 0), new DateTime(2013, 1, 2, 6, 59, 0), 13 };
        yield return new object[] { new DateTime(2013, 1, 2, 7, 0, 0), new DateTime(2013, 1, 2, 7, 59, 0), 18 };
        yield return new object[] { new DateTime(2013, 1, 2, 8, 0, 0), new DateTime(2013, 1, 2, 8, 29, 0), 13 };
        yield return new object[] { new DateTime(2013, 1, 2, 8, 30, 0), new DateTime(2013, 1, 2, 14, 59, 0), 8 };
        yield return new object[] { new DateTime(2013, 1, 2, 15, 0, 0), new DateTime(2013, 1, 2, 15, 29, 0), 13 };
        yield return new object[] { new DateTime(2013, 1, 2, 15, 30, 0), new DateTime(2013, 1, 2, 16, 59, 0), 18 };
        yield return new object[] { new DateTime(2013, 1, 2, 17, 0, 0), new DateTime(2013, 1, 2, 17, 59, 0), 13 };
        yield return new object[] { new DateTime(2013, 1, 2, 18, 0, 0), new DateTime(2013, 1, 2, 18, 29, 0), 8 };
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