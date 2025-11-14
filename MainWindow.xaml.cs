using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CycleDesk.Views;

namespace CycleDesk
{
    public partial class MainWindow : Window
    {
        private LoginControl loginControl;
        private RegisterStep1Control registerStep1Control;
        private RegisterStep2Control registerStep2Control;

        private string storedUsername = string.Empty;
        private string storedFirstName = string.Empty;
        private string storedLastName = string.Empty;
        private string storedAccountType = string.Empty;

        public MainWindow()
        {
            InitializeComponent();

            loginControl = new LoginControl();
            registerStep1Control = new RegisterStep1Control();
            registerStep2Control = new RegisterStep2Control();

            loginControl.LoginClicked += LoginControl_LoginClicked;
            loginControl.SignUpClicked += LoginControl_SignUpClicked;
            loginControl.ForgotPasswordClicked += LoginControl_ForgotPasswordClicked;

            registerStep1Control.VerifyCodeClicked += RegisterStep1_VerifyCodeClicked;
            registerStep1Control.BackToLoginClicked += BackToLogin;

            registerStep2Control.ActivateAccountClicked += RegisterStep2_ActivateAccountClicked;
            registerStep2Control.BackToLoginClicked += BackToLogin;

            StartSplashScreen();
        }

        private async void StartSplashScreen()
        {
            await Task.Delay(2000);
            ContentContainer.Content = loginControl;
            Storyboard storyboard = (Storyboard)this.Resources["ShrinkLogoAnimation"];
            storyboard.Begin();
        }

        
        private async void SwitchView(object newView)
        {
            
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            ContentContainer.BeginAnimation(OpacityProperty, fadeOut);
            await Task.Delay(500);

          
            ContentContainer.Content = newView;

            
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            ContentContainer.BeginAnimation(OpacityProperty, fadeIn);
        }

        private void LoginControl_LoginClicked(object? sender, EventArgs e)
        {
            string username = loginControl.Username;
            string password = loginControl.Password;

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Podaj nazwę użytkownika!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Podaj hasło!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: Sprawdź w bazie danych
            // Na razie MOCK DATA
            if (username == "admin" && password == "admin123")
            {
                // Otwórz Dashboard
                MainDashboardWindow dashboard = new MainDashboardWindow(
                    username: "admin",
                    password: "admin123",
                    fullName: "Admin User",
                    role: "Supervisor"
                );
                dashboard.Show();

                // Zamknij okno logowania
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid username or password!", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoginControl_ForgotPasswordClicked(object? sender, EventArgs e)
        {
            MessageBox.Show("Forgot Password!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoginControl_SignUpClicked(object? sender, EventArgs e)
        {
            SwitchView(registerStep1Control);
        }

        private void RegisterStep1_VerifyCodeClicked(object? sender, EventArgs e)
        {
            string code = registerStep1Control.AccessCode;

            if (code.Length != 11)
            {
                MessageBox.Show("Access code must be 11 digits!", "Invalid", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (VerifyCodeInDatabase(code))
            {
                registerStep2Control.SetAccountDetails(storedUsername, storedFirstName, storedLastName, storedAccountType);
                SwitchView(registerStep2Control);
            }
            else
            {
                MessageBox.Show("Invalid code!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool VerifyCodeInDatabase(string code)
        {
            if (code == "12345678901")
            {
                storedUsername = "john_operator";
                storedFirstName = "John";
                storedLastName = "Smith";
                storedAccountType = "Operator";
                return true;
            }
            return false;
        }

        private void RegisterStep2_ActivateAccountClicked(object? sender, EventArgs e)
        {
            string password = registerStep2Control.Password;
            string confirm = registerStep2Control.ConfirmPassword;

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Enter password!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != confirm)
            {
                MessageBox.Show("Passwords don't match!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsPasswordStrong(password))
            {
                MessageBox.Show("Password too weak!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show($"Account activated!\n\nUsername: {storedUsername}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            registerStep2Control.ClearPasswords();
            registerStep1Control.ClearCode();
            loginControl.SetUsername(storedUsername);

            SwitchView(loginControl);
        }

        private bool IsPasswordStrong(string password)
        {
            if (password.Length < 8) return false;
            if (!Regex.IsMatch(password, @"[A-Z]")) return false;
            if (!Regex.IsMatch(password, @"[a-z]")) return false;
            if (!Regex.IsMatch(password, @"[0-9]")) return false;
            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?]")) return false;
            return true;
        }

        private void BackToLogin(object? sender, EventArgs e)
        {
            registerStep1Control.ClearCode();
            SwitchView(loginControl);
        }
    }
}