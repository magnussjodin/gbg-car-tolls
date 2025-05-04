
namespace TollFeeCalculator
{
    /***
     * Class to handle toll fee calculation for vehicles passing through a toll booth
     * It is stateful, keeping track of every passage registered, something you can also get listed out.
     * It can also give out lists of calculated toll fees for vehicles based on their type, date and time of passage.
     */

    public class PassageTollHandler
    {
        public const string UNSUPPORTED_VEHICLETYPE_EXCEPTION_MESSAGE = "Unsupported vehicle type.";

        // Dictionary to store vehicle passage registrations
        private readonly Dictionary<Vehicle, List<DateTime>> _vehiclePassageRegistry;

        public PassageTollHandler()
        {
            _vehiclePassageRegistry = new Dictionary<Vehicle, List<DateTime>>(new VehicleEqualityComparer());
        }

        // Method to register a vehicle passage
        public bool RegisterPassage(string licenseNumber, VehicleType vehicleType, DateTime timeStamp)
        {
            if (string.IsNullOrEmpty(licenseNumber))
            {
                return false; // The license number is invalid
            }
            
            var vehicle = CreateVehicle(licenseNumber, vehicleType);
            
            return RegisterPassage(vehicle, timeStamp);
        }

        private bool RegisterPassage(Vehicle vehicle, DateTime timeStamp)
        {
            if (!_vehiclePassageRegistry.ContainsKey(vehicle))
            {
                _vehiclePassageRegistry[vehicle] = new List<DateTime>();
            }

            // Only add the timestamp if it is not already present
            // This prevents duplicate entries for the same vehicle and timestamp
            if (_vehiclePassageRegistry[vehicle].Contains(timeStamp))
            {
                return false; // Passage already registered
            }

            _vehiclePassageRegistry[vehicle].Add(timeStamp);
            return true; // Passage successfully registered
        }

        private Vehicle CreateVehicle(string licenseNumber, VehicleType vehicleType)
        {
            return vehicleType switch
            {
                VehicleType.Car => 
                    new Car { LicenseNumber = licenseNumber },
                VehicleType.Motorbike => 
                    new Motorbike { LicenseNumber = licenseNumber },
                _ => 
                    throw new ArgumentException(UNSUPPORTED_VEHICLETYPE_EXCEPTION_MESSAGE),
            };
        }
    }
}