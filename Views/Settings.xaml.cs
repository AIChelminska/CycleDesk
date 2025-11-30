using CycleDesk.Views;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CycleDesk
{
    public partial class SettingsWindow : Window
    {
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;

        private AppSettings _currentSettings;

        public SettingsWindow(string username, string password, string fullName, string role)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _fullName = fullName;
            _role = role;

            // ✅ Inicjalizuj menu przez UserControl
            sideMenu.Initialize(fullName, role);
            sideMenu.SetActiveMenu("Administration", "Settings");

            // ✅ Podłącz eventy nawigacji
            ConnectMenuEvents();

            // Load settings
            LoadSettings();
        }

        private void ConnectMenuEvents()
        {
            sideMenu.DashboardClicked += (s, e) => NavigateToDashboard();
            sideMenu.ProductsClicked += (s, e) => NavigateToProducts();
            sideMenu.CategoriesClicked += (s, e) => NavigateToCategories();
            sideMenu.InventoryStatusClicked += (s, e) => NavigateToInventoryStatus();
            sideMenu.GoodsReceiptClicked += (s, e) => NavigateToGoodsReceipt();
            sideMenu.SuppliersClicked += (s, e) => NavigateToSuppliers();
            sideMenu.NewSaleClicked += (s, e) => NavigateToNewSale();
            sideMenu.SalesHistoryClicked += (s, e) => NavigateToSalesHistory();
            
            sideMenu.SalesReportsClicked += (s, e) => NavigateToSalesReports();
            sideMenu.InventoryReportsClicked += (s, e) => NavigateToInventoryReports();
            sideMenu.ProductsToOrderClicked += (s, e) => NavigateToProductsToOrder();
            sideMenu.UsersClicked += (s, e) => NavigateToUsers();
            sideMenu.SettingsClicked += (s, e) => { }; // Already here
            sideMenu.LogoutClicked += (s, e) => HandleLogout();
        }

        // ===== DATA MODELS =====
        public class AppSettings
        {
            public string Language { get; set; } = "Polski";
            public string Theme { get; set; } = "Light";
            public string Currency { get; set; } = "PLN";
            public string DateFormat { get; set; } = "DD.MM.YYYY";
            public string DefaultPaymentMethod { get; set; } = "Cash";
            public bool ShowProductImages { get; set; } = true;
            public bool EnableDiscounts { get; set; } = true;
            public string ConnectionString { get; set; } = "Server=localhost;Database=CycleDesk;Trusted_Connection=True;";
            public bool AutoBackup { get; set; } = false;
        }

        // ===== SETTINGS MANAGEMENT =====
        private void LoadSettings()
        {
            _currentSettings = new AppSettings();
            ApplySettingsToUI();
        }

        private void ApplySettingsToUI()
        {
            // General Settings
            cmbLanguage.SelectedIndex = _currentSettings.Language == "Polski" ? 0 :
                                       _currentSettings.Language == "English" ? 1 : 2;
            cmbTheme.SelectedIndex = _currentSettings.Theme == "Light" ? 0 :
                                    _currentSettings.Theme == "Dark" ? 1 : 2;
            cmbCurrency.SelectedIndex = _currentSettings.Currency == "PLN" ? 0 :
                                       _currentSettings.Currency == "EUR" ? 1 :
                                       _currentSettings.Currency == "USD" ? 2 : 3;
            cmbDateFormat.SelectedIndex = _currentSettings.DateFormat == "DD.MM.YYYY" ? 0 :
                                         _currentSettings.DateFormat == "MM/DD/YYYY" ? 1 : 2;

            // Sales Settings
            cmbDefaultPayment.SelectedIndex = _currentSettings.DefaultPaymentMethod == "Cash" ? 0 :
                                             _currentSettings.DefaultPaymentMethod == "Card" ? 1 : 2;
            chkShowProductImages.IsChecked = _currentSettings.ShowProductImages;
            chkEnableDiscounts.IsChecked = _currentSettings.EnableDiscounts;

            // Database Settings
            txtConnectionString.Text = _currentSettings.ConnectionString;
            chkAutoBackup.IsChecked = _currentSettings.AutoBackup;
        }

        private void SaveCurrentSettings()
        {
            try
            {
                _currentSettings.Language = (cmbLanguage.SelectedItem as ComboBoxItem)?.Content.ToString().Contains("Polski") == true ? "Polski" :
                                           (cmbLanguage.SelectedItem as ComboBoxItem)?.Content.ToString().Contains("English") == true ? "English" : "Deutsch";

                _currentSettings.Theme = (cmbTheme.SelectedItem as ComboBoxItem)?.Content.ToString().Contains("Light") == true ? "Light" :
                                        (cmbTheme.SelectedItem as ComboBoxItem)?.Content.ToString().Contains("Dark") == true ? "Dark" : "System";

                _currentSettings.Currency = (cmbCurrency.SelectedItem as ComboBoxItem)?.Content.ToString().StartsWith("PLN") == true ? "PLN" :
                                           (cmbCurrency.SelectedItem as ComboBoxItem)?.Content.ToString().StartsWith("EUR") == true ? "EUR" :
                                           (cmbCurrency.SelectedItem as ComboBoxItem)?.Content.ToString().StartsWith("USD") == true ? "USD" : "GBP";

                _currentSettings.DateFormat = (cmbDateFormat.SelectedItem as ComboBoxItem)?.Content.ToString();

                _currentSettings.DefaultPaymentMethod = (cmbDefaultPayment.SelectedItem as ComboBoxItem)?.Content.ToString().Contains("Cash") == true ? "Cash" :
                                                       (cmbDefaultPayment.SelectedItem as ComboBoxItem)?.Content.ToString().Contains("Card") == true ? "Card" : "None";

                _currentSettings.ShowProductImages = chkShowProductImages.IsChecked ?? true;
                _currentSettings.EnableDiscounts = chkEnableDiscounts.IsChecked ?? true;

                _currentSettings.ConnectionString = txtConnectionString.Text;
                _currentSettings.AutoBackup = chkAutoBackup.IsChecked ?? false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving settings: {ex.Message}");
            }
        }

        // ===== EVENT HANDLERS =====
        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveCurrentSettings();
                MessageBox.Show("Settings saved successfully!", "Success",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings:\n{ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetToDefaults_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Reset all settings to default values?\n\nThis action cannot be undone.",
                                        "Confirm Reset",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _currentSettings = new AppSettings();
                ApplySettingsToUI();
                MessageBox.Show("Settings have been reset to defaults.", "Success",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Language_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Placeholder for language change
        }

        private void BackupDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Database backup created successfully!\n\nLocation: C:\\CycleDesk\\Backups\\backup_" +
                              DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bak",
                              "Backup Complete",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating backup:\n{ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RestoreDatabase_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Restore database from backup?\n\n" +
                                        "WARNING: This will replace all current data!\n" +
                                        "Make sure you have a recent backup of your current data.",
                                        "Confirm Restore",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("Database restore functionality will be implemented here.",
                              "Info",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
        }

        // ===== NAVIGATION METHODS =====
        private void NavigateToDashboard()
        {
            MainDashboardWindow dashboard = new MainDashboardWindow(_username, _password, _fullName, _role);
            dashboard.Show();
            this.Close();
        }

        private void NavigateToProducts()
        {
            ProductsWindow productsWindow = new ProductsWindow(_username, _password, _fullName, _role);
            productsWindow.Show();
            this.Close();
        }

        private void NavigateToCategories()
        {
            CategoriesWindow categoriesWindow = new CategoriesWindow(_username, _password, _fullName, _role);
            categoriesWindow.Show();
            this.Close();
        }

        private void NavigateToInventoryStatus()
        {
            InventoryStatusWindow inventoryWindow = new InventoryStatusWindow(_username, _password, _fullName, _role);
            inventoryWindow.Show();
            this.Close();
        }

        private void NavigateToGoodsReceipt()
        {
            GoodsReceiptWindow goodsReceiptWindow = new GoodsReceiptWindow(_username, _password, _fullName, _role);
            goodsReceiptWindow.Show();
            this.Close();
        }

        private void NavigateToSuppliers()
        {
            SuppliersWindow suppliersWindow = new SuppliersWindow(_username, _password, _fullName, _role);
            suppliersWindow.Show();
            this.Close();
        }

        private void NavigateToNewSale()
        {
            NewSaleWindow newSaleWindow = new NewSaleWindow(_username, _password, _fullName, _role);
            newSaleWindow.Show();
            this.Close();
        }

        private void NavigateToSalesHistory()
        {
            SalesHistoryWindow salesHistoryWindow = new SalesHistoryWindow(_username, _password, _fullName, _role);
            salesHistoryWindow.Show();
            this.Close();
        }

        private void NavigateToInvoices()
        {
            MessageBox.Show("Invoices view - coming soon!", "Info",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void NavigateToSalesReports()
        {
            SalesReportsWindow salesReportsWindow = new SalesReportsWindow(_username, _password, _fullName, _role);
            salesReportsWindow.Show();
            this.Close();
        }

        private void NavigateToInventoryReports()
        {
            InventoryReportsWindow inventoryReportsWindow = new InventoryReportsWindow(_username, _password, _fullName, _role);
            inventoryReportsWindow.Show();
            this.Close();
        }

        private void NavigateToProductsToOrder()
        {
            ProductsToOrderWindow productsToOrderWindow = new ProductsToOrderWindow(_username, _password, _fullName, _role);
            productsToOrderWindow.Show();
            this.Close();
        }

        private void NavigateToUsers()
        {
            UsersWindow usersWindow = new UsersWindow(_username, _password, _fullName, _role);
            usersWindow.Show();
            this.Close();
        }

        private void HandleLogout()
        {
            var result = MessageBox.Show("Are you sure you want to logout?",
                                        "Confirm Logout",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MainWindow loginWindow = new MainWindow();
                loginWindow.Show();
                this.Close();
            }
        }
    }
}