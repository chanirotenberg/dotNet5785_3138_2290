using System;
using System.Windows;
using BlApi;
using BO;

namespace PL
{
    public partial class LoginWindow : Window
    {
        private readonly IBl _bl = Factory.Get();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text;
            string password = PasswordBox.Password;

            try
            {
                Jobs role = _bl.Volunteer.Login(username, password);

                this.Hide();

                if (role == Jobs.Administrator)
                {
                    new MainWindow().Show();
                }
                else
                {
                    //new VolunteerMainWindow(username).Show(); // אפשר גם להעביר ID אם תרצה
                }

                this.Close();
            }
            catch (BlDoesNotExistException)
            {
                MessageBox.Show("Username not found.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
    }
}
