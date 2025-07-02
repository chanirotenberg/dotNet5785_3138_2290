using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using BO;
using BlApi;

namespace PL.Admin
{
    public partial class CallManagementWindow : Window, INotifyPropertyChanged
    {
        private static readonly IBl _bl = Factory.Get();
        private readonly int _currentManagerId;

        public ObservableCollection<CallInList> Calls { get; set; } = new();
        public IEnumerable<CallStatus> Statuses => Enum.GetValues(typeof(CallStatus)).Cast<CallStatus>();
        public IEnumerable<CallType> CallTypes => Enum.GetValues(typeof(CallType)).Cast<CallType>();

        private CallStatus? _selectedStatus;
        public CallStatus? SelectedStatus
        {
            get => _selectedStatus;
            set { _selectedStatus = value; OnPropertyChanged(nameof(SelectedStatus)); }
        }

        private CallType? _selectedType;
        public CallType? SelectedType
        {
            get => _selectedType;
            set { _selectedType = value; OnPropertyChanged(nameof(SelectedType)); }
        }

        private volatile DispatcherOperation? _refreshCallsOperation = null;

        public CallManagementWindow(int managerId)
        {
            InitializeComponent();
            _currentManagerId = managerId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCalls();
            _bl.Call.AddObserver(RefreshCalls);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _bl.Call.RemoveObserver(RefreshCalls);
        }

        private void LoadCalls()
        {
            Calls.Clear();
            var list = _bl.Call.GetCallList(
                filterField: SelectedStatus != null ? CallSortAndFilterField.Status : null,
                filterValue: SelectedStatus,
                sortField: null
            );

            foreach (var call in list)
                Calls.Add(call);
        }

        private void RefreshCalls()
        {
            if (_refreshCallsOperation is null || _refreshCallsOperation.Status == DispatcherOperationStatus.Completed)
            {
                _refreshCallsOperation = Dispatcher.BeginInvoke(() =>
                {
                    LoadCalls();
                });
            }
        }
        private void Filter_Click(object sender, RoutedEventArgs e) => LoadCalls();

        private void CancelFilter_Click(object sender, RoutedEventArgs e)
        {
            SelectedStatus = null;
            SelectedType = null;
            LoadCalls();
        }

        private void AddCall_Click(object sender, RoutedEventArgs e)
        {
            var win = new AddOrUpdateCallWindow();
            win.Show();
            LoadCalls();
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if ((e.OriginalSource as FrameworkElement)?.DataContext is CallInList selectedCall)
            {
                try
                {
                    var updateWindow = new AddOrUpdateCallWindow(selectedCall.CallId);
                    updateWindow.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("אירעה שגיאה בעת פתיחת מסך עדכון קריאה.\n\n" + ex.Message,
                        "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteCall_Click(object sender, RoutedEventArgs e)
        {
            if ((e.Source as FrameworkElement)?.DataContext is CallInList call)
            {
                try
                {
                    _bl.Call.DeleteCall(call.CallId);
                    MessageBox.Show("הקריאה נמחקה בהצלחה.", "מחיקה", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadCalls();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("לא ניתן למחוק את הקריאה.\n\n" + ex.Message,
                                    "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelAssignment_Click(object sender, RoutedEventArgs e)
        {
            if ((e.Source as FrameworkElement)?.DataContext is CallInList call)
            {
                try
                {
                    var callDetails = _bl.Call.GetCallDetails(call.CallId);
                    var activeAssignment = callDetails.Assignments.FirstOrDefault(a => a.EndTime == null);

                    if (activeAssignment == null)
                    {
                        MessageBox.Show("לא נמצאה הקצאה פעילה לביטול.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var volunteerId = activeAssignment.VolunteerId;
                    var assignmentId = _bl.Volunteer.GetVolunteerDetails((int)volunteerId).CallInProgress.Id;

                    _bl.Call.CancelCall(_currentManagerId, assignmentId);
                    RefreshCalls();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("אירעה שגיאה בעת ביטול ההקצאה.\n\n" + ex.Message,
                                    "שגיאת ביטול", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
