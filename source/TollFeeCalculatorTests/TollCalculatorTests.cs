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

    private DateTime GetTimeStampThatHasFee()
    {
        return new DateTime(2013, 1, 2, 8, 0, 0);
    }

    public static IEnumerable<object[]> GetVehicleAndHasFeeTestData()
    {
        yield return new object[] { new Car(), true };
        yield return new object[] { new Motorbike(), false };
    }
}