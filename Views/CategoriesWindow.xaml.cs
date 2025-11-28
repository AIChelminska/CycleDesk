using CycleDesk.Views;
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

            // Inicjalizuj menu
            sideMenu.Initialize(fullName, role);
            sideMenu.SetActiveMenu("Inventory");

            // Podłącz eventy nawigacji
            ConnectMenuEvents();
        }

        private void ConnectMenuEvents()
        {
            sideMenu.DashboardClicked += (s, e) => { new MainDashboardWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.ProductsClicked += (s, e) => { new ProductsWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.CategoriesClicked += (s, e) => { }; // Już tutaj jesteśmy
            sideMenu.InventoryStatusClicked += (s, e) => { new InventoryStatusWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.GoodsReceiptClicked += (s, e) => { new GoodsReceiptWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.SuppliersClicked += (s, e) => { new SuppliersWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.NewSaleClicked += (s, e) => { new NewSaleWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.SalesHistoryClicked += (s, e) => { new SalesHistoryWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.InvoicesClicked += (s, e) => { new InvoicesWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.SalesReportsClicked += (s, e) => { new SalesReportsWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.InventoryReportsClicked += (s, e) => { new InventoryReportsWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.ProductsToOrderClicked += (s, e) => { new ProductsToOrderWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.UsersClicked += (s, e) => { new UsersWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.SettingsClicked += (s, e) => { new SettingsWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.LogoutClicked += (s, e) => HandleLogout();
        }

        private void HandleLogout()
        {
            var result = MessageBox.Show("Are you sure you want to logout?",
                                        "Confirm Logout",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                new MainWindow().Show();
                Close();
            }
        }

        // ===== CATEGORY-SPECIFIC METHODS =====

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

        // ===== MODAL HANDLING =====

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

            ComboBoxItem selectedItem = cmbParentCategory.SelectedItem as ComboBoxItem;
            string parentCategory = selectedItem?.Content?.ToString();

            string message = parentCategory == "None (Main Category)"
                ? $"Main category '{categoryName}' has been added successfully!"
                : $"Subcategory '{categoryName}' under '{parentCategory}' has been added successfully!";

            MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            modalOverlay.Visibility = Visibility.Collapsed;
        }
    }
}