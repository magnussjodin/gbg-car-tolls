using System;
using Xunit;
using TollFeeCalculator;

namespace TollFeeCalculatorTests
{
    public class PassageTollHandlerTests
    {
        [Fact]
        public void RegisterListOfPassages_ShouldReturnZeroIfNoPassagesAreAdded()
        {
            var passageTollHandler = new PassageTollHandler();
            var passages = new List<VehiclePassage>();
            var expectedResult = 0;

            var result = passageTollHandler.RegisterListOfPassages(passages);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void RegisterListOfPassages_ShouldReturnNumberOfSuccessfullyAddedPassages()
        {
            var passageTollHandler = new PassageTollHandler();
            var timeStamp = DateTime.Now;
            var passages = new List<VehiclePassage>()
            {
                new VehiclePassage { Vehicle = new Car { LicenseNumber = "ABC 123" }, TimeStamp = timeStamp },
                new VehiclePassage { Vehicle = new Car { LicenseNumber = "DEF 456" }, TimeStamp = timeStamp.AddMinutes(1) },
                new VehiclePassage { Vehicle = new Car { LicenseNumber = string.Empty}, TimeStamp = timeStamp.AddMinutes(2) }, // Invalid license number
                new VehiclePassage { Vehicle = new Car { LicenseNumber = "ABC 123" }, TimeStamp = timeStamp } // Duplicate timestamp
            };
            var expectedResult = 2; // Only two passages should be added successfully

            var result = passageTollHandler.RegisterListOfPassages(passages);

            Assert.Equal(expectedResult, result);
        }
        
        [Fact]
        public void RegisterPassage_ShouldFailIfLicenseNumberIsEmpty()
        {
            var passageTollHandler = new PassageTollHandler();
            string licenseNumber = string.Empty;
            var vehicleType = GetValidVehicleType();
            var timeStamp = DateTime.Now;
            var expectedResult = false;

            var result = passageTollHandler.RegisterPassage(licenseNumber, vehicleType, timeStamp);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void RegisterPassage_ShouldRaiseExceptionForUnsupportedVehicleType()
        {
            var passageTollHandler = new PassageTollHandler();
            string licenseNumber = "ABC 123";
            var vehicleType = GetInvalidVehicleType();
            var timeStamp = DateTime.Now;
            var result = false;
            var expectedResult = false;

            try
            {
                result = passageTollHandler.RegisterPassage(licenseNumber, vehicleType, timeStamp);
            }
            catch (System.Exception ex)
            {
                Assert.IsType<ArgumentException>(ex);
                Assert.Equal(PassageTollHandler.UNSUPPORTED_VEHICLETYPE_EXCEPTION_MESSAGE, ex.Message);
            }

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void RegisterPassage_ShouldFailIfTimestampAlreadyRegisteredForVehicle()
        {
            var passageTollHandler = new PassageTollHandler();
            string licenseNumber = "ABC 123";
            var vehicleType = GetValidVehicleType();
            var timeStamp = DateTime.Now;
            var expectedResult = false;

            passageTollHandler.RegisterPassage(licenseNumber, vehicleType, timeStamp);
            var result = passageTollHandler.RegisterPassage(licenseNumber, vehicleType, timeStamp);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void RegisterPassage_ShouldSucceedToRegisterATimestamp()
        {
            var passageTollHandler = new PassageTollHandler();
            string licenseNumber = "ABC 123";
            var vehicleType = GetValidVehicleType();
            var timeStamp = DateTime.Now;
            var expectedResult = true;

            var result = passageTollHandler.RegisterPassage(licenseNumber, vehicleType, timeStamp);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void RegisterPassage_ShouldSucceedToRegisterDifferentTimestampsForSameVehicle()
        {
            var passageTollHandler = new PassageTollHandler();
            string licenseNumber = "ABC 123";
            var vehicleType = GetValidVehicleType();
            var timeStamp1 = DateTime.Now;
            var timeStamp2 = DateTime.Now.AddMinutes(1);
            var expectedResult = true;

            passageTollHandler.RegisterPassage(licenseNumber, vehicleType, timeStamp1);
            var result = passageTollHandler.RegisterPassage(licenseNumber, vehicleType, timeStamp2);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void RegisterPassage_ShouldSucceedToRegisterSameTimestampForDifferentVehicles()
        {
            var passageTollHandler = new PassageTollHandler();
            string licenseNumber1 = "ABC 123", licenseNumber2 = "DEF 456";
            var vehicleType = GetValidVehicleType();
            var timeStamp = DateTime.Now;
            var expectedResult = true;

            passageTollHandler.RegisterPassage(licenseNumber1, vehicleType, timeStamp);
            var result = passageTollHandler.RegisterPassage(licenseNumber2, vehicleType, timeStamp);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void RegisterPassage_ShouldFailToRegisterSameTimestampForSameLicenseNrButDifferentVehicleTypes()
        {
            var passageTollHandler = new PassageTollHandler();
            string licenseNumber = "ABC 123";
            var vehicleType1 = GetValidVehicleType();
            var vehicleType2 = VehicleType.Motorbike;
            var timeStamp = DateTime.Now;
            var expectedResult = false;

            passageTollHandler.RegisterPassage(licenseNumber, vehicleType1, timeStamp);
            var result = passageTollHandler.RegisterPassage(licenseNumber, vehicleType2, timeStamp);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void RegisterPassage_ShouldCapRegisteredTimeStampToTheMinute()
        {
            var passageTollHandler = new PassageTollHandler();
            string licenseNumber = "ABC 123";
            var vehicleType = GetValidVehicleType();
            var timeStamp = new DateTime(2013, 1, 2, 7, 59, 59, 999);
            var expectedTimeStamp = new DateTime(2013, 1, 2, 7, 59, 0, 0);

            var result = passageTollHandler.RegisterPassage(licenseNumber, vehicleType, timeStamp);
            Assert.True(result);

            var passageList = passageTollHandler.GetListOfPassages(licenseNumber, expectedTimeStamp, expectedTimeStamp.AddMinutes(1));
            Assert.True(passageList is not null && passageList.Any());
            Assert.Equal(passageList.FirstOrDefault()?.TimeStamp, expectedTimeStamp);
        }

        [Fact]
        public void RegisterPassage_ShouldFailAndTreatATimeStampThatDiffersOnlyOnTheSecondAsDuplicateEntry()
        {
            var passageTollHandler = new PassageTollHandler();
            string licenseNumber = "ABC 123";
            var vehicleType = GetValidVehicleType();
            var timeStamp1 = DateTime.Now;
            var timeStamp2 = timeStamp1.AddSeconds(1);

            passageTollHandler.RegisterPassage(licenseNumber, vehicleType, timeStamp1);
            var result = passageTollHandler.RegisterPassage(licenseNumber, vehicleType, timeStamp2);

            Assert.False(result);
        }

        [Fact]
        public void GetListOfPassages_SpecificLicenseNr_ShouldRaiseExceptionForEmptyLicenseNr()
        {
            var passageTollHandler = new PassageTollHandler();
            passageTollHandler.RegisterListOfPassages(GetVehiclePassageTestData());

            string? licenseNumber = string.Empty;
            var fromTimeStamp = new DateTime(2013, 1, 2, 0, 0, 0);
            var toTimeStamp = new DateTime(2013, 1, 3, 0, 0, 0);
            IOrderedEnumerable<VehiclePassage>? list = null;

            try
            {
                list = passageTollHandler.GetListOfPassages(licenseNumber, fromTimeStamp, toTimeStamp);
            }
            catch (System.Exception ex)
            {
                Assert.IsType<ArgumentException>(ex);
                Assert.Equal(PassageTollHandler.INVALID_LICENSE_NUMBER_EXCEPTION_MESSAGE, ex.Message);
            }
            Assert.True(list is null || list.Count() == 0);
        }

        [Fact]
        public void GetListOfPassages_SpecificLicenseNr_ShouldRaiseExceptionWhenFromDateLaterThanToDate()
        {
            var passageTollHandler = new PassageTollHandler();
            passageTollHandler.RegisterListOfPassages(GetVehiclePassageTestData());

            string licenseNumber = "ABC 123";
            var fromTimeStamp = new DateTime(2013, 1, 3, 0, 0, 0);
            var toTimeStamp = new DateTime(2013, 1, 2, 0, 0, 0);
            IOrderedEnumerable<VehiclePassage>? list = null;

            try
            {
                list = passageTollHandler.GetListOfPassages(licenseNumber, fromTimeStamp, toTimeStamp);
            }
            catch (System.Exception ex)
            {
                Assert.IsType<ArgumentException>(ex);
                Assert.Equal(PassageTollHandler.TIMESTAMPS_IN_WRONG_ORDER_EXCEPTION_MESSAGE, ex.Message);
            }

            Assert.True(list is null || list.Count() == 0);
        }

        [Fact]
        public void GetListOfPassages_SpecificLicenseNr_ShouldReturnEmptyListForNonExistingLicenseNr()
        {
            var passageTollHandler = new PassageTollHandler();
            passageTollHandler.RegisterListOfPassages(GetVehiclePassageTestData());

            string licenseNumber = "ABC 789";
            var fromTimeStamp = new DateTime(2013, 1, 2, 0, 0, 0);
            var toTimeStamp = new DateTime(2013, 1, 3, 0, 0, 0);
            IOrderedEnumerable<VehiclePassage>? list = null;

            try
            {
                list = passageTollHandler.GetListOfPassages(licenseNumber, fromTimeStamp, toTimeStamp);
            }
            catch (System.Exception)
            {
            }

            Assert.True(list is null || list.Count() == 0);
        }

        [Fact]
        public void GetListOfPassages_SpecificLicenseNr_ShouldReturnPassagesOnlyWithDatesInTimeStampInterval()
        {
            var passageTollHandler = new PassageTollHandler();
            var passageList = GetVehiclePassageTestData();
            passageTollHandler.RegisterListOfPassages(passageList);

            string licenseNumber = "ABC 123";
            var fromTimeStamp = new DateTime(2013, 1, 3, 0, 0, 0);
            var toTimeStamp = new DateTime(2013, 1, 4, 0, 0, 0);
            IOrderedEnumerable<VehiclePassage>? list = null;

            var expectedCount = passageList
                .Where(x => x.Vehicle.LicenseNumber == licenseNumber && x.TimeStamp >= fromTimeStamp && x.TimeStamp <= toTimeStamp)
                .Count();

            try
            {
                list = passageTollHandler.GetListOfPassages(licenseNumber, fromTimeStamp, toTimeStamp);
            }
            catch (System.Exception)
            {
            }

            Assert.True(list is not null);
            Assert.Equal(expectedCount, list.Count());
        }

        [Fact]
        public void GetListOfPassages_All_ShouldRaiseExceptionWhenFromDateLaterThanToDate()
        {
            var passageTollHandler = new PassageTollHandler();
            passageTollHandler.RegisterListOfPassages(GetVehiclePassageTestData());

            var fromTimeStamp = new DateTime(2013, 1, 3, 0, 0, 0);
            var toTimeStamp = new DateTime(2013, 1, 2, 0, 0, 0);
            IOrderedEnumerable<VehiclePassage>? list = null;

            try
            {
                list = passageTollHandler.GetListOfPassages(fromTimeStamp, toTimeStamp);
            }
            catch (System.Exception ex)
            {
                Assert.IsType<ArgumentException>(ex);
                Assert.Equal(PassageTollHandler.TIMESTAMPS_IN_WRONG_ORDER_EXCEPTION_MESSAGE, ex.Message);
            }

            Assert.True(list is null || list.Count() == 0);
        }

        [Fact]
        public void GetListOfPassages_All_ShouldReturnPassagesForAnyVehicleWithDatesInTimeStampInterval()
        {
            var passageTollHandler = new PassageTollHandler();
            var passageList = GetVehiclePassageTestData();
            passageTollHandler.RegisterListOfPassages(passageList);

            var fromTimeStamp = new DateTime(2013, 1, 3, 0, 0, 0);
            var toTimeStamp = new DateTime(2013, 1, 4, 0, 0, 0);
            IOrderedEnumerable<VehiclePassage>? list = null;

            var expectedCount = passageList
                .Where(x => x.TimeStamp >= fromTimeStamp && x.TimeStamp <= toTimeStamp)
                .Count();

            try
            {
                list = passageTollHandler.GetListOfPassages(fromTimeStamp, toTimeStamp);
            }
            catch (System.Exception)
            {
            }

            Assert.True(list is not null);
            Assert.Equal(expectedCount, list.Count());
        }


        [Fact]
        public void GetListOfDailyFees_SpecificLicenseNr_ShouldRaiseExceptionForEmptyLicenseNr()
        {
            var passageTollHandler = new PassageTollHandler();
            passageTollHandler.RegisterListOfPassages(GetVehiclePassageTestData());

            string? licenseNumber = string.Empty;
            var fromTimeStamp = new DateTime(2013, 1, 2, 0, 0, 0);
            var toTimeStamp = new DateTime(2013, 1, 3, 0, 0, 0);
            IOrderedEnumerable<DailyFee>? list = null;

            try
            {
                list = passageTollHandler.GetListOfDailyFees(licenseNumber, fromTimeStamp, toTimeStamp);
            }
            catch (System.Exception ex)
            {
                Assert.IsType<ArgumentException>(ex);
                Assert.Equal(PassageTollHandler.INVALID_LICENSE_NUMBER_EXCEPTION_MESSAGE, ex.Message);
            }
            Assert.True(list is null || list.Count() == 0);
        }

        [Fact]
        public void GetListOfDailyFees_SpecificLicenseNr_ShouldRaiseExceptionWhenFromDateLaterThanToDate()
        {
            var passageTollHandler = new PassageTollHandler();
            passageTollHandler.RegisterListOfPassages(GetVehiclePassageTestData());

            string licenseNumber = "ABC 123";
            var fromTimeStamp = new DateTime(2013, 1, 3, 0, 0, 0);
            var toTimeStamp = new DateTime(2013, 1, 2, 0, 0, 0);
            IOrderedEnumerable<DailyFee>? list = null;

            try
            {
                list = passageTollHandler.GetListOfDailyFees(licenseNumber, fromTimeStamp, toTimeStamp);
            }
            catch (System.Exception ex)
            {
                Assert.IsType<ArgumentException>(ex);
                Assert.Equal(PassageTollHandler.TIMESTAMPS_IN_WRONG_ORDER_EXCEPTION_MESSAGE, ex.Message);
            }

            Assert.True(list is null || list.Count() == 0);
        }

        [Fact]
        public void GetListOfDailyFees_SpecificLicenseNr_ShouldReturnEmptyListForNonExistingLicenseNr()
        {
            var passageTollHandler = new PassageTollHandler();
            passageTollHandler.RegisterListOfPassages(GetVehiclePassageTestData());

            string licenseNumber = "ABC 789";
            var fromTimeStamp = new DateTime(2013, 1, 2, 0, 0, 0);
            var toTimeStamp = new DateTime(2013, 1, 3, 0, 0, 0);
            IOrderedEnumerable<DailyFee>? list = null;

            try
            {
                list = passageTollHandler.GetListOfDailyFees(licenseNumber, fromTimeStamp, toTimeStamp);
            }
            catch (System.Exception)
            {
            }

            Assert.True(list is null || list.Count() == 0);
        }

        [Fact]
        public void GetListOfDailyFees_SpecificLicenseNr_ShouldReturnDailyFeesOnlyWithDatesInTimeStampInterval()
        {
            var passageTollHandler = new PassageTollHandler();
            var passageList = GetVehiclePassageTestData();
            passageTollHandler.RegisterListOfPassages(passageList);

            string licenseNumber = "ABC 123";
            var fromTimeStamp = new DateTime(2013, 1, 3, 0, 0, 0);
            var toTimeStamp = new DateTime(2013, 1, 4, 0, 0, 0);
            IOrderedEnumerable<DailyFee>? list = null;

            var expectedCount = passageList
                .Where(x => x.Vehicle.LicenseNumber == licenseNumber && x.TimeStamp >= fromTimeStamp && x.TimeStamp <= toTimeStamp)
                .Select(x => x.TimeStamp.Date)
                .Distinct()
                .Count();

            try
            {
                list = passageTollHandler.GetListOfDailyFees(licenseNumber, fromTimeStamp, toTimeStamp);
            }
            catch (System.Exception)
            {
            }

            Assert.True(list is not null);
            Assert.Equal(expectedCount, list.Count());
        }

        [Fact]
        public void GetListOfDailyFees_All_ShouldRaiseExceptionWhenFromDateLaterThanToDate()
        {
            var passageTollHandler = new PassageTollHandler();
            passageTollHandler.RegisterListOfPassages(GetVehiclePassageTestData());

            var fromTimeStamp = new DateTime(2013, 1, 3, 0, 0, 0);
            var toTimeStamp = new DateTime(2013, 1, 2, 0, 0, 0);
            IOrderedEnumerable<DailyFee>? list = null;

            try
            {
                list = passageTollHandler.GetListOfDailyFees(fromTimeStamp, toTimeStamp);
            }
            catch (System.Exception ex)
            {
                Assert.IsType<ArgumentException>(ex);
                Assert.Equal(PassageTollHandler.TIMESTAMPS_IN_WRONG_ORDER_EXCEPTION_MESSAGE, ex.Message);
            }

            Assert.True(list is null || list.Count() == 0);
        }

        [Fact]
        public void GetListOfDailyFees_All_ShouldReturnDailyFeesForAnyVehicleWithDatesInTimeStampInterval()
        {
            var passageTollHandler = new PassageTollHandler();
            var passageList = GetVehiclePassageTestData();
            passageTollHandler.RegisterListOfPassages(passageList);

            var fromTimeStamp = new DateTime(2013, 1, 3, 0, 0, 0);
            var toTimeStamp = new DateTime(2013, 1, 4, 0, 0, 0);
            IOrderedEnumerable<DailyFee>? list = null;

            var expectedCount = passageList
                .Where(x => x.TimeStamp >= fromTimeStamp && x.TimeStamp <= toTimeStamp)
                .Select(x => ( x.Vehicle?.LicenseNumber, x.TimeStamp.Date ))
                .Distinct()
                .Count();

            try
            {
                list = passageTollHandler.GetListOfDailyFees(fromTimeStamp, toTimeStamp);
            }
            catch (System.Exception)
            {
            }

            Assert.True(list is not null);
            Assert.Equal(expectedCount, list.Count());
        }

        private VehicleType GetInvalidVehicleType()
        {
            return VehicleType.Tractor;
        }

        private VehicleType GetValidVehicleType()
        {
            return VehicleType.Car;
        }

        private static IEnumerable<VehiclePassage> GetVehiclePassageTestData()
        {
            var car1 = new Car() { LicenseNumber = "ABC 123" };
            var car2 = new Car() { LicenseNumber = "DEF 456" };
            var car3 = new Car() { LicenseNumber = "GHI 789" };
            var motorBike1 = new Motorbike() { LicenseNumber = "MC 1000" };

            var timeStampsThatGivesDailyMax = new List<DateTime>
            {
                new DateTime(2013, 1, 2, 6, 29, 0), // 8
                new DateTime(2013, 1, 2, 7, 0, 0), // 18, the maximum fee in 1st time period
                new DateTime(2013, 1, 2, 7, 59, 0), // 18, the maximum fee in 2nd time period
                new DateTime(2013, 1, 2, 8, 30, 0), // 8
                new DateTime(2013, 1, 2, 14, 44, 0), // 8, the maximum fee in 3rd time period
                new DateTime(2013, 1, 2, 16, 59, 0), // 18, the maximum fee in 4th time period
            };

            var timeStampsThatGivesALowerDailyThanMax = new List<DateTime>
            {
                new DateTime(2013, 1, 3, 6, 29, 0), // 8
                new DateTime(2013, 1, 3, 7, 0, 0), // 18, the maximum fee in 1st time period
                new DateTime(2013, 1, 3, 7, 59, 0), // 18, the maximum fee in 2nd time period
                new DateTime(2013, 1, 3, 8, 30, 0), // 8
            };

            var timeStampsThatAreTollFree = new List<DateTime>
            {
                new DateTime(2013, 1, 4, 0, 0, 0), // Before fee period
                new DateTime(2013, 1, 4, 5, 29, 0), // Before fee period
                new DateTime(2013, 1, 4, 21, 29, 0), // After fee period
                new DateTime(2013, 1, 7, 10, 0, 0), // Saturday
                new DateTime(2013, 1, 8, 10, 0, 0), // Sunday
                new DateTime(2013, 7, 3, 7, 59, 0), // July
                new DateTime(2013, 7, 31, 7, 59, 0), // July
            };

            foreach (var timeStamp in timeStampsThatGivesDailyMax)
            {
                yield return new VehiclePassage { Vehicle = car1, TimeStamp = timeStamp };
                yield return new VehiclePassage { Vehicle = car2, TimeStamp = timeStamp };
                yield return new VehiclePassage { Vehicle = car3, TimeStamp = timeStamp };
                yield return new VehiclePassage { Vehicle = motorBike1, TimeStamp = timeStamp };
            };

            foreach (var timeStamp in timeStampsThatGivesALowerDailyThanMax)
            {
                yield return new VehiclePassage { Vehicle = car1, TimeStamp = timeStamp };
                yield return new VehiclePassage { Vehicle = car2, TimeStamp = timeStamp };
                yield return new VehiclePassage { Vehicle = car3, TimeStamp = timeStamp };
                yield return new VehiclePassage { Vehicle = motorBike1, TimeStamp = timeStamp };
            };

            foreach (var timeStamp in timeStampsThatAreTollFree)
            {
                yield return new VehiclePassage { Vehicle = car1, TimeStamp = timeStamp };
                yield return new VehiclePassage { Vehicle = car2, TimeStamp = timeStamp };
                yield return new VehiclePassage { Vehicle = car3, TimeStamp = timeStamp };
                yield return new VehiclePassage { Vehicle = motorBike1, TimeStamp = timeStamp };
            };
        }
    }
}