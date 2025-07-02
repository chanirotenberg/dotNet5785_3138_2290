using BO;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace PL.Admin
{
    public partial class MainWindow : Window
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private volatile DispatcherOperation? _clockObserverOperation = null;
        private volatile DispatcherOperation? _configObserverOperation = null;
        private BO.Volunteer _currentVolunteer;

        public MainWindow(BO.Volunteer volunteer)
        {
            InitializeComponent();
            CurrentVolunteer = volunteer;
        }

        public BO.Volunteer CurrentVolunteer
        {
            get => _currentVolunteer;
            set => _currentVolunteer = value;
        }

        public DateTime CurrentTime
        {
            get => (DateTime)GetValue(CurrentTimeProperty);
            set => SetValue(CurrentTimeProperty, value);
        }

        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(MainWindow));

        public TimeSpan RiskRange
        {
            get => (TimeSpan)GetValue(RiskRangeProperty);
            set => SetValue(RiskRangeProperty, value);
        }

        public static readonly DependencyProperty RiskRangeProperty =
            DependencyProperty.Register("RiskRange", typeof(TimeSpan), typeof(MainWindow));

        public bool IsSimulatorRunning
        {
            get => (bool)GetValue(IsSimulatorRunningProperty);
            set => SetValue(IsSimulatorRunningProperty, value);
        }

        public static readonly DependencyProperty IsSimulatorRunningProperty =
            DependencyProperty.Register("IsSimulatorRunning", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public int Interval
        {
            get => (int)GetValue(IntervalProperty);
            set => SetValue(IntervalProperty, value);
        }

        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register("Interval", typeof(int), typeof(MainWindow), new PropertyMetadata(2000));

        private void clockObserver()
        {
            if (_clockObserverOperation is null || _clockObserverOperation.Status == DispatcherOperationStatus.Completed)
            {
                _clockObserverOperation = Dispatcher.BeginInvoke(() =>
                {
                    CurrentTime = s_bl.Admin.GetClock();
                });
            }
        }

        private void configObserver()
        {
            if (_configObserverOperation is null || _configObserverOperation.Status == DispatcherOperationStatus.Completed)
            {
                _configObserverOperation = Dispatcher.BeginInvoke(() =>
                {
                    RiskRange = s_bl.Admin.GetRiskRange();
                });
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentTime = s_bl.Admin.GetClock();
            RiskRange = s_bl.Admin.GetRiskRange();
            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            s_bl.Admin.RemoveClockObserver(clockObserver);
            s_bl.Admin.RemoveConfigObserver(configObserver);

            if (IsSimulatorRunning)
                s_bl.Admin.StopSimulator();
        }

        private void ToggleSimulator_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsSimulatorRunning)
                {
                    s_bl.Admin.StartSimulator(Interval);
                    IsSimulatorRunning = true;
                }
                else
                {
                    s_bl.Admin.StopSimulator();
                    IsSimulatorRunning = false;
                }
            }
            catch (Exception ex)
            {
                ShowError("Simulator Error", "An error occurred while starting/stopping the simulator.", ex);
            }
        }

        private void btnAddOneMinute_Click(object sender, RoutedEventArgs e) =>
            s_bl.Admin.AdvanceClock(BO.TimeUnit.Minute);

        private void btnAddOneHour_Click(object sender, RoutedEventArgs e) =>
            s_bl.Admin.AdvanceClock(BO.TimeUnit.Hour);

        private void btnAddOneDay_Click(object sender, RoutedEventArgs e) =>
            s_bl.Admin.AdvanceClock(BO.TimeUnit.Day);

        private void btnAddOneMonth_Click(object sender, RoutedEventArgs e) =>
            s_bl.Admin.AdvanceClock(BO.TimeUnit.Month);

        private void btnAddOneYear_Click(object sender, RoutedEventArgs e) =>
            s_bl.Admin.AdvanceClock(BO.TimeUnit.Year);

        private void UpdateRiskRange_Click(object sender, RoutedEventArgs e)
        {
            if (TimeSpan.TryParse(RiskRange.ToString(), out TimeSpan parsed) && parsed.TotalMinutes > 0)
                s_bl.Admin.SetRiskRange(parsed);
        }

        private void InitializeDB_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.InitializeDB();
        }

        private void ResetDB_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.ResetDB();
        }

        private void HandleVolunteer_Click(object sender, RoutedEventArgs e)
        {
            new VolunteerListWindow().Show();
        }

        private void HandleCall_Click(object sender, RoutedEventArgs e)
        {
            new CallManagementWindow(CurrentVolunteer.Id).Show();
        }

        private void ShowError(string title, string userMessage, Exception ex)
        {
            string message = userMessage;
            if (ex.InnerException != null)
                message += "\n\nTechnical details:\n" + ex.InnerException.Message;

            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
