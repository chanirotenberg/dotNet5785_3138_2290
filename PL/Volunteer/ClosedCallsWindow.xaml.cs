using System.Collections.ObjectModel;
using System.Windows;
using BlApi;
using BO;
using System.ComponentModel;

namespace PL.Volunteer
{
    public partial class ClosedCallsWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private ObservableCollection<ClosedCallInList> _closedCalls;
        public ObservableCollection<ClosedCallInList> ClosedCalls
        {
            get => _closedCalls;
            set
            {
                _closedCalls = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClosedCalls)));
            }
        }

        public ClosedCallsWindow(int volunteerId)
        {
            InitializeComponent();
            DataContext = this;

            try
            {
                var bl = Factory.Get();
                var calls = bl.Call.GetClosedCallsByVolunteer(volunteerId);
                ClosedCalls = new ObservableCollection<ClosedCallInList>(calls);
            }
            catch (BlDoesNotExistException)
            {
                MessageBox.Show("לא נמצאו קריאות שנסגרו למתנדב זה.",
                                "אין נתונים",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                this.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("אירעה שגיאה בטעינת קריאות סגורות:\n" + ex.Message,
                                "שגיאת מערכת",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                this.Close();
            }
        }
    }
}
