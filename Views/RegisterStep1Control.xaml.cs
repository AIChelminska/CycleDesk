using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CycleDesk.Views
{
    public partial class RegisterStep1Control : UserControl
    {
        public event EventHandler? VerifyCodeClicked;
        public event EventHandler? BackToLoginClicked;

        public RegisterStep1Control()
        {
            InitializeComponent();
        }

        public string AccessCode => txtAccessCode.Text.Trim();

        public void ClearCode()
        {
            txtAccessCode.Text = string.Empty;
        }

        private void NumbersOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void VerifyCode_Click(object sender, RoutedEventArgs e)
        {
            VerifyCodeClicked?.Invoke(this, EventArgs.Empty);
        }

        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            BackToLoginClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
