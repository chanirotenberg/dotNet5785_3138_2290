using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading; // שימי לב שיש להוסיף using זה
using BO;

namespace PL.Admin
{
    public partial class VolunteerWindow : Window, INotifyPropertyChanged
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public string ButtonText { get; set; }

        private Action? volunteerObserver;

        private BO.Volunteer? _currentVolunteer;
        public BO.Volunteer? CurrentVolunteer
        {
            get => _currentVolunteer;
            set
            {
                _currentVolunteer = value;
                OnPropertyChanged(nameof(CurrentVolunteer));
            }
        }

        // שדה DispatcherOperation לשמירת מצב הקריאה הנוכחית ל-Dispatcher
        private volatile DispatcherOperation? _refreshVolunteerOperation = null;

        public VolunteerWindow(int id = 0)
        {
            ButtonText = id == 0 ? "Add" : "Update";
            CurrentVolunteer = (id != 0)
                ? s_bl.Volunteer.GetVolunteerDetails(id)!
                : new BO.Volunteer() { Id = 0 };

            InitializeComponent();
            DataContext = this;
        }

        private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ButtonText == "Add")
                {
                    s_bl.Volunteer.CreateVolunteer(CurrentVolunteer!);
                    MessageBox.Show("המתנדב נוסף בהצלחה", "הוספה", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(CurrentVolunteer!.Password))
                    {
                        var existing = s_bl.Volunteer.GetVolunteerDetails(CurrentVolunteer.Id);
                        CurrentVolunteer.Password = existing.Password;
                    }

                    s_bl.Volunteer.UpdateVolunteer(CurrentVolunteer.Id, CurrentVolunteer);
                    MessageBox.Show("המתנדב עודכן בהצלחה", "עדכון", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                this.Close();
            }
            catch (Exception ex)
            {
                string userMessage = "אירעה שגיאה בעת שמירת המתנדב. ";
                if (ex.InnerException != null)
                    userMessage += "\n\nDetails: " + ex.InnerException.Message;
                //else
                //    userMessage += "\n\nDetails: " + ex.Message;
                MessageBox.Show(userMessage, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // מתודת ההשקפה עם השימוש ב-DispatcherOperation לפי ההוראות
        private void RefreshVolunteer()
        {
            if (_refreshVolunteerOperation == null || _refreshVolunteerOperation.Status == DispatcherOperationStatus.Completed)
            {
                _refreshVolunteerOperation = Dispatcher.BeginInvoke(() =>
                {
                    if (CurrentVolunteer != null)
                    {
                        var id = CurrentVolunteer.Id;
                        CurrentVolunteer = null;
                        CurrentVolunteer = s_bl.Volunteer.GetVolunteerDetails(id);
                    }
                });
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            volunteerObserver = RefreshVolunteer;
            if (CurrentVolunteer != null && CurrentVolunteer.Id != 0)
                s_bl.Volunteer.AddObserver(CurrentVolunteer.Id, volunteerObserver);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (CurrentVolunteer != null && volunteerObserver != null)
                s_bl.Volunteer.RemoveObserver(CurrentVolunteer.Id, volunteerObserver);
        }
    }
}
