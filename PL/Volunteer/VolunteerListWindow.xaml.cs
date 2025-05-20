using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PL.Volunteer
{

    /// <summary>
    /// Interaction logic for VolunteerListWindow.xaml
    /// </summary>
    public partial class VolunteerListWindow : Window
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        public VolunteerListWindow()
        {
            InitializeComponent();
        }
        public IEnumerable<BO.VolunteerInList> CallList
        {
            get { return (IEnumerable<BO.VolunteerInList>)GetValue(VolunteerListProperty); }
            set { SetValue(VolunteerListProperty, value); }
        }

        public static readonly DependencyProperty VolunteerListProperty =
            DependencyProperty.Register("VolunteerList", typeof(IEnumerable<BO.VolunteerInList>), typeof(VolunteerListWindow), new PropertyMetadata(null));

        public BO.CallType calls { get; set; } = BO.CallType.None;

        private void CallTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CallList = (calls == BO.CallType.None) ?
                s_bl?.Volunteer.GetVolunteersFilterList(null)! :
                s_bl?.Volunteer.GetVolunteersFilterList(calls)!;
        }

        private void queryCallList()
    => CallList = (calls == BO.CallType.None) ?
        s_bl?.Volunteer.GetVolunteersFilterList(null)! : s_bl?.Volunteer.GetVolunteersFilterList(calls)!;

        private void callListObserver()
            => queryCallList();
 
private void Window_Loaded(object sender, RoutedEventArgs e)
    => s_bl.Volunteer.AddObserver(callListObserver);

        private void Window_Closed(object sender, EventArgs e)
            => s_bl.Volunteer.RemoveObserver(callListObserver);

    }

}

