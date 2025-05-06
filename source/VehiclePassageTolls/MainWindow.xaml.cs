using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TollFeeCalculator;

namespace VehiclePassageTolls
{
    public partial class MainWindow : Window
    {
        private readonly PassageTollHandler _passageTollHandler;

        public MainWindow()
        {
            InitializeComponent();
            _passageTollHandler = new PassageTollHandler();

            // Load initial passages
            _passageTollHandler.RegisterListOfPassages(GetVehiclePassageTestData());
        }

        private void RegisterPassage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var licenseNumber = LicenseNumberInput.Text;
                var vehicleType = (VehicleType)Enum.Parse(typeof(VehicleType), (VehicleTypeInput.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Car");
                var date = TimestampInputDate.SelectedDate ?? DateTime.Now.Date;
                var time = TimeSpan.Parse(TimestampInputTime.Text);

                var timestamp = date.Add(time);

                if (timestamp.Year != 2013)
                {
                    MessageBox.Show("NB! Only dates during 2013 are fully supported in this app version.");
                }

                _passageTollHandler.RegisterPassage(licenseNumber, vehicleType, timestamp);
                Filter_Click(sender, e); // Refresh the data grids after registering a passage
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var licenseNumber = FilterLicenseNumberInput.Text;
                var fromDate = FilterFromDate.SelectedDate ?? DateTime.MinValue;
                var toDate = FilterToDate.SelectedDate ?? DateTime.MaxValue;

                if (fromDate != DateTime.MinValue && fromDate.Year != 2013 || 
                    toDate != DateTime.MaxValue && toDate.Year != 2013 )
                {
                    MessageBox.Show("NB! Only dates during 2013 are fully supported in this app version.");
                }

                // Filter passages
                IOrderedEnumerable<VehiclePassage> passages;
                if (string.IsNullOrEmpty(licenseNumber))
                    passages = _passageTollHandler.GetListOfPassages(fromDate, toDate);
                else
                    passages = _passageTollHandler.GetListOfPassages(licenseNumber, fromDate, toDate);
                PassagesDataGrid.ItemsSource = passages.ToList();

                // Filter daily fees
                IOrderedEnumerable<DailyFee> dailyFees;
                if (string.IsNullOrEmpty(licenseNumber))
                    dailyFees = _passageTollHandler.GetListOfDailyFees(fromDate, toDate);
                else
                    dailyFees = _passageTollHandler.GetListOfDailyFees(licenseNumber, fromDate, toDate);
                DailyFeesDataGrid.ItemsSource = dailyFees.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Filter_Click(sender, e); // Initially update the data grids with all passages and daily fees
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