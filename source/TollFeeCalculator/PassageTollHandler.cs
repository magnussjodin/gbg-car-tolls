
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
        public const string INVALID_LICENSE_NUMBER_EXCEPTION_MESSAGE = "Invalid license number.";
        public const string TIMESTAMPS_IN_WRONG_ORDER_EXCEPTION_MESSAGE = "lower boundary timestamp is later than higher boundary timestamp.";

        // Dictionary to store vehicle passage registrations
        private readonly Dictionary<string, List<VehiclePassage>> _vehiclePassageRegistry;

        public PassageTollHandler()
        {
            _vehiclePassageRegistry = new Dictionary<string, List<VehiclePassage>>();
        }

        // Method to add a list of vehicle passages to the registred passages
        public int RegisterListOfPassages(IEnumerable<VehiclePassage> passages)
        {
            int numberOfAddedPassages = 0;

            foreach (var passage in passages)
            {
                if (RegisterPassage(passage))
                {
                    numberOfAddedPassages++;
                }
            }

            return numberOfAddedPassages;            
        }

        // Method to register a vehicle passage, normalizing the timestamp by setting seconds and milliseconds to 0
        // This cappes down to the minute and avoids duplicate entries for the same vehicle and timestamp
        public bool RegisterPassage(string licenseNumber, VehicleType vehicleType, DateTime timeStamp)
        {
            var passage = new VehiclePassage()
            { 
                Vehicle = CreateVehicle(licenseNumber, vehicleType),
                TimeStamp = timeStamp.AddSeconds(-timeStamp.Second)
                                     .AddMilliseconds(-timeStamp.Millisecond)
            };
            
            return RegisterPassage(passage);
        }

        public IOrderedEnumerable<VehiclePassage> GetListOfPassages(DateTime fromTimeStamp, DateTime toTimeStamp)
        {
            // Check if start <= end
            if (fromTimeStamp > toTimeStamp)
            {
                throw new ArgumentException(TIMESTAMPS_IN_WRONG_ORDER_EXCEPTION_MESSAGE);
            }

            var allPassages = new List<VehiclePassage>();
            foreach (var licenseNumber in _vehiclePassageRegistry.Keys)
            {
                var passages = GetListOfPassages(licenseNumber, fromTimeStamp, toTimeStamp);
                if (passages != null)
                {
                    allPassages.AddRange(passages);
                }
            }
            return allPassages
                .OrderBy(x => x.Vehicle.LicenseNumber);
        }

        public IOrderedEnumerable<VehiclePassage> GetListOfPassages(string licenseNumber, DateTime fromTimeStamp, DateTime toTimeStamp)
        {
            if (string.IsNullOrEmpty(licenseNumber))
            {
                throw new ArgumentException(INVALID_LICENSE_NUMBER_EXCEPTION_MESSAGE);
            }

            // Check if the license number is valid (e.g., matches a specific format)
            if (fromTimeStamp > toTimeStamp)
            {
                throw new ArgumentException(TIMESTAMPS_IN_WRONG_ORDER_EXCEPTION_MESSAGE);
            }

            // Check if passages exist for the given license number
            if (!_vehiclePassageRegistry.TryGetValue(licenseNumber, out var passages))
            {
                return (IOrderedEnumerable<VehiclePassage>)new List<VehiclePassage>(); 
            }

            return passages
                .Where(x => x.TimeStamp >= fromTimeStamp && x.TimeStamp <= toTimeStamp)
                .OrderBy(x => x.TimeStamp);
        }


        private bool RegisterPassage(VehiclePassage passage)
        {
            var licenseNumber = passage.Vehicle.LicenseNumber;
            
            // Check if the license number is invalid
            if (string.IsNullOrEmpty(licenseNumber))
            {
                return false;
            }
            
            // Check if the passage already has a registration
            if (!_vehiclePassageRegistry.ContainsKey(licenseNumber))
            {
                _vehiclePassageRegistry[licenseNumber] = new List<VehiclePassage>();
            }

            // Only add the passage if it is not already present
            // This prevents duplicate entries for the same vehicle and timestamp
            if (_vehiclePassageRegistry[licenseNumber].Exists(x => x.TimeStamp == passage.TimeStamp))
            {
                return false; // Passage already registered
            }

            _vehiclePassageRegistry[licenseNumber].Add(passage);
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