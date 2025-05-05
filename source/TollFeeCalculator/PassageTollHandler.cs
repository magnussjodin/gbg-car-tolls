
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
        private readonly TollCalculator _tollCalculator;

        public PassageTollHandler()
        {
            _vehiclePassageRegistry = new Dictionary<string, List<VehiclePassage>>();
            _tollCalculator = new TollCalculator();
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
            // Ensure start <= end
            if (fromTimeStamp > toTimeStamp)
            {
                throw new ArgumentException(TIMESTAMPS_IN_WRONG_ORDER_EXCEPTION_MESSAGE);
            }

            var allPassages = new List<VehiclePassage>();
            foreach (var licenseNumber in _vehiclePassageRegistry.Keys)
            {
                var passages = GetListOfPassagesWithTrustedParameters(licenseNumber, fromTimeStamp, toTimeStamp);
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
            // Ensure the license number is valid
            if (string.IsNullOrEmpty(licenseNumber))
            {
                throw new ArgumentException(INVALID_LICENSE_NUMBER_EXCEPTION_MESSAGE);
            }

            // Ensure start <= end
            if (fromTimeStamp > toTimeStamp)
            {
                throw new ArgumentException(TIMESTAMPS_IN_WRONG_ORDER_EXCEPTION_MESSAGE);
            }

            return GetListOfPassagesWithTrustedParameters(licenseNumber, fromTimeStamp, toTimeStamp);
        }

        public IOrderedEnumerable<DailyFee> GetListOfDailyFees(DateTime fromTimeStamp, DateTime toTimeStamp)
        {
            // Ensure start <= end
            if (fromTimeStamp > toTimeStamp)
            {
                throw new ArgumentException(TIMESTAMPS_IN_WRONG_ORDER_EXCEPTION_MESSAGE);
            }

            var allDailyFees = new List<DailyFee>();
            foreach (var licenseNumber in _vehiclePassageRegistry.Keys)
            {
                var dailyFees = GetListOfDailyFeessWithTrustedParameters(licenseNumber, fromTimeStamp, toTimeStamp);
                if (dailyFees != null)
                {
                    allDailyFees.AddRange(dailyFees);
                }
            }
            return allDailyFees
                .OrderBy(x => x.Vehicle?.LicenseNumber)
                .ThenBy(x => x.Date);
        }

        public IOrderedEnumerable<DailyFee> GetListOfDailyFees(string licenseNumber, DateTime fromTimeStamp, DateTime toTimeStamp)
        {
            // Ensure the license number is valid
            if (string.IsNullOrEmpty(licenseNumber))
            {
                throw new ArgumentException(INVALID_LICENSE_NUMBER_EXCEPTION_MESSAGE);
            }

            // Ensure start <= end
            if (fromTimeStamp > toTimeStamp)
            {
                throw new ArgumentException(TIMESTAMPS_IN_WRONG_ORDER_EXCEPTION_MESSAGE);
            }

            return GetListOfDailyFeessWithTrustedParameters(licenseNumber, fromTimeStamp, toTimeStamp);
        }


        private bool RegisterPassage(VehiclePassage passage)
        {
            var licenseNumber = passage.Vehicle.LicenseNumber;
            
            // Ensure the license number is valid
            if (string.IsNullOrEmpty(licenseNumber))
            {
                return false;
            }
            
            // Ensure the license number exists in the registry
            if (!_vehiclePassageRegistry.ContainsKey(licenseNumber))
            {
                _vehiclePassageRegistry[licenseNumber] = new List<VehiclePassage>();
            }

            // Ensure the passage is added if it is not already present
            // This prevents duplicate entries for the same vehicle and timestamp
            if (_vehiclePassageRegistry[licenseNumber].Exists(x => x.TimeStamp == passage.TimeStamp))
            {
                return false; // Passage already registered
            }

            _vehiclePassageRegistry[licenseNumber].Add(passage);
            return true; // Passage successfully registered
        }

        private IOrderedEnumerable<VehiclePassage> GetListOfPassagesWithTrustedParameters(string licenseNumber, DateTime fromTimeStamp, DateTime toTimeStamp)
        {
            // If no passage exists for the given license number, return an empty list
            if (!_vehiclePassageRegistry.TryGetValue(licenseNumber, out var passages))
            {
                return (IOrderedEnumerable<VehiclePassage>)new List<VehiclePassage>(); 
            }

            return passages
                .Where(x => x.TimeStamp >= fromTimeStamp && x.TimeStamp <= toTimeStamp)
                .OrderBy(x => x.TimeStamp);
        }
                
        private IOrderedEnumerable<DailyFee> GetListOfDailyFeessWithTrustedParameters(string licenseNumber, DateTime fromTimeStamp, DateTime toTimeStamp)
        {
            // An ordered list of passages for the license number and time range
            var passages = GetListOfPassagesWithTrustedParameters(licenseNumber, fromTimeStamp, toTimeStamp);

            // If no passage exists for the license number, return an empty list
            if (!passages.Any())
            {
                return (IOrderedEnumerable<DailyFee>)new List<DailyFee>(); 
            }

            // Get the vehicle type from the first passage
            var vehicleType = passages.Select(x => x.Vehicle.VehicleType).FirstOrDefault();

            // Return the daily fees for the vehicle type and the timestamps of the passages
            return _tollCalculator
                .GetListOfDailyTollFees(vehicleType, passages.Select(x => x.TimeStamp).ToArray())
                .Select(dailyFee => new DailyFee
                {
                    Vehicle = CreateVehicle(licenseNumber, vehicleType),
                    Date = dailyFee.Date,
                    Fee = dailyFee.Fee
                })
                .OrderBy(x => x.Date);
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