using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CycleDesk
{
    public partial class MainDashboardWindow : Window
    {
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;

        public MainDashboardWindow(string username, string password, string fullName, string role)
        {
            InitializeComponent();

            // Zapisz dane użytkownika
            _username = username;
            _password = password;
            _fullName = fullName;
            _role = role;

            // Ustaw dane w UI
            lblUserFullName.Text = fullName;
            lblUserRole.Text = role;

            // Ustaw Dashboard jako aktywny
            SetActiveButton(btnDashboard);
        }

        private void SetActiveButton(System.Windows.Controls.Button activeButton)
        {
            // Reset wszystkich buttonów - usuń Tag
            btnDashboard.Tag = null;
            btnInventory.Tag = null;
            btnSales.Tag = null;
            btnReports.Tag = null;
            btnAdministration.Tag = null;

            // Usuń poprzednie tła (na wszelki wypadek)
            btnDashboard.Background = Brushes.Transparent;
            btnInventory.Background = Brushes.Transparent;
            btnSales.Background = Brushes.Transparent;
            btnReports.Background = Brushes.Transparent;
            btnAdministration.Background = Brushes.Transparent;

            // Zaznacz aktywny - ustaw Tag="Active"
            activeButton.Tag = "Active";
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(btnDashboard);
            lblPageTitle.Text = "Dashboard";
            MessageBox.Show("Dashboard clicked!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ===== MENU TOGGLE =====
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

        // ===== SUBMENU TOGGLE LOGIC =====
        private void ToggleSubmenu(StackPanel submenu)
        {
            // Jeśli klikamy już otwarte submenu - zamknij
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

            // Otwórz kliknięte submenu
            submenu.Visibility = Visibility.Visible;
        }

        // ===== INVENTORY SUBMENU HANDLERS =====
        private void InventoryProducts_Click(object sender, RoutedEventArgs e)
        {
            ProductsWindow productsWindow = new ProductsWindow(_username, _password, _fullName, _role);
            productsWindow.Show();
            this.Close();
        }

        private void InventoryCategories_Click(object sender, RoutedEventArgs e)
        {
            CategoriesWindow categoriesWindow = new CategoriesWindow(_username, _password, _fullName, _role);
            categoriesWindow.Show();
            this.Close();
        }

        private void InventoryStatus_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Inventory Status view - coming soon!");
        }

        private void GoodsReceipt_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Goods Receipt view - coming soon!");
        }

        private void Suppliers_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Suppliers view - coming soon!");
        }

        // ===== SALES SUBMENU HANDLERS =====
        private void NewSale_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("New Sale view - coming soon!");
        }

        private void SalesHistory_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Sales History view - coming soon!");
        }

        private void Invoices_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Invoices view - coming soon!");
        }

        // ===== REPORTS SUBMENU HANDLERS =====
        private void SalesReports_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Sales Reports view - coming soon!");
        }

        private void InventoryReports_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Inventory Reports view - coming soon!");
        }

        private void ProductsToOrder_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Products to Order view - coming soon!");
        }

        // ===== ADMINISTRATION SUBMENU HANDLERS =====
        private void Users_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Users view - coming soon!");
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Settings view - coming soon!");
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?",
                                        "Confirm Logout",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Otwórz okno logowania
                MainWindow loginWindow = new MainWindow();
                loginWindow.Show();

                // Zamknij dashboard
                this.Close();
            }
        }
    }
}
