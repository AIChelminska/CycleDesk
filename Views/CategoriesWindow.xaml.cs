using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CycleDesk
{
    public partial class CategoriesWindow : Window
    {
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;

        public CategoriesWindow(string username, string password, string fullName, string role)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _fullName = fullName;
            _role = role;

            lblUserFullName.Text = fullName;
            lblUserRole.Text = role;

            SetActiveButton(btnInventory);
            submenuInventory.Visibility = Visibility.Visible;
        }

        private void ToggleCategory_Click(object sender, MouseButtonEventArgs e)
        {
            Grid clickedGrid = sender as Grid;
            if (clickedGrid == null) return;

            string categoryTag = clickedGrid.Tag?.ToString();
            if (string.IsNullOrEmpty(categoryTag)) return;

            StackPanel subcategories = null;
            TextBlock expandIcon = null;

            switch (categoryTag)
            {
                case "Bikes":
                    subcategories = subcategoriesBikes;
                    expandIcon = expandBikes;
                    break;
                case "Tires":
                    subcategories = subcategoriesTires;
                    expandIcon = expandTires;
                    break;
            }

            if (subcategories != null && expandIcon != null)
            {
                if (subcategories.Visibility == Visibility.Visible)
                {
                    subcategories.Visibility = Visibility.Collapsed;
                    expandIcon.Text = "▶";
                }
                else
                {
                    subcategories.Visibility = Visibility.Visible;
                    expandIcon.Text = "▼";
                }
            }
        }

        private void SetActiveButton(Button activeButton)
        {
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

        private void Products_Click(object sender, RoutedEventArgs e)
        {
            ProductsWindow productsWindow = new ProductsWindow(_username, _password, _fullName, _role);
            productsWindow.Show();
            this.Close();
        }

        private void Categories_Click(object sender, RoutedEventArgs e) { }
        private void InventoryStatus_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Inventory Status view - coming soon!"); }
        private void GoodsReceipt_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Goods Receipt view - coming soon!"); }
        private void Suppliers_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Suppliers view - coming soon!"); }
        private void NewSale_Click(object sender, RoutedEventArgs e) { MessageBox.Show("New Sale view - coming soon!"); }
        private void SalesHistory_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Sales History view - coming soon!"); }
        private void Invoices_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Invoices view - coming soon!"); }
        private void SalesReports_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Sales Reports view - coming soon!"); }
        private void InventoryReports_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Inventory Reports view - coming soon!"); }
        private void ProductsToOrder_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Products to Order view - coming soon!"); }
        private void Users_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Users view - coming soon!"); }
        private void Settings_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Settings view - coming soon!"); }

        // MODAL HANDLING METHODS
        private void OpenAddCategoryModal_Click(object sender, RoutedEventArgs e)
        {
            modalOverlay.Visibility = Visibility.Visible;
            txtCategoryName.Text = "";
            cmbParentCategory.SelectedIndex = 0;
            txtCategoryName.Focus();
        }

        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            modalOverlay.Visibility = Visibility.Collapsed;
        }

        private void CancelModal_Click(object sender, RoutedEventArgs e)
        {
            modalOverlay.Visibility = Visibility.Collapsed;
        }

        private void SaveCategory_Click(object sender, RoutedEventArgs e)
        {
            string categoryName = txtCategoryName.Text.Trim();

            if (string.IsNullOrEmpty(categoryName))
            {
                MessageBox.Show("Please enter a category name.", "Validation Error",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCategoryName.Focus();
                return;
            }

            // Get selected parent category
            ComboBoxItem selectedItem = cmbParentCategory.SelectedItem as ComboBoxItem;
            string parentCategory = selectedItem?.Content?.ToString();

            // Here you would normally save to database
            // For now, just show success message
            string message = parentCategory == "None (Main Category)"
                ? $"Main category '{categoryName}' has been added successfully!"
                : $"Subcategory '{categoryName}' under '{parentCategory}' has been added successfully!";

            MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            modalOverlay.Visibility = Visibility.Collapsed;
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