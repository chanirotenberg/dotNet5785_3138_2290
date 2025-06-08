using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
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
            set
            {
                _selectedCallType = value;
                OnPropertyChanged(nameof(SelectedCallType));
                RefreshOpenCalls();
            }
        }

        private CallSortAndFilterField? _sortField;
        public CallSortAndFilterField? SortField
        {
            get => _sortField;
            set
            {
                _sortField = value;
                OnPropertyChanged(nameof(SortField));
                RefreshOpenCalls();
            }
        }

        public ICommand AssignCallCommand { get; }

        public ChooseCallWindow(int volunteerId)
        {
            InitializeComponent();
            _volunteerId = volunteerId;
            DataContext = this;

            AssignCallCommand = new RelayCommand<OpenCallInList>(AssignCall);

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
                MessageBox.Show("אירעה שגיאה בעת טעינת הקריאות: " + ex.Message,
                                "שגיאה בטעינת נתונים",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                Close();
            }
        }

        private async void UpdateAddressButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // עדכון הכתובת במערכת
                var volunteer = _bl.Volunteer.GetVolunteerDetails(_volunteerId);
                volunteer.Address = VolunteerAddress;

                // כאן אפשר להוסיף חישוב קואורדינטות לפי הכתובת (אם יש לך שירות גיאוקוד)
                // var coords = geocodingService.GetCoordinates(VolunteerAddress);
                // volunteer.Latitude = coords.Latitude;
                // volunteer.Longitude = coords.Longitude;

                _bl.Volunteer.UpdateVolunteer(volunteer.Id,volunteer);

                MessageBox.Show("כתובת עודכנה בהצלחה.",
                                "הצלחה",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                RefreshOpenCalls(); // רענון הקריאות בהתאם לכתובת החדשה
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בעדכון הכתובת: " + ex.Message,
                                "שגיאה",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void AssignCall(OpenCallInList call)
        {
            if (call == null) return;

            try
            {
                _bl.Call.AssignCallToVolunteer(_volunteerId, call.Id);

                MessageBox.Show("הקריאה שויכה בהצלחה.",
                                "שיבוץ הצליח",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                Close();
            }
            catch (BlDoesNotExistException)
            {
                MessageBox.Show("הקריאה שבחרת כבר אינה זמינה.",
                                "שגיאה בשיבוץ",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
            catch (BlInvalidValueException)
            {
                MessageBox.Show("לא ניתן לשבץ את הקריאה למתנדב.",
                                "שיבוץ לא חוקי",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("אירעה שגיאה בשיבוץ הקריאה:\n" + ex.Message,
                                "שגיאת מערכת",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // מחלקת עזר לפקודות
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T>? _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute((T)parameter!);

        public void Execute(object? parameter)
        {
            if (parameter is T param)
                _execute(param);
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
