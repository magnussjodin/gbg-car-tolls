using System;

namespace TollFeeCalculator
{
    public enum VehicleType
    {
        Car = 0,
        Motorbike = 1,
        Tractor = 2,
        Emergency = 3,
        Diplomat = 4,
        Foreign = 5,
        Military = 6,
    }

    public interface Vehicle
    {
        VehicleType VehicleType { get; }
        string LicenseNumber { get; set; }
    }
}