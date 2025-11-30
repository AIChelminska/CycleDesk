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

            // Ukryj przycisk Users dla Operator i Cashier
            if (role == "Operator" || role == "Cashier")
            {
                btnUsers.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnUsers.Visibility = Visibility.Visible;
            }
        }

        // Metoda do ustawiania aktywnego menu i podmenu
        public void SetActiveMenu(string mainMenu, string submenu = null)
        {
            // Reset wszystkich głównych menu
            btnDashboard.Tag = null;
            btnInventory.Tag = null;
            btnSales.Tag = null;
            btnReports.Tag = null;
            btnAdministration.Tag = null;

            // Reset wszystkich submenu
            btnProducts.Tag = null;
            btnCategories.Tag = null;
            btnInventoryStatus.Tag = null;
            btnGoodsReceipt.Tag = null;
            btnSuppliers.Tag = null;
            btnNewSale.Tag = null;
            btnSalesHistory.Tag = null;
            btnSalesReports.Tag = null;
            btnInventoryReports.Tag = null;
            btnProductsToOrder.Tag = null;
            btnUsers.Tag = null;
            btnSettings.Tag = null;

            submenuInventory.Visibility = Visibility.Collapsed;
            submenuSales.Visibility = Visibility.Collapsed;
            submenuReports.Visibility = Visibility.Collapsed;
            submenuAdministration.Visibility = Visibility.Collapsed;

            // Ustaw aktywny - tylko jedno podświetlenie!
            switch (mainMenu.ToLower())
            {
                case "dashboard":
                    // Dashboard nie ma submenu - podświetl główny przycisk
                    btnDashboard.Tag = "Active";
                    break;
                case "inventory":
                    submenuInventory.Visibility = Visibility.Visible;
                    // Podświetl TYLKO submenu, nie główny przycisk
                    SetActiveSubmenu(submenu);
                    break;
                case "sales":
                    submenuSales.Visibility = Visibility.Visible;
                    SetActiveSubmenu(submenu);
                    break;
                case "reports":
                    submenuReports.Visibility = Visibility.Visible;
                    SetActiveSubmenu(submenu);
                    break;
                case "administration":
                    submenuAdministration.Visibility = Visibility.Visible;
                    SetActiveSubmenu(submenu);
                    break;
            }
        }

        private void SetActiveSubmenu(string submenu)
        {
            if (string.IsNullOrEmpty(submenu)) return;

            switch (submenu.ToLower())
            {
                // Inventory submenu
                case "products":
                    btnProducts.Tag = "Active";
                    break;
                case "categories":
                    btnCategories.Tag = "Active";
                    break;
                case "inventorystatus":
                case "stockstatus":
                    btnInventoryStatus.Tag = "Active";
                    break;
                case "goodsreceipt":
                    btnGoodsReceipt.Tag = "Active";
                    break;
                case "suppliers":
                    btnSuppliers.Tag = "Active";
                    break;

                // Sales submenu
                case "newsale":
                    btnNewSale.Tag = "Active";
                    break;
                case "saleshistory":
                case "history":
                    btnSalesHistory.Tag = "Active";
                    break;
                case "invoices":
                    // Invoices nie ma przycisku w menu, ale obsługujemy dla kompatybilności
                    break;

                // Reports submenu
                case "salesreports":
                    btnSalesReports.Tag = "Active";
                    break;
                case "inventoryreports":
                    btnInventoryReports.Tag = "Active";
                    break;
                case "productstoorder":
                    btnProductsToOrder.Tag = "Active";
                    break;

                // Administration submenu
                case "users":
                    btnUsers.Tag = "Active";
                    break;
                case "settings":
                    btnSettings.Tag = "Active";
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
        private void SalesReports_Click(object sender, RoutedEventArgs e) => SalesReportsClicked?.Invoke(this, EventArgs.Empty);
        private void InventoryReports_Click(object sender, RoutedEventArgs e) => InventoryReportsClicked?.Invoke(this, EventArgs.Empty);
        private void ProductsToOrder_Click(object sender, RoutedEventArgs e) => ProductsToOrderClicked?.Invoke(this, EventArgs.Empty);
        private void Users_Click(object sender, RoutedEventArgs e) => UsersClicked?.Invoke(this, EventArgs.Empty);
        private void Settings_Click(object sender, RoutedEventArgs e) => SettingsClicked?.Invoke(this, EventArgs.Empty);
        private void Logout_Click(object sender, RoutedEventArgs e) => LogoutClicked?.Invoke(this, EventArgs.Empty);
    }
}
