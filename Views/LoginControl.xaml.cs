using System;
using System.Windows;
using System.Windows.Controls;

namespace CycleDesk.Views
{
    public partial class LoginControl : UserControl
    {
        // Events dla MainWindow
        public event EventHandler? LoginClicked;
        public event EventHandler? SignUpClicked;
        public event EventHandler? ForgotPasswordClicked;

        public LoginControl()
        {
            InitializeComponent();
        }

        // Publiczne metody dostępu do danych
        public string Username => txtUsername.Text;
        public string Password => txtPassword.Password;

        public void SetUsername(string username)
        {
            txtUsername.Text = username;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginClicked?.Invoke(this, EventArgs.Empty);
        }

        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            SignUpClicked?.Invoke(this, EventArgs.Empty);
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            ForgotPasswordClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
