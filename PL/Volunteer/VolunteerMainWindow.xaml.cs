using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using BO;
using BlApi;

namespace PL.Volunteer
{
    public partial class VolunteerMainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly IBl _bl = Factory.Get();
        private readonly int volunteerId;

        private BO.Volunteer _currentVolunteer;
        public BO.Volunteer CurrentVolunteer
        {
            get => _currentVolunteer;
            set
            {
                _currentVolunteer = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentVolunteer)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CallInProgress)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanChooseCall)));
            }
        }

        public CallInProgress? CallInProgress => CurrentVolunteer.CallInProgress;
        public bool CanChooseCall => CurrentVolunteer.IsActive && CurrentVolunteer.CallInProgress == null;

        private volatile DispatcherOperation? _refreshOperation = null;

        public VolunteerMainWindow(BO.Volunteer volunteer)
        {
           
            volunteerId = volunteer.Id;
            CurrentVolunteer = volunteer;
            InitializeComponent();

            _bl.Volunteer.AddObserver(volunteerId, RefreshVolunteer);
        }

        private void RefreshVolunteer() // Stage 7
        {
            if (_refreshOperation is null || _refreshOperation.Status == DispatcherOperationStatus.Completed)
            {
                _refreshOperation = Dispatcher.BeginInvoke(() =>
                {
                    CurrentVolunteer = _bl.Volunteer.GetVolunteerDetails(volunteerId);
                });
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _bl.Volunteer.RemoveObserver(volunteerId, RefreshVolunteer);
        }

        private void UpdateVolunteer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CurrentVolunteer.Password))
                {
                    var existing = _bl.Volunteer.GetVolunteerDetails(CurrentVolunteer.Id);
                    CurrentVolunteer.Password = existing.Password;
                }

                _bl.Volunteer.UpdateVolunteer(CurrentVolunteer.Id, CurrentVolunteer);
                MessageBox.Show("הפרטים עודכנו בהצלחה.", "עדכון מתנדב", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                RefreshVolunteer();
                MessageBox.Show("אירעה שגיאה בעת עדכון המתנדב:\n" + ex.Message, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewClosedCalls_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new ClosedCallsWindow(CurrentVolunteer.Id);
                window.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("אירעה שגיאה בעת טעינת הקריאות הסגורות.\n\n" + ex.Message,
                                "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewOpenCalls_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new ChooseCallWindow(CurrentVolunteer.Id);
                window.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("אירעה שגיאה בעת פתיחת מסך הקריאות.\n\n" + ex.Message,
                                "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseCurrentCall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentVolunteer.CallInProgress is not null)
                {
                    _bl.Call.CloseCall(CurrentVolunteer.Id, CurrentVolunteer.CallInProgress.Id);
                    MessageBox.Show("הקריאה נסגרה בהצלחה!", "סיום קריאה", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("אירעה שגיאה בסגירת הקריאה.\n\n" + ex.Message,
                                "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelCurrentCall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentVolunteer.CallInProgress is not null)
                {
                    _bl.Call.CancelCall(CurrentVolunteer.Id, CurrentVolunteer.CallInProgress.Id);
                    MessageBox.Show("הקריאה בוטלה בהצלחה.", "ביטול קריאה", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("אירעה שגיאה בביטול הקריאה.\n\n" + ex.Message,
                                "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e) => Close();

    }
}
