using CycleDesk.Data;
using CycleDesk.Views;
using CycleDesk.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;


namespace CycleDesk
{
    public partial class MainWindow : Window
    {
        private LoginControl loginControl;
        private RegisterStep1Control registerStep1Control;
        private RegisterStep2Control registerStep2Control;
        private UserService _userService;

        private string storedUsername = string.Empty;
        private string storedFirstName = string.Empty;
        private string storedLastName = string.Empty;
        private string storedAccountType = string.Empty;
        private string storedAccessCode = string.Empty;
        private bool isPasswordReset = false;

        public MainWindow()
        {
            InitializeComponent();

            _userService = new UserService();

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
            TestSimpleDatabaseConnection();
        }

        private async void TestSimpleDatabaseConnection()
        {
            string connectionString =
                "Server=localhost\\SQLEXPRESS;" +
                "Database=CycleDesk;" +
                "Trusted_Connection=True;" +
                "TrustServerCertificate=True;";

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // Pokaż toast sukcesu
                ShowToast("Połączono z bazą danych!", "", true);
            }
            catch (Exception)
            {
                // Pokaż toast błędu
                ShowToast("Nie połączono z bazą danych!", "Sprawdź połączenie z SQL Server", false);
            }
        }

        private async void ShowToast(string title, string message, bool isSuccess)
        {
            // Ustaw kolor tła i ikonę w zależności od sukcesu/błędu
            if (isSuccess)
            {
                ToastBackground.Color = (Color)ColorConverter.ConvertFromString("#22C55E"); // Zielony
                ToastIcon.Text = "✓";
            }
            else
            {
                ToastBackground.Color = (Color)ColorConverter.ConvertFromString("#EF4444"); // Czerwony
                ToastIcon.Text = "✗";
            }

            ToastTitle.Text = title;
            ToastMessage.Text = message;
            ToastMessage.Visibility = string.IsNullOrEmpty(message) ? Visibility.Collapsed : Visibility.Visible;

            // Animacja wejścia
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            var slideIn = new DoubleAnimation(50, 0, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            ToastNotification.BeginAnimation(OpacityProperty, fadeIn);
            ToastTranslate.BeginAnimation(TranslateTransform.XProperty, slideIn);

            // Czekaj 4 sekundy
            await Task.Delay(4000);

            // Animacja wyjścia
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            var slideOut = new DoubleAnimation(0, 50, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            ToastNotification.BeginAnimation(OpacityProperty, fadeOut);
            ToastTranslate.BeginAnimation(TranslateTransform.XProperty, slideOut);
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

        private async void LoginControl_LoginClicked(object? sender, EventArgs e)
        {
            string username = loginControl.Username;
            string password = loginControl.Password;

            using var context = new CycleDeskDbContext();
            
            // Najpierw znajdź użytkownika po username
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user != null)
            {
                // Weryfikacja hasła z BCrypt
                bool isPasswordValid = false;
                try
                {
                    // Sprawdź czy hash zaczyna się od $2 (BCrypt format)
                    if (user.PasswordHash.StartsWith("$2"))
                    {
                        isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                    }
                    else
                    {
                        // Fallback dla starych haseł (plain text) - do usunięcia po migracji
                        isPasswordValid = user.PasswordHash == password;
                    }
                }
                catch
                {
                    isPasswordValid = false;
                }

                if (isPasswordValid)
                {
                    // Aktualizuj LastLogin
                    user.LastLoginDate = DateTime.Now;
                    await context.SaveChangesAsync();
                    
                    MainDashboardWindow dashboard = new MainDashboardWindow(
                        username: user.Username,
                        password: password,
                        fullName: user.FullName,
                        role: user.Role
                    );
                    dashboard.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid username or password!", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Invalid username or password!", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoginControl_ForgotPasswordClicked(object? sender, EventArgs e)
        {
            // Przekieruj do ekranu wpisywania kodu (ten sam co dla nowych użytkowników)
            // Użytkownik wpisze kod resetu hasła otrzymany od managera
            registerStep1Control.SetForPasswordReset();
            SwitchView(registerStep1Control);
        }

        private void LoginControl_SignUpClicked(object? sender, EventArgs e)
        {
            registerStep1Control.SetForActivation();
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
                if (isPasswordReset)
                {
                    registerStep2Control.SetAccountDetailsForReset(storedUsername, storedFirstName, storedLastName);
                }
                else
                {
                    registerStep2Control.SetAccountDetails(storedUsername, storedFirstName, storedLastName, storedAccountType);
                }
                SwitchView(registerStep2Control);
            }
            else
            {
                MessageBox.Show("Invalid code!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool VerifyCodeInDatabase(string code)
        {
            // Sprawdź czy to kod resetu hasła
            var resetCode = _userService.ValidatePasswordResetCode(code);
            if (resetCode != null)
            {
                storedUsername = resetCode.Username;
                storedFirstName = resetCode.FirstName;
                storedLastName = resetCode.LastName;
                storedAccountType = resetCode.AccountType;
                storedAccessCode = code;
                isPasswordReset = true;
                return true;
            }

            // Sprawdź czy to kod dostępu dla nowego użytkownika
            var accessCode = _userService.ValidateAccessCode(code);
            if (accessCode != null)
            {
                storedUsername = accessCode.Username;
                storedFirstName = accessCode.FirstName;
                storedLastName = accessCode.LastName;
                storedAccountType = accessCode.AccountType;
                storedAccessCode = code;
                isPasswordReset = false;
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
                MessageBox.Show("Password must be at least 8 characters and contain uppercase, lowercase, number, and special character.", 
                    "Password Too Weak", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (isPasswordReset)
                {
                    // Reset hasła
                    bool success = _userService.ResetPasswordWithCode(storedAccessCode, password);
                    
                    if (success)
                    {
                        MessageBox.Show($"Password has been reset successfully!\n\nUsername: {storedUsername}\n\nYou can now log in with your new password.", 
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        registerStep2Control.ClearPasswords();
                        registerStep1Control.ClearCode();
                        loginControl.SetUsername(storedUsername);
                        isPasswordReset = false;

                        SwitchView(loginControl);
                    }
                    else
                    {
                        MessageBox.Show("Error resetting password. The reset code may have been already used.", 
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // Aktywuj konto - tworzy użytkownika w bazie i oznacza kod jako użyty
                    var newUser = _userService.ActivateUserAccount(storedAccessCode, password);
                    
                    if (newUser != null)
                    {
                        MessageBox.Show($"Account activated successfully!\n\nUsername: {newUser.Username}\nRole: {newUser.Role}\n\nYou can now log in.", 
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        registerStep2Control.ClearPasswords();
                        registerStep1Control.ClearCode();
                        loginControl.SetUsername(newUser.Username);

                        SwitchView(loginControl);
                    }
                    else
                    {
                        MessageBox.Show("Error activating account. The access code may have been already used.", 
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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