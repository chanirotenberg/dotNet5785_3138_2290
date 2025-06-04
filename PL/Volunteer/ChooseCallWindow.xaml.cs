// ChooseCallWindow.xaml.cs
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

        private ObservableCollection<OpenCallInList> _openCalls;
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

        public ChooseCallWindow(int volunteerId)
        {
            InitializeComponent();
            _volunteerId = volunteerId;
            DataContext = this;
            RefreshOpenCalls();
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
        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            SelectedCallType = null;
            SortField = null;
        }


        private void CallsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedCall == null)
                return;

            try
            {
                _bl.Call.AssignCallToVolunteer(_volunteerId, SelectedCall.Id);

                MessageBox.Show("הקריאה שויכה בהצלחה.",
                                "שיבוץ הצליח",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                this.Close();
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
}
