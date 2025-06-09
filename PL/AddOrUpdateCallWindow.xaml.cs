using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using BlApi;
using BO;

namespace PL
{
    public partial class AddOrUpdateCallWindow : Window
    {
        private readonly IBl _bl = Factory.Get();
        public Call CurrentCall { get; private set; }
        public List<CallType> CallTypes => new List<CallType>((CallType[])Enum.GetValues(typeof(CallType)));
        public bool IsNewCall { get; private set; }
        public bool CanEditAll => IsNewCall || (CurrentCall.Status == CallStatus.Open || CurrentCall.Status == CallStatus.OpenInRisk);
        public bool CanEditMaxOnly => CanEditAll || (CurrentCall.Status == CallStatus.InTreatment || CurrentCall.Status == CallStatus.InRiskTreatment);
        public Visibility AssignmentsVisibility => (CurrentCall?.Assignments?.Count ?? 0) > 0 ? Visibility.Visible : Visibility.Collapsed;

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
                    CurrentCall = _bl.Call.GetCallDetails(callId.Value);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"שגיאה בטעינת קריאה: {ex.Message}");
                    this.Close();
                    return;
                }
            }

            this.DataContext = this;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsNewCall)
                {
                    _bl.Call.AddCall(CurrentCall);
                    MessageBox.Show("הקריאה נוספה בהצלחה");
                }
                else
                {
                    _bl.Call.UpdateCall(CurrentCall);
                    MessageBox.Show("הקריאה עודכנה בהצלחה");
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בשמירה: {ex.Message}");
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
