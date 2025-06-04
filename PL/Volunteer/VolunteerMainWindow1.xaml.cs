using System.Windows;
using BO;
using PL.Volunteer.Admin;

namespace PL.Volunteer
{
    public partial class VolunteerMainWindow : Window
    {
        public BO.Volunteer CurrentVolunteer { get; set; }

        public VolunteerMainWindow(BO.Volunteer volunteer)
        {
            InitializeComponent();
            CurrentVolunteer = volunteer;
            DataContext = this;
        }

        private void UpdateVolunteer_Click(object sender, RoutedEventArgs e)
        {
            var window = new VolunteerWindow(CurrentVolunteer.Id); // assumes constructor supports update mode
            window.ShowDialog();
        }

        private void ViewClosedCalls_Click(object sender, RoutedEventArgs e)
        {
            //var window = new ClosedCallsWindow(CurrentVolunteer.Id);
            //window.ShowDialog();
        }

        private void ViewOpenCalls_Click(object sender, RoutedEventArgs e)
        {
            //var window = new OpenCallsWindow(CurrentVolunteer.Id);
            //window.ShowDialog();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
