using System;
using System.ComponentModel;
using System.Windows;
using BO;
using BlApi;

namespace PL.Volunteer
{
    public partial class VolunteerMainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly IBl _bl = Factory.Get();

        private BO.Volunteer _currentVolunteer;
        public BO.Volunteer CurrentVolunteer
        {
            get => _currentVolunteer;
            set
            {
                _currentVolunteer = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentVolunteer)));
            }
        }

        public VolunteerMainWindow(BO.Volunteer volunteer)
        {
            InitializeComponent();
            CurrentVolunteer = volunteer;
            DataContext = this;
            _bl.Volunteer.AddObserver(volunteer.Id, RefreshVolunteer);
        }

        private void RefreshVolunteer()
        {
            CurrentVolunteer = _bl.Volunteer.GetVolunteerDetails(CurrentVolunteer.Id);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _bl.Volunteer.RemoveObserver(CurrentVolunteer.Id, RefreshVolunteer);
        }

        private void UpdateVolunteer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new Admin.VolunteerWindow(CurrentVolunteer.Id);
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("אירעה שגיאה בעת פתיחת מסך העדכון.\n\n" + ex.Message,
                                "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewClosedCalls_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new ClosedCallsWindow(CurrentVolunteer.Id);
                window.ShowDialog();
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
                var chooseCallWindow = new ChooseCallWindow(CurrentVolunteer.Id);
                chooseCallWindow.ShowDialog();
                RefreshVolunteer();
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
                    RefreshVolunteer();
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
                    RefreshVolunteer();
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
