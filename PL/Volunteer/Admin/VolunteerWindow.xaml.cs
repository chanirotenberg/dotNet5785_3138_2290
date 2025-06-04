using System;
using System.Windows;
using System.Collections.Generic;
using BO;
using System.Windows.Controls;
using DO;
using System.Windows.Input;

namespace PL.Volunteer.Admin;

/// <summary>
/// Represents a window for adding or updating a single volunteer entity.
/// The window supports data binding via a dependency property and reflects changes in the BL through observer registration.
/// </summary>
public partial class VolunteerWindow : Window
{
    /// <summary>
    /// Static reference to the BL (Business Logic) instance.
    /// </summary>
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    /// <summary>
    /// Text for the action button ("Add" or "Update").
    /// </summary>
    public string ButtonText { get; set; }

    /// <summary>
    /// Observer delegate used for refreshing the volunteer when updated externally.
    /// </summary>
    private Action? volunteerObserver;

    /// <summary>
    /// Internal volunteer ID used for identifying the current volunteer.
    /// </summary>
    private int id;

    /// <summary>
    /// Initializes the window. If an ID is provided, loads an existing volunteer for update.
    /// Otherwise, prepares a new volunteer for creation.
    /// </summary>
    /// <param name="id">The ID of the volunteer to update, or 0 to add a new volunteer.</param>
    public VolunteerWindow(int id = 0)
    {
        ButtonText = id == 0 ? "Add" : "Update";
        CurrentVolunteer = (id != 0)
            ? s_bl.Volunteer.GetVolunteerDetails(id)!
            : new BO.Volunteer() { Id = 0 };

        InitializeComponent();
    }

    /// <summary>
    /// The current volunteer being edited (for both add and update).
    /// Bound to UI via Data Binding.
    /// </summary>
    public BO.Volunteer? CurrentVolunteer
    {
        get => (BO.Volunteer?)GetValue(CurrentVolunteerProperty);
        set => SetValue(CurrentVolunteerProperty, value);
    }

    /// <summary>
    /// Dependency property wrapper for CurrentVolunteer to support binding.
    /// </summary>
    public static readonly DependencyProperty CurrentVolunteerProperty =
        DependencyProperty.Register("CurrentVolunteer", typeof(BO.Volunteer), typeof(VolunteerWindow), new PropertyMetadata(null));

    /// <summary>
    /// Handles the Add or Update button click.
    /// Performs validation and sends the volunteer to the BL for saving.
    /// </summary>
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
                // Ensure password is retained if not re-entered
                if (string.IsNullOrWhiteSpace(CurrentVolunteer!.Password))
                {
                    var existing = s_bl.Volunteer.GetVolunteerDetails(CurrentVolunteer.Id);
                    CurrentVolunteer.Password = existing.Password;
                }

                s_bl.Volunteer.UpdateVolunteer(CurrentVolunteer!.Id, CurrentVolunteer);
                MessageBox.Show("המתנדב עודכן בהצלחה", "עדכון", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            this.Close();
        }
        catch (Exception ex)
        {
            string userMessage = "אירעה שגיאה בעת שמירת המתנדב. אנא בדוק את הפרטים ונסה שוב.";
            if (ex.InnerException != null)
                userMessage += "\n\nDetails: " + ex.InnerException.Message;

            MessageBox.Show($"{userMessage}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Reloads the current volunteer from the BL to ensure updated data is shown.
    /// </summary>
    private void RefreshVolunteer()
    {
        int id = CurrentVolunteer!.Id;
        CurrentVolunteer = null;
        CurrentVolunteer = s_bl.Volunteer.GetVolunteerDetails(id);
    }

    /// <summary>
    /// Handles logic when the window is loaded, including observer registration for updates.
    /// </summary>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        volunteerObserver = RefreshVolunteer;
        if (CurrentVolunteer!.Id != 0)
            s_bl.Volunteer.AddObserver(CurrentVolunteer.Id, volunteerObserver);
    }

    /// <summary>
    /// Handles logic when the window is closed, including observer deregistration.
    /// </summary>
    private void Window_Closed(object sender, EventArgs e)
    {
        if (CurrentVolunteer != null && volunteerObserver != null)
            s_bl.Volunteer.RemoveObserver(CurrentVolunteer.Id, volunteerObserver);
    }
}
