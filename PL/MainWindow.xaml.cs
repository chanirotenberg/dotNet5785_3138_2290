using PL.Volunteer;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        public MainWindow()
        {
            InitializeComponent();
        }
        public DateTime CurrentTime
        {
            get { return (DateTime)GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }
        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(MainWindow));

        private void btnAddOneMinute_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.AdvanceClock(BO.TimeUnit.Minute);
        }
        private void btnAddOneHour_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.AdvanceClock(BO.TimeUnit.Hour);
        }
        private void btnAddOneDay_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.AdvanceClock(BO.TimeUnit.Day);
        }
        private void btnAddOneMonth_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.AdvanceClock(BO.TimeUnit.Month);
        }
        private void btnAddOneYear_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.AdvanceClock(BO.TimeUnit.Year);
        }

        public TimeSpan RiskRange
        {
            get { return (TimeSpan)GetValue(RiskRangeProperty); }
            set { SetValue(RiskRangeProperty, value); }
        }

        public static readonly DependencyProperty RiskRangeProperty =
            DependencyProperty.Register("RiskRange", typeof(TimeSpan), typeof(MainWindow));
        private void UpdateRiskRange_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.SetRiskRange(RiskRange);
        }
        private void clockObserver()
        {
            CurrentTime = s_bl.Admin.GetClock();
        }
        private void configObserver()
        {
            RiskRange = s_bl.Admin.GetRiskRange();
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // השמה של השעון
            CurrentTime = s_bl.Admin.GetClock();

            // השמה של משתני התצורה
            RiskRange = s_bl.Admin.GetRiskRange();
            // אם יש עוד משתני תצורה - תעשה גם להם השמה כאן

            // רישום השקיפים
            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);
        }
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            // הסרת ההשקפה על השעון
            s_bl.Admin.RemoveClockObserver(clockObserver);

            // הסרת ההשקפה על משתני התצורה
            s_bl.Admin.RemoveConfigObserver(configObserver);
        }
        private void InitializeDB_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("האם אתה בטוח שברצונך לאתחל את בסיס הנתונים?", "אישור אתחול", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Mouse.OverrideCursor = Cursors.Wait; // שינוי סמן לשעון חול
                try
                {
                    // סגירת כל המסכים הפתוחים חוץ מהמסך הראשי
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window != this)
                            window.Close();
                    }

                    s_bl.Admin.InitializeDB(); // קריאה לאתחול
                }
                finally
                {
                    Mouse.OverrideCursor = null; // החזרת סמן רגיל
                }
            }
        }

        private void ResetDB_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("האם אתה בטוח שברצונך לאפס את בסיס הנתונים?", "אישור איפוס", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Mouse.OverrideCursor = Cursors.Wait; // שינוי סמן לשעון חול
                try
                {
                    // סגירת כל המסכים הפתוחים חוץ מהמסך הראשי
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window != this)
                            window.Close();
                    }

                    s_bl.Admin.ResetDB(); // קריאה לאיפוס
                }
                finally
                {
                    Mouse.OverrideCursor = null; // החזרת סמן רגיל
                }
            }
        }
        private void HandleVolunteer_Click(object sender, RoutedEventArgs e)
        {
            new VolunteerListWindow().Show(); // פתיחת מסך רשימת הקורסים בלי לחסום את המסך הראשי
        }
    }
}