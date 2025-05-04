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

    // This class is used to compare vehicles based on their license number and type.
    // It implements the IEqualityComparer interface to provide custom equality comparison 
    // logic in e g the Dictionary.CompareKey() method.

    // Reason for that the License number is not made unique for Vehicle type, 
    // is that there is no other way to detect that same license nr has been used for different vehicle types.
    // Otherwise, first vehicle type will be registered and the second will be ignored, in a dictionary.
    
    public class VehicleEqualityComparer : IEqualityComparer<Vehicle>
    {
        public bool Equals(Vehicle x, Vehicle y)
        {
            if (x == null || y == null) return false;
            return x.LicenseNumber == y.LicenseNumber && x.VehicleType == y.VehicleType;
        }

        public int GetHashCode(Vehicle obj)
        {
            return HashCode.Combine(obj.LicenseNumber, obj.VehicleType);
        }
    }
}