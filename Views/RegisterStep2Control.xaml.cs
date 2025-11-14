using System;
using System.Windows;
using System.Windows.Controls;

namespace CycleDesk.Views
{
    public partial class RegisterStep2Control : UserControl
    {
        public event EventHandler? ActivateAccountClicked;
        public event EventHandler? BackToLoginClicked;

        public RegisterStep2Control()
        {
            InitializeComponent();
        }
        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            BackToLoginClicked?.Invoke(this, EventArgs.Empty);
        }

        public string Password => txtPassword.Password;
        public string ConfirmPassword => txtConfirmPassword.Password;

        public void SetAccountDetails(string username, string firstName, string lastName, string accountType)
        {
            lblUsername.Text = username;
            lblFirstName.Text = firstName;
            lblLastName.Text = lastName;
            lblAccountType.Text = accountType;
        }

        public void ClearPasswords()
        {
            txtPassword.Password = string.Empty;
            txtConfirmPassword.Password = string.Empty;
        }

        private void ActivateAccount_Click(object sender, RoutedEventArgs e)
        {
            ActivateAccountClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
