using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using BlApi;
using BO;

namespace PL.Volunteer
{
    public partial class ChooseCallWindow : Window, INotifyPropertyChanged
    {
        private readonly IBl _bl = Factory.Get();
        private readonly int _volunteerId;

        public event PropertyChangedEventHandler? PropertyChanged;

        private ObservableCollection<OpenCallInList> _openCalls = new();
        public ObservableCollection<OpenCallInList> OpenCalls
        {
            get => _openCalls;
            set { _openCalls = value; OnPropertyChanged(nameof(OpenCalls)); }
        }

        private OpenCallInList? _selectedCall;
        public OpenCallInList? SelectedCall
        {
            get => _selectedCall;
            set { _selectedCall = value; OnPropertyChanged(nameof(SelectedCall)); }
        }

        private string _volunteerAddress = "";
        public string VolunteerAddress
        {
            get => _volunteerAddress;
            set { _volunteerAddress = value; OnPropertyChanged(nameof(VolunteerAddress)); }
        }

        public Array CallTypes => Enum.GetValues(typeof(CallType));
        public Array CallSortFields => Enum.GetValues(typeof(CallSortAndFilterField));

        private CallType? _selectedCallType;
        public CallType? SelectedCallType
        {
            get => _selectedCallType;
            set { _selectedCallType = value; OnPropertyChanged(nameof(SelectedCallType)); RefreshOpenCalls(); }
        }

        private CallSortAndFilterField? _sortField;
        public CallSortAndFilterField? SortField
        {
            get => _sortField;
            set { _sortField = value; OnPropertyChanged(nameof(SortField)); RefreshOpenCalls(); }
        }

        public ChooseCallWindow(int volunteerId)
        {
            InitializeComponent();
            _volunteerId = volunteerId;
            DataContext = this;
            LoadVolunteerAddress();
            RefreshOpenCalls();
        }

        private void LoadVolunteerAddress()
        {
            try
            {
                var volunteer = _bl.Volunteer.GetVolunteerDetails(_volunteerId);
                VolunteerAddress = volunteer.Address ?? "";
            }
            catch
            {
                VolunteerAddress = "";
            }
        }

        private void RefreshOpenCalls()
        {
            try
            {
                var result = _bl.Call.GetOpenCallsForVolunteer(_volunteerId, SelectedCallType, SortField);
                OpenCalls = new ObservableCollection<OpenCallInList>(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show("אירעה שגיאה בעת טעינת הקריאות: " + ex.Message, "שגיאה בטעינת נתונים", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void UpdateAddressButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var volunteer = _bl.Volunteer.GetVolunteerDetails(_volunteerId);
                volunteer.Address = VolunteerAddress;
                _bl.Volunteer.UpdateVolunteer(volunteer.Id, volunteer);
                MessageBox.Show("כתובת עודכנה בהצלחה.", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshOpenCalls();
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בעדכון הכתובת: " + ex.Message, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AssignCall_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCall == null) return;
            try
            {
                _bl.Call.AssignCallToVolunteer(_volunteerId, SelectedCall.Id);
                MessageBox.Show("הקריאה שויכה בהצלחה.", "שיבוץ הצליח", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (BlDoesNotExistException)
            {
                MessageBox.Show("הקריאה שבחרת כבר אינה זמינה.", "שגיאה בשיבוץ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (BlInvalidValueException)
            {
                MessageBox.Show("לא ניתן לשבץ את הקריאה למתנדב.", "שיבוץ לא חוקי", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("אירעה שגיאה בשיבוץ הקריאה:\n" + ex.Message, "שגיאת מערכת", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
