using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows;
using BlApi;
using BO;

namespace PL.Admin
{
    public partial class AddOrUpdateCallWindow : Window, INotifyPropertyChanged
    {
        private static readonly IBl s_bl = Factory.Get();

        private Action? _callObserver; // מעקב לפי מזהה
        private Action? _listObserver; // מעקב אחר רשימת הקריאות (ל־Add בלבד)

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public Call CurrentCall { get; private set; }
        public List<CallType> CallTypes => new((CallType[])Enum.GetValues(typeof(CallType)));
        public bool IsNewCall { get; private set; }

        public bool CanEditAll =>
            IsNewCall || (CurrentCall.Status == CallStatus.Open || CurrentCall.Status == CallStatus.OpenInRisk);

        public bool CanEditMaxOnly =>
            CanEditAll || (CurrentCall.Status == CallStatus.InTreatment || CurrentCall.Status == CallStatus.InRiskTreatment);

        public Visibility AssignmentsVisibility =>
            (CurrentCall?.Assignments?.Count ?? 0) > 0 ? Visibility.Visible : Visibility.Collapsed;

        public AddOrUpdateCallWindow(int? callId = null)
        {
            InitializeComponent();

            if (callId == null)
            {
                IsNewCall = true;
                CurrentCall = new Call
                {
                    OpeningTime = DateTime.Now,
                    Assignments = new List<CallAssignInList>()
                };
            }
            else
            {
                IsNewCall = false;
                try
                {
                    CurrentCall = s_bl.Call.GetCallDetails(callId.Value);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"שגיאה בטעינת קריאה: {ex.Message}");
                    this.Close();
                    return;
                }
            }

            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsNewCall)
            {
                _callObserver = RefreshCall;
                s_bl.Call.AddObserver(CurrentCall.Id, _callObserver);
            }
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            if (!IsNewCall && _callObserver != null)
                s_bl.Call.RemoveObserver(CurrentCall.Id, _callObserver);
        }


        // שדה חדש מחוץ למתודות, בתוך המחלקה
        private volatile DispatcherOperation? _refreshCallOperation = null;

        // מתודת ההשקפה המעודכנת
        private void RefreshCall()
        {
            if (_refreshCallOperation is null || _refreshCallOperation.Status == System.Windows.Threading.DispatcherOperationStatus.Completed)
                _refreshCallOperation = Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        CurrentCall = s_bl.Call.GetCallDetails(CurrentCall.Id);
                        OnPropertyChanged(nameof(CurrentCall));
                        OnPropertyChanged(nameof(CanEditAll));
                        OnPropertyChanged(nameof(CanEditMaxOnly));
                        OnPropertyChanged(nameof(AssignmentsVisibility));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"שגיאה ברענון הקריאה: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
        }


        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsNewCall)
                    s_bl.Call.AddCall(CurrentCall);
                else
                    s_bl.Call.UpdateCall(CurrentCall);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בשמירה: {ex.Message}");
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
