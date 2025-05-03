namespace TollFeeCalculatorTests;

using System.Runtime.CompilerServices;
using TollFeeCalculator;

public class TollCalculatorTests
{
    [Fact]
    public void GetTimelyTollFee_ShouldReturn0IfVehicleIsNull()
    {
        // Arrange
        var tollCalculator = new TollCalculator();
        Vehicle vehicle = null;
        var timeStamp = GetTimeStampThatHasFee();

        // Act
        var fee = tollCalculator.GetTimelyTollFee(timeStamp, vehicle);

        // Assert
        Assert.Equal(0, fee);
    }
    
    [Theory]
    [MemberData(nameof(GetVehicleAndHasFeeTestData))]
    public void GetTimelyTollFee_ShouldGiveFeeOnlyIfVehicleTypeHasFee(Vehicle vehicle, bool hasFee)
    {
        // Arrange
        var tollCalculator = new TollCalculator();
        var timeStamp = GetTimeStampThatHasFee();

        // Act
        var fee = tollCalculator.GetTimelyTollFee(timeStamp, vehicle);

        // Assert
        Assert.Equal(hasFee, fee > 0);
    }
    
    [Theory]
    [MemberData(nameof(GetTimelyTollFeeTestData))]
    public void GetTimelyTollFee_ShouldGiveFeeAccordingToTimePeriod(DateTime startTimeStamp, DateTime endTimeStamp, int fee)
    {
        // Arrange
        var tollCalculator = new TollCalculator();
        var vehicle = GetVehicleThatHasFee();

        // Act & Assert
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
        return new DateTime(2013, 1, 2, 8, 0, 0);
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
        yield return new object[] { new DateTime(2013, 1, 2, 0, 0, 0), new DateTime(2013, 1, 2, 5, 59, 0), 0 };
        yield return new object[] { new DateTime(2013, 1, 2, 6, 0, 0), new DateTime(2013, 1, 2, 6, 29, 0), 8 };
        yield return new object[] { new DateTime(2013, 1, 2, 6, 30, 0), new DateTime(2013, 1, 2, 6, 59, 0), 13 };
        yield return new object[] { new DateTime(2013, 1, 2, 7, 0, 0), new DateTime(2013, 1, 2, 7, 59, 0), 18 };
        yield return new object[] { new DateTime(2013, 1, 2, 8, 0, 0), new DateTime(2013, 1, 2, 8, 29, 0), 13 };
        yield return new object[] { new DateTime(2013, 1, 2, 8, 30, 0), new DateTime(2013, 1, 2, 14, 59, 0), 8 };
        yield return new object[] { new DateTime(2013, 1, 2, 15, 0, 0), new DateTime(2013, 1, 2, 15, 29, 0), 13 };
        yield return new object[] { new DateTime(2013, 1, 2, 15, 30, 0), new DateTime(2013, 1, 2, 16, 59, 0), 18 };
        yield return new object[] { new DateTime(2013, 1, 2, 17, 0, 0), new DateTime(2013, 1, 2, 17, 59, 0), 13 };
        yield return new object[] { new DateTime(2013, 1, 2, 18, 0, 0), new DateTime(2013, 1, 2, 18, 29, 0), 8 };
        yield return new object[] { new DateTime(2013, 1, 2, 18, 30, 0), new DateTime(2013, 1, 2, 23, 59, 0), 0 };
    }
}