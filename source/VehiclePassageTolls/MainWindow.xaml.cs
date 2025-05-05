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

                _passageTollHandler.RegisterPassage(licenseNumber, vehicleType, timestamp);
                Filter_Click(sender, e);
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
    }
}