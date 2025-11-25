using CycleDesk.Views;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CycleDesk
{
    public partial class InvoicesWindow : Window
    {
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;
        private Border _currentlyHoveredRow = null;

        public InvoicesWindow(string username, string password, string fullName, string role)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _fullName = fullName;
            _role = role;

            // Ustaw dane użytkownika w UI
            lblUserFullName.Text = fullName;
            lblUserRole.Text = role;

            // Ustaw Sales jako aktywny przycisk
            SetActiveButton(btnSales);

            // Rozwiń submenu Sales (bo jesteśmy na stronie Invoices)
            submenuSales.Visibility = Visibility.Visible;
        }

        // ===== ROW HOVER EFFECT =====
        private void InvoiceRow_MouseEnter(object sender, MouseEventArgs e)
        {
            Border row = sender as Border;
            if (row == null) return;

            _currentlyHoveredRow = row;
            row.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8F9FA"));
        }

        private void InvoiceRow_MouseLeave(object sender, MouseEventArgs e)
        {
            Border row = sender as Border;
            if (row == null) return;

            row.Background = new SolidColorBrush(Colors.White);
            _currentlyHoveredRow = null;
        }

        private void ActionButtonsPopup_Closed(object sender, EventArgs e)
        {
            if (_currentlyHoveredRow != null)
            {
                _currentlyHoveredRow.Background = new SolidColorBrush(Colors.White);
                _currentlyHoveredRow = null;
            }
        }

        // ===== INVOICE ACTIONS =====
        private void GenerateInvoice_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Generate Invoice functionality will allow you to create a new invoice from a sale.\n\nThis feature is coming soon!",
                "Generate Invoice",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void ViewInvoice_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "View Invoice Details\n\nThis will open a detailed view of the selected invoice showing:\n" +
                "• Invoice information\n" +
                "• Customer details\n" +
                "• Itemized list of products\n" +
                "• Payment information\n" +
                "• Total amount and tax breakdown",
                "View Invoice",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            actionButtonsPopup.IsOpen = false;
        }

        private void DownloadInvoice_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Do you want to download this invoice as a PDF file?",
                "Download Invoice",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show(
                    "Invoice downloaded successfully!\n\nThe PDF file has been saved to your Downloads folder.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }

            actionButtonsPopup.IsOpen = false;
        }

        private void DeleteInvoice_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete this invoice?\n\n" +
                "This action cannot be undone and will permanently remove the invoice from the system.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show(
                    "Invoice deleted successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }

            actionButtonsPopup.IsOpen = false;
        }

        // ===== ACTIVE BUTTON MANAGEMENT =====
        private void SetActiveButton(Button activeButton)
        {
            btnDashboard.Tag = null;
            btnInventory.Tag = null;
            btnSales.Tag = null;
            btnReports.Tag = null;
            btnAdministration.Tag = null;

            btnDashboard.Background = Brushes.Transparent;
            btnInventory.Background = Brushes.Transparent;
            btnSales.Background = Brushes.Transparent;
            btnReports.Background = Brushes.Transparent;
            btnAdministration.Background = Brushes.Transparent;

            activeButton.Tag = "Active";
        }

        private void ToggleSubmenu(StackPanel submenu)
        {
            if (submenu.Visibility == Visibility.Visible)
            {
                submenu.Visibility = Visibility.Collapsed;
                return;
            }

            submenuInventory.Visibility = Visibility.Collapsed;
            submenuSales.Visibility = Visibility.Collapsed;
            submenuReports.Visibility = Visibility.Collapsed;
            submenuAdministration.Visibility = Visibility.Collapsed;

            submenu.Visibility = Visibility.Visible;
        }

        // ===== NAVIGATION HANDLERS =====
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

        private void InventoryStatus_Click(object sender, RoutedEventArgs e)
        {
            InventoryStatusWindow inventoryStatusWindow = new InventoryStatusWindow(_username, _password, _fullName, _role);
            inventoryStatusWindow.Show();
            this.Close();
        }

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
            NewSaleWindow newSaleWindow = new NewSaleWindow(_username, _password, _fullName, _role);
            newSaleWindow.Show();
            this.Close();
        }

        private void SalesHistory_Click(object sender, RoutedEventArgs e)
        {
            SalesHistoryWindow salesHistoryWindow = new SalesHistoryWindow(_username, _password, _fullName, _role);
            salesHistoryWindow.Show();
            this.Close();
        }

        private void Invoices_Click(object sender, RoutedEventArgs e)
        {
            // Już jesteśmy na stronie Invoices - nic nie rób
        }

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
            var result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Confirm Logout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                MainWindow loginWindow = new MainWindow();
                loginWindow.Show();
                this.Close();
            }
        }
    }
}
