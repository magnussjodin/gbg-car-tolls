using System;
using Xunit;
using TollFeeCalculator;

namespace TollFeeCalculatorTests
{
    public class PassageTollHandlerTests
    {
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

        private VehicleType GetInvalidVehicleType()
        {
            return VehicleType.Tractor;
        }

        private VehicleType GetValidVehicleType()
        {
            return VehicleType.Car;
        }
    }
}