using System;
using System.Windows;
using System.Collections.Generic;
using BO;
using System.Windows.Controls;
using DO;
using System.Windows.Input;

namespace PL.Volunteer;

/// <summary>
/// Interaction logic for VolunteerWindow.xaml
/// </summary>
public partial class VolunteerWindow : Window
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    public string ButtonText { get; set; }
    private Action? volunteerObserver;
    private int id;

    public VolunteerWindow(int id = 0)
    {
        ButtonText = id == 0 ? "Add" : "Update";
        CurrentVolunteer = (id != 0)
            ? s_bl.Volunteer.GetVolunteerDetails(id)!
            : new BO.Volunteer() { Id = 0 };

        InitializeComponent();
    }

    public BO.Volunteer? CurrentVolunteer
    {
        get => (BO.Volunteer?)GetValue(CurrentVolunteerProperty);
        set => SetValue(CurrentVolunteerProperty, value);
    }

    public static readonly DependencyProperty CurrentVolunteerProperty =
        DependencyProperty.Register("CurrentVolunteer", typeof(BO.Volunteer), typeof(VolunteerWindow), new PropertyMetadata(null));

    private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (ButtonText == "Add")
            {
                s_bl.Volunteer.CreateVolunteer(CurrentVolunteer!);
                MessageBox.Show("המתנדב נוסף בהצלחה", "הוספה", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else // Update
            {
                s_bl.Volunteer.UpdateVolunteer(CurrentVolunteer!.Id, CurrentVolunteer);
                MessageBox.Show("המתנדב עודכן בהצלחה", "עדכון", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            this.Close();
        }
        catch (Exception ex)
        {
            string message = ex.Message;
            if (ex.InnerException != null)
                message += "\n\nDetails: " + ex.InnerException.Message;

            MessageBox.Show(message, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RefreshVolunteer()
    {
        int id = CurrentVolunteer!.Id;
        CurrentVolunteer = null;
        CurrentVolunteer = s_bl.Volunteer.GetVolunteerDetails(id);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        volunteerObserver = RefreshVolunteer;
        if (CurrentVolunteer!.Id != 0)
            s_bl.Volunteer.AddObserver(CurrentVolunteer.Id, volunteerObserver);
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        if (CurrentVolunteer != null && volunteerObserver != null)
            s_bl.Volunteer.RemoveObserver(CurrentVolunteer.Id, volunteerObserver);
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox && DataContext is VolunteerWindow window)
        {
            window.CurrentVolunteer.Password = passwordBox.Password;
        }

    }

}
