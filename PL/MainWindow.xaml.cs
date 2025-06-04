using PL.Volunteer;
using PL.Volunteer.Admin;
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

namespace PL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        /// <summary>
        /// Initializes the main window.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Current simulated system time.
        /// </summary>
        public DateTime CurrentTime
        {
            get { return (DateTime)GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }

        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(MainWindow));

        /// <summary>
        /// Time range used for risk detection configuration.
        /// </summary>
        public TimeSpan RiskRange
        {
            get { return (TimeSpan)GetValue(RiskRangeProperty); }
            set { SetValue(RiskRangeProperty, value); }
        }

        public static readonly DependencyProperty RiskRangeProperty =
            DependencyProperty.Register("RiskRange", typeof(TimeSpan), typeof(MainWindow));

        /// <summary>
        /// Observer method for updating the current time.
        /// </summary>
        private void clockObserver()
        {
            CurrentTime = s_bl.Admin.GetClock();
        }

        /// <summary>
        /// Observer method for updating the risk range.
        /// </summary>
        private void configObserver()
        {
            RiskRange = s_bl.Admin.GetRiskRange();
        }

        /// <summary>
        /// Loads initial data and subscribes to observers.
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentTime = s_bl.Admin.GetClock();
            RiskRange = s_bl.Admin.GetRiskRange();
            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);
        }

        /// <summary>
        /// Unsubscribes from observers when window is closed.
        /// </summary>
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            s_bl.Admin.RemoveClockObserver(clockObserver);
            s_bl.Admin.RemoveConfigObserver(configObserver);
        }

        private void btnAddOneMinute_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                s_bl.Admin.AdvanceClock(BO.TimeUnit.Minute);
            }
            catch (Exception ex)
            {
                ShowError("Failed to advance clock by one minute", "An error occurred while updating the clock by one minute.", ex);
            }
        }

        private void btnAddOneHour_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                s_bl.Admin.AdvanceClock(BO.TimeUnit.Hour);
            }
            catch (Exception ex)
            {
                ShowError("Failed to advance clock by one hour", "An error occurred while updating the clock by one hour.", ex);
            }
        }

        private void btnAddOneDay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                s_bl.Admin.AdvanceClock(BO.TimeUnit.Day);
            }
            catch (Exception ex)
            {
                ShowError("Failed to advance clock by one day", "An error occurred while updating the clock by one day.", ex);
            }
        }

        private void btnAddOneMonth_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                s_bl.Admin.AdvanceClock(BO.TimeUnit.Month);
            }
            catch (Exception ex)
            {
                ShowError("Failed to advance clock by one month", "An error occurred while updating the clock by one month.", ex);
            }
        }

        private void btnAddOneYear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                s_bl.Admin.AdvanceClock(BO.TimeUnit.Year);
            }
            catch (Exception ex)
            {
                ShowError("Failed to advance clock by one year", "An error occurred while updating the clock by one year.", ex);
            }
        }

        private void UpdateRiskRange_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                s_bl.Admin.SetRiskRange(RiskRange);
            }
            catch (Exception ex)
            {
                ShowError("Failed to update risk range", "An error occurred while trying to update the risk range.", ex);
            }
        }

        private void InitializeDB_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to initialize the database?", "Confirm Initialization", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window != this)
                            window.Close();
                    }
                    s_bl.Admin.InitializeDB();
                }
                catch (Exception ex)
                {
                    ShowError("Database Initialization Error", "An error occurred while trying to initialize the database.", ex);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void ResetDB_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reset the database?", "Confirm Reset", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window != this)
                            window.Close();
                    }
                    s_bl.Admin.ResetDB();
                }
                catch (Exception ex)
                {
                    ShowError("Database Reset Error", "An error occurred while trying to reset the database.", ex);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void HandleVolunteer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new VolunteerListWindow().Show();
            }
            catch (Exception ex)
            {
                ShowError("Error opening volunteers window", "An error occurred while trying to open the volunteers window.", ex);
            }
        }

        /// <summary>
        /// Displays an error message with additional technical details.
        /// </summary>
        private void ShowError(string title, string userMessage, Exception ex)
        {
            string message = userMessage;
            if (ex.InnerException != null)
                message += "\n\nTechnical details:\n" + ex.InnerException.Message;

            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
