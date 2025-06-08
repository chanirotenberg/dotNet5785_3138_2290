using System.Collections.ObjectModel;
using System.Windows;
using BlApi;
using BO;

namespace PL.Volunteer
{
    public partial class ClosedCallsWindow : Window
    {
        public ObservableCollection<ClosedCallInList> ClosedCalls { get; set; }

        public ClosedCallsWindow(int volunteerId)
        {
            InitializeComponent();
            var bl = Factory.Get();
            var calls = bl.Call.GetClosedCallsByVolunteer(volunteerId);
            ClosedCalls = new ObservableCollection<ClosedCallInList>(calls);
            DataContext = this;
        }
    }
}
