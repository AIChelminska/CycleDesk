using CycleDesk.Views;
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

            _username = username;
            _password = password;
            _fullName = fullName;
            _role = role;

            // Inicjalizuj menu
            sideMenu.Initialize(fullName, role);
            sideMenu.SetActiveMenu("Dashboard");

            // Podłącz eventy
            sideMenu.ProductsClicked += (s, e) => NavigateToProducts();
            sideMenu.CategoriesClicked += (s, e) => NavigateToCategories();
            sideMenu.InventoryStatusClicked += (s, e) => NavigateToInventoryStatus();
            sideMenu.GoodsReceiptClicked += (s, e) => NavigateToGoodsReceipt();
            sideMenu.SuppliersClicked += (s, e) => NavigateToSuppliers();
            sideMenu.NewSaleClicked += (s, e) => NavigateToNewSale();
            sideMenu.SalesHistoryClicked += (s, e) => NavigateToSalesHistory();
            sideMenu.InvoicesClicked += (s, e) => NavigateToInvoices();
            sideMenu.SalesReportsClicked += (s, e) => NavigateToSalesReports();
            sideMenu.InventoryReportsClicked += (s, e) => NavigateToInventoryReports();
            sideMenu.ProductsToOrderClicked += (s, e) => NavigateToProductsToOrder();
            sideMenu.UsersClicked += (s, e) => NavigateToUsers();
            sideMenu.SettingsClicked += (s, e) => NavigateToSettings();
            sideMenu.LogoutClicked += (s, e) => HandleLogout();
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

        private void NavigateToSettings()
        {
            SettingsWindow settingsWindow = new SettingsWindow(_username, _password, _fullName, _role);
            settingsWindow.Show();
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
