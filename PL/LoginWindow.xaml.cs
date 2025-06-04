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

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int id = int.Parse(VolunteerId);
                string password = Password;

                Jobs role = _bl.Volunteer.Login(id, password);

                this.Hide();

                if (role == Jobs.Administrator)
                    new MainWindow().Show();
                else
                    new Volunteer.VolunteerMainWindow(_bl.Volunteer.GetVolunteerDetails(id)).Show();

                this.Close();
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid ID format. Please enter digits only.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (BlDoesNotExistException)
            {
                MessageBox.Show("Volunteer not found.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (BlInvalidValueException)
            {
                MessageBox.Show("Incorrect password.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login failed.\n\n" + ex.Message, "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
