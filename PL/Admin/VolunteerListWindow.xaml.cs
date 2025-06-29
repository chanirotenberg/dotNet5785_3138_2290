using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace PL.Admin
{
    /// <summary>
    /// Interaction logic for VolunteerListWindow.xaml
    /// This window displays a list of volunteers, allows filtering by call type,
    /// and supports basic operations like viewing, adding, and deleting volunteers.
    /// </summary>
    public partial class VolunteerListWindow : Window
    {
        /// <summary>
        /// Static reference to the BL (Business Logic) layer through a factory.
        /// </summary>
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        /// <summary>
        /// The volunteer currently selected in the UI.
        /// </summary>
        public BO.VolunteerInList? SelectedVolunteer { get; set; }

        /// <summary>
        /// Initializes the VolunteerListWindow and its components.
        /// </summary>
        public VolunteerListWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// A bindable list of volunteers to be shown in the ListView.
        /// </summary>
        public IEnumerable<BO.VolunteerInList> VolunteerList
        {
            get => (IEnumerable<BO.VolunteerInList>)GetValue(VolunteerListProperty);
            set => SetValue(VolunteerListProperty, value);
        }

        /// <summary>
        /// Dependency property for VolunteerList, allowing UI binding.
        /// </summary>
        public static readonly DependencyProperty VolunteerListProperty =
            DependencyProperty.Register("VolunteerList", typeof(IEnumerable<BO.VolunteerInList>), typeof(VolunteerListWindow), new PropertyMetadata(null));

        /// <summary>
        /// Stores the selected call type filter (None by default).
        /// </summary>
        public BO.CallType calls { get; set; } = BO.CallType.None;

        // שדה DispatcherOperation ייעודי למתודת ההשקפה RefreshVolunteerList
        private volatile DispatcherOperation? _refreshVolunteerListOperation = null;

        /// <summary>
        /// Handles double-clicking a volunteer in the ListView to open their detailed window.
        /// </summary>
        private void lsvVolunteerList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedVolunteer is not null)
            {
                try
                {
                    new VolunteerWindow(SelectedVolunteer.Id).Show();
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                    if (ex.InnerException != null)
                        message += "\n\nDetails: " + ex.InnerException.Message;

                    MessageBox.Show(message, "Error opening volunteer window", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Queries the volunteer list from the BL layer according to the selected call type.
        /// </summary>
        private void queryCallList()
        {
            try
            {
                VolunteerList = (calls == BO.CallType.None) ?
                    s_bl?.Volunteer.GetVolunteersFilterList(null)! :
                    s_bl?.Volunteer.GetVolunteersFilterList(calls)!;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (ex.InnerException != null)
                    message += "\n\nDetails: " + ex.InnerException.Message;

                MessageBox.Show(message, "Error loading volunteer list", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Refreshes the current list of volunteers by re-querying it asynchronously on the Dispatcher.
        /// </summary>
        private void RefreshVolunteerList()
        {
            if (_refreshVolunteerListOperation is null || _refreshVolunteerListOperation.Status == DispatcherOperationStatus.Completed)
            {
                _refreshVolunteerListOperation = Dispatcher.BeginInvoke(() =>
                {
                    queryCallList();
                });
            }
        }

        /// <summary>
        /// Handles logic to be executed when the window loads.
        /// Adds observer for changes and populates the volunteer list.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                s_bl.Volunteer.AddObserver(RefreshVolunteerList);
                queryCallList();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (ex.InnerException != null)
                    message += "\n\nDetails: " + ex.InnerException.Message;

                MessageBox.Show(message, "Error loading window", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Unsubscribes from observers when the window is closed.
        /// </summary>
        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                s_bl.Volunteer.RemoveObserver(RefreshVolunteerList);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (ex.InnerException != null)
                    message += "\n\nDetails: " + ex.InnerException.Message;

                MessageBox.Show(message, "Error closing window", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles selection change in the call type combo box and re-queries the list.
        /// </summary>
        private void CallTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            queryCallList();
        }

        /// <summary>
        /// Opens the window to add a new volunteer.
        /// </summary>
        private void AddVolunteer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addWindow = new VolunteerWindow();
                addWindow.Show();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (ex.InnerException != null)
                    message += "\n\nDetails: " + ex.InnerException.Message;

                MessageBox.Show(message, "Error adding volunteer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles deletion of a volunteer after user confirmation.
        /// </summary>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if ((e.Source as Button)?.CommandParameter is BO.VolunteerInList volunteer)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete volunteer {volunteer.Name}?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        s_bl.Volunteer.DeleteVolunteer(volunteer.Id);
                        MessageBox.Show("Volunteer deleted successfully.", "Delete", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (BO.BlDoesNotExistException)
                    {
                        MessageBox.Show("The volunteer no longer exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                        if (ex.InnerException != null)
                            message += "\n\nDetails: " + ex.InnerException.Message;

                        MessageBox.Show(message, "Error Deleting volunteer", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
