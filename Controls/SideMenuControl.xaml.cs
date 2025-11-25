using System;
using System.Windows;
using System.Windows.Controls;

namespace CycleDesk.Controls
{
    public partial class SideMenuControl : UserControl
    {
        // Events dla nawigacji
        public event EventHandler DashboardClicked;
        public event EventHandler ProductsClicked;
        public event EventHandler CategoriesClicked;
        public event EventHandler InventoryStatusClicked;
        public event EventHandler GoodsReceiptClicked;
        public event EventHandler SuppliersClicked;
        public event EventHandler NewSaleClicked;
        public event EventHandler SalesHistoryClicked;
        public event EventHandler InvoicesClicked;
        public event EventHandler SalesReportsClicked;
        public event EventHandler InventoryReportsClicked;
        public event EventHandler ProductsToOrderClicked;
        public event EventHandler UsersClicked;
        public event EventHandler SettingsClicked;
        public event EventHandler LogoutClicked;

        public SideMenuControl()
        {
            InitializeComponent();
        }

        // Metoda do inicjalizacji danych użytkownika
        public void Initialize(string fullName, string role)
        {
            lblUserFullName.Text = fullName;
            lblUserRole.Text = role;
        }

        // Metoda do ustawiania aktywnego menu i podmenu
        public void SetActiveMenu(string mainMenu, string submenu = null)
        {
            // Reset wszystkich
            btnDashboard.Tag = null;
            btnInventory.Tag = null;
            btnSales.Tag = null;
            btnReports.Tag = null;
            btnAdministration.Tag = null;

            submenuInventory.Visibility = Visibility.Collapsed;
            submenuSales.Visibility = Visibility.Collapsed;
            submenuReports.Visibility = Visibility.Collapsed;
            submenuAdministration.Visibility = Visibility.Collapsed;

            // Ustaw aktywny
            switch (mainMenu.ToLower())
            {
                case "dashboard":
                    btnDashboard.Tag = "Active";
                    break;
                case "inventory":
                    btnInventory.Tag = "Active";
                    submenuInventory.Visibility = Visibility.Visible;
                    break;
                case "sales":
                    btnSales.Tag = "Active";
                    submenuSales.Visibility = Visibility.Visible;
                    break;
                case "reports":
                    btnReports.Tag = "Active";
                    submenuReports.Visibility = Visibility.Visible;
                    break;
                case "administration":
                    btnAdministration.Tag = "Active";
                    submenuAdministration.Visibility = Visibility.Visible;
                    break;
            }
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

        // Event handlers - wywołują eventy
        private void Dashboard_Click(object sender, RoutedEventArgs e) => DashboardClicked?.Invoke(this, EventArgs.Empty);
        private void Inventory_Click(object sender, RoutedEventArgs e) => ToggleSubmenu(submenuInventory);
        private void Sales_Click(object sender, RoutedEventArgs e) => ToggleSubmenu(submenuSales);
        private void Reports_Click(object sender, RoutedEventArgs e) => ToggleSubmenu(submenuReports);
        private void Administration_Click(object sender, RoutedEventArgs e) => ToggleSubmenu(submenuAdministration);

        private void Products_Click(object sender, RoutedEventArgs e) => ProductsClicked?.Invoke(this, EventArgs.Empty);
        private void Categories_Click(object sender, RoutedEventArgs e) => CategoriesClicked?.Invoke(this, EventArgs.Empty);
        private void InventoryStatus_Click(object sender, RoutedEventArgs e) => InventoryStatusClicked?.Invoke(this, EventArgs.Empty);
        private void GoodsReceipt_Click(object sender, RoutedEventArgs e) => GoodsReceiptClicked?.Invoke(this, EventArgs.Empty);
        private void Suppliers_Click(object sender, RoutedEventArgs e) => SuppliersClicked?.Invoke(this, EventArgs.Empty);
        private void NewSale_Click(object sender, RoutedEventArgs e) => NewSaleClicked?.Invoke(this, EventArgs.Empty);
        private void SalesHistory_Click(object sender, RoutedEventArgs e) => SalesHistoryClicked?.Invoke(this, EventArgs.Empty);
        private void Invoices_Click(object sender, RoutedEventArgs e) => InvoicesClicked?.Invoke(this, EventArgs.Empty);
        private void SalesReports_Click(object sender, RoutedEventArgs e) => SalesReportsClicked?.Invoke(this, EventArgs.Empty);
        private void InventoryReports_Click(object sender, RoutedEventArgs e) => InventoryReportsClicked?.Invoke(this, EventArgs.Empty);
        private void ProductsToOrder_Click(object sender, RoutedEventArgs e) => ProductsToOrderClicked?.Invoke(this, EventArgs.Empty);
        private void Users_Click(object sender, RoutedEventArgs e) => UsersClicked?.Invoke(this, EventArgs.Empty);
        private void Settings_Click(object sender, RoutedEventArgs e) => SettingsClicked?.Invoke(this, EventArgs.Empty);
        private void Logout_Click(object sender, RoutedEventArgs e) => LogoutClicked?.Invoke(this, EventArgs.Empty);
    }
}