using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading; // נדרש לשימוש ב־DispatcherOperation
using BlApi;
using BO;

namespace PL.Volunteer
{
    public partial class ClosedCallsWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly IBl _bl = Factory.Get();
        private readonly int _volunteerId;

        public Array CallTypes => Enum.GetValues(typeof(CallType));
        public Array CallSortFields => Enum.GetValues(typeof(CallSortAndFilterField));

        private ObservableCollection<ClosedCallInList> _closedCalls = new();
        public ObservableCollection<ClosedCallInList> ClosedCalls
        {
            get => _closedCalls;
            set { _closedCalls = value; OnPropertyChanged(nameof(ClosedCalls)); }
        }

        private CallType? _selectedCallType;
        public CallType? SelectedCallType
        {
            get => _selectedCallType;
            set { _selectedCallType = value; OnPropertyChanged(nameof(SelectedCallType)); RefreshClosedCalls(); }
        }

        private CallSortAndFilterField? _sortField;
        public CallSortAndFilterField? SortField
        {
            get => _sortField;
            set { _sortField = value; OnPropertyChanged(nameof(SortField)); RefreshClosedCalls(); }
        }

        // שלב 7 - DispatcherOperation ייעודי למתודת ההשקפה
        private volatile DispatcherOperation? _refreshClosedCallsOperation = null;

        public ClosedCallsWindow(int volunteerId)
        {
            InitializeComponent();
            DataContext = this;
            _volunteerId = volunteerId;
            RefreshClosedCalls();
        }

        private void RefreshClosedCalls()
        {
            if (_refreshClosedCallsOperation is null || _refreshClosedCallsOperation.Status == DispatcherOperationStatus.Completed)
            {
                _refreshClosedCallsOperation = Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        var result = _bl.Call.GetClosedCallsByVolunteer(_volunteerId, SelectedCallType, SortField);
                        ClosedCalls = new ObservableCollection<ClosedCallInList>(result);
                    }
                    catch (BlDoesNotExistException)
                    {
                        MessageBox.Show("לא נמצאו קריאות סגורות.", "אין נתונים", MessageBoxButton.OK, MessageBoxImage.Information);
                        Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("שגיאה בטעינת קריאות סגורות:\n" + ex.Message, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                        Close();
                    }
                });
            }
        }

        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
