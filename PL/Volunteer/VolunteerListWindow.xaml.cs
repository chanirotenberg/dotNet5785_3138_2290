using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL.Volunteer
{
    public partial class VolunteerListWindow : Window
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        public BO.VolunteerInList? SelectedVolunteer { get; set; }

        public VolunteerListWindow()
        {
            InitializeComponent();
        }

        public IEnumerable<BO.VolunteerInList> VolunteerList
        {
            get => (IEnumerable<BO.VolunteerInList>)GetValue(VolunteerListProperty);
            set => SetValue(VolunteerListProperty, value);
        }

        public static readonly DependencyProperty VolunteerListProperty =
            DependencyProperty.Register("VolunteerList", typeof(IEnumerable<BO.VolunteerInList>), typeof(VolunteerListWindow), new PropertyMetadata(null));

        public BO.CallType calls { get; set; } = BO.CallType.None;

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

                    MessageBox.Show(message, "שגיאה בפתיחת חלון מתנדב", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

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

                MessageBox.Show(message, "שגיאה בטעינת רשימת מתנדבים", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshVolunteerList() => queryCallList();

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

                MessageBox.Show(message, "שגיאה בטעינת חלון", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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

                MessageBox.Show(message, "שגיאה בסגירת חלון", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CallTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            queryCallList();
        }

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

                MessageBox.Show(message, "שגיאה בהוספת מתנדב", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is BO.VolunteerInList volunteer)
            {
                var result = MessageBox.Show($"Are you sure you want to delete volunteer {volunteer.Name}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        s_bl.Volunteer.DeleteVolunteer(volunteer.Id);
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                        if (ex.InnerException != null)
                            message += "\n\nDetails: " + ex.InnerException.Message;

                        MessageBox.Show(message, "Delete Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
