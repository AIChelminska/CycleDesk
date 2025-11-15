using CycleDesk.Views;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CycleDesk
{
    public partial class InventoryStatusWindow : Window
    {
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;

        public InventoryStatusWindow(string username, string password, string fullName, string role)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _fullName = fullName;
            _role = role;

            // Ustaw dane użytkownika w UI
            lblUserFullName.Text = fullName;
            lblUserRole.Text = role;

            // Ustaw Inventory jako aktywny przycisk
            SetActiveButton(btnInventory);

            // Rozwiń submenu Inventory (bo jesteśmy na stronie Inventory Status)
            submenuInventory.Visibility = Visibility.Visible;
        }

        // ===== ACTIVE BUTTON MANAGEMENT =====
        private void SetActiveButton(Button activeButton)
        {
            // Resetuj wszystkie przyciski
            btnDashboard.Tag = null;
            btnInventory.Tag = null;
            btnSales.Tag = null;
            btnReports.Tag = null;
            btnAdministration.Tag = null;

            btnDashboard.Background = System.Windows.Media.Brushes.Transparent;
            btnInventory.Background = System.Windows.Media.Brushes.Transparent;
            btnSales.Background = System.Windows.Media.Brushes.Transparent;
            btnReports.Background = System.Windows.Media.Brushes.Transparent;
            btnAdministration.Background = System.Windows.Media.Brushes.Transparent;

            // Ustaw aktywny przycisk
            activeButton.Tag = "Active";
        }

        private void ToggleSubmenu(StackPanel submenu)
        {
            // Jeśli submenu jest już otwarte, zamknij
            if (submenu.Visibility == Visibility.Visible)
            {
                submenu.Visibility = Visibility.Collapsed;
                return;
            }

            // Zamknij wszystkie inne submenu
            submenuInventory.Visibility = Visibility.Collapsed;
            submenuSales.Visibility = Visibility.Collapsed;
            submenuReports.Visibility = Visibility.Collapsed;
            submenuAdministration.Visibility = Visibility.Collapsed;

            // Otwórz wybrane submenu
            submenu.Visibility = Visibility.Visible;
        }

        // ===== MENU NAVIGATION =====
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MainDashboardWindow dashboard = new MainDashboardWindow(_username, _password, _fullName, _role);
            dashboard.Show();
            this.Close();
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            ToggleSubmenu(submenuInventory);
            SetActiveButton(btnInventory);
        }

        private void Sales_Click(object sender, RoutedEventArgs e)
        {
            ToggleSubmenu(submenuSales);
            SetActiveButton(btnSales);
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            ToggleSubmenu(submenuReports);
            SetActiveButton(btnReports);
        }

        private void Administration_Click(object sender, RoutedEventArgs e)
        {
            ToggleSubmenu(submenuAdministration);
            SetActiveButton(btnAdministration);
        }

        // ===== SUBMENU NAVIGATION =====
        private void Products_Click(object sender, RoutedEventArgs e)
        {
            ProductsWindow productsWindow = new ProductsWindow(_username, _password, _fullName, _role);
            productsWindow.Show();
            this.Close();
        }

        private void Categories_Click(object sender, RoutedEventArgs e)
        {
            CategoriesWindow categoriesWindow = new CategoriesWindow(_username, _password, _fullName, _role);
            categoriesWindow.Show();
            this.Close();
        }

        // Pusta metoda - już jesteśmy na tej stronie
        private void InventoryStatus_Click(object sender, RoutedEventArgs e) { }

        private void GoodsReceipt_Click(object sender, RoutedEventArgs e)
        {
            GoodsReceiptWindow goodsReceiptWindow = new GoodsReceiptWindow(_username, _password, _fullName, _role);
            goodsReceiptWindow.Show();
            this.Close();
        }

        private void Suppliers_Click(object sender, RoutedEventArgs e)
        {
            SuppliersWindow suppliersWindow = new SuppliersWindow(_username, _password, _fullName, _role);
            suppliersWindow.Show();
            this.Close();
        }

        private void NewSale_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("New Sale view - coming soon!", "Info",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SalesHistory_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Sales History view - coming soon!", "Info",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Invoices_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Invoices view - coming soon!", "Info",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SalesReports_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Sales Reports view - coming soon!", "Info",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void InventoryReports_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Inventory Reports view - coming soon!", "Info",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ProductsToOrder_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Products to Order view - coming soon!", "Info",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Users_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Users view - coming soon!", "Info",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Settings view - coming soon!", "Info",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
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
