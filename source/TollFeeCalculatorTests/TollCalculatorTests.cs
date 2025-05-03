namespace TollFeeCalculatorTests;

using TollFeeCalculator;

public class TollCalculatorTests
{
    [Fact]
    public void GetTimelyTollFee_ShouldReturn0IfVehicleIsNull()
    {
        // Arrange
        var tollCalculator = new TollCalculator();
        Vehicle vehicle = null;
        var timeStamp = new DateTime(2013, 5, 2, 8, 0, 0);

        // Act
        var toll = tollCalculator.GetTimelyTollFee(timeStamp, vehicle);

        // Assert
        Assert.Equal(0, toll);
    }
}