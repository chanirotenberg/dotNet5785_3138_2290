using System;
using System.ComponentModel;
using System.Windows;
using BlApi;
using BO;


namespace PL
{
    public partial class LoginWindow : Window, INotifyPropertyChanged
    {
        private readonly IBl _bl = Factory.Get();

        private string _volunteerId;
        public string VolunteerId
        {
            get => _volunteerId;
            set { _volunteerId = value; OnPropertyChanged(nameof(VolunteerId)); }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }

        private bool _isPasswordHidden = true;
        public bool IsPasswordHidden
        {
            get => _isPasswordHidden;
            set { _isPasswordHidden = value; OnPropertyChanged(nameof(IsPasswordHidden)); }
        }

        private static bool _isAdminLoggedIn = false;

        public LoginWindow()
        {
            InitializeComponent();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int id = int.Parse(VolunteerId);
                string password = Password;

                Jobs role = _bl.Volunteer.Login(id, password);

                if (role == Jobs.Administrator)
                {
                    if (_isAdminLoggedIn)
                    {
                        MessageBox.Show("מנהל כבר מחובר כרגע.", "שגיאת התחברות", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _isAdminLoggedIn = true;
                    var adminWindow = new Admin.MainWindow(_bl.Volunteer.GetVolunteerDetails(id));
                    adminWindow.Closed += (_, _) => _isAdminLoggedIn = false;
                    adminWindow.Show();
                }
                else
                {
                    var volunteerWindow = new Volunteer.VolunteerMainWindow(_bl.Volunteer.GetVolunteerDetails(id));
                    volunteerWindow.Show();
                }

                // נקה שדות לאחר התחברות מוצלחת
                VolunteerId = string.Empty;
                Password = string.Empty;
            }
            catch (FormatException)
            {
                MessageBox.Show("מספר מזהה שגוי. יש להזין ספרות בלבד.", "שגיאת התחברות", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (BlDoesNotExistException)
            {
                MessageBox.Show("המתנדב לא נמצא.", "שגיאת התחברות", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (BlInvalidValueException)
            {
                MessageBox.Show("סיסמה שגויה.", "שגיאת התחברות", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("התחברות נכשלה.\n\n" + ex.Message, "שגיאת התחברות", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
