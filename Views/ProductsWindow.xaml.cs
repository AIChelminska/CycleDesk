using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CycleDesk
{
    public partial class ProductsWindow : Window
    {
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;
        private Border _currentlySelectedRow = null;


        public ProductsWindow(string username, string password, string fullName, string role)
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

            // Rozwiń submenu Inventory (bo jesteśmy na stronie Products)
            submenuInventory.Visibility = Visibility.Visible;

        }

        // ===== CLICKABLE ROW LOGIC =====
        private void ProductRow_Click(object sender, MouseButtonEventArgs e)
        {
            Border clickedRow = sender as Border;
            if (clickedRow == null) return;

            // Resetuj poprzednie podświetlenie
            if (_currentlySelectedRow != null && _currentlySelectedRow != clickedRow)
            {
                _currentlySelectedRow.Background = new SolidColorBrush(Colors.White);
            }

            // Podświetl nowy wiersz
            clickedRow.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8F9FA"));
            _currentlySelectedRow = clickedRow;

            // Pokaż popup z przyciskami - tuż obok kursora
            actionButtonsPopup.PlacementTarget = clickedRow;
            actionButtonsPopup.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;

            // Małe przesunięcie: 10px w prawo, 5px w dół od kursora
            actionButtonsPopup.HorizontalOffset = 10;
            actionButtonsPopup.VerticalOffset = 5;

            actionButtonsPopup.IsOpen = true;
        }

        private void ProductRow_MouseLeave(object sender, MouseEventArgs e)
        {
            Border row = sender as Border;
            if (row == null) return;

            // Jeśli popup jest otwarty i mysz NIE jest nad popupem - zamknij
            if (actionButtonsPopup.IsOpen)
            {
                if (!IsMouseOverPopup())
                {
                    actionButtonsPopup.IsOpen = false;

                    // Zresetuj podświetlenie
                    if (_currentlySelectedRow != null)
                    {
                        _currentlySelectedRow.Background = new SolidColorBrush(Colors.White);
                        _currentlySelectedRow = null;
                    }
                }
            }
        }

        private bool IsMouseOverPopup()
        {
            if (actionButtonsPopup.Child == null) return false;

            Point mousePos = Mouse.GetPosition(actionButtonsPopup.Child as UIElement);

            if (actionButtonsPopup.Child is FrameworkElement element)
            {
                return mousePos.X >= 0 && mousePos.X <= element.ActualWidth &&
                       mousePos.Y >= 0 && mousePos.Y <= element.ActualHeight;
            }

            return false;
        }

        private void ActionButtonsPopup_Closed(object sender, EventArgs e)
        {
            if (_currentlySelectedRow != null)
            {
                _currentlySelectedRow.Background = new SolidColorBrush(Colors.White);
                _currentlySelectedRow = null;
            }
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (_currentlySelectedRow == null) return;

            Grid rowGrid = _currentlySelectedRow.Child as Grid;
            string productSKU = "Unknown";

            if (rowGrid != null)
            {
                foreach (var child in rowGrid.Children)
                {
                    if (child is Border border && Grid.GetColumn(border) == 1)
                    {
                        TextBlock skuText = FindVisualChild<TextBlock>(border);
                        if (skuText != null)
                        {
                            productSKU = skuText.Text;
                        }
                    }
                }
            }

            MessageBox.Show($"Edit product: {productSKU}", "Edit Product", MessageBoxButton.OK, MessageBoxImage.Information);
            actionButtonsPopup.IsOpen = false;
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (_currentlySelectedRow == null) return;

            Grid rowGrid = _currentlySelectedRow.Child as Grid;
            string productSKU = "Unknown";
            string productName = "Unknown";

            if (rowGrid != null)
            {
                foreach (var child in rowGrid.Children)
                {
                    if (child is Border border)
                    {
                        int column = Grid.GetColumn(border);
                        if (column == 1)
                        {
                            TextBlock skuText = FindVisualChild<TextBlock>(border);
                            if (skuText != null) productSKU = skuText.Text;
                        }
                        else if (column == 2)
                        {
                            TextBlock nameText = FindVisualChild<TextBlock>(border);
                            if (nameText != null) productName = nameText.Text;
                        }
                    }
                }
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete:\n\n{productSKU} - {productName}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show($"Product {productSKU} deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                actionButtonsPopup.IsOpen = false;
            }
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }
            return null;
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

        private void OpenAddProductModal_Click(object sender, RoutedEventArgs e)
        {
            modalOverlay.Visibility = Visibility.Visible;
        }

        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            modalOverlay.Visibility = Visibility.Collapsed;
        }

        private void SaveProduct_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Product saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            modalOverlay.Visibility = Visibility.Collapsed;
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

        private void Products_Click(object sender, RoutedEventArgs e) { }
        private void Categories_Click(object sender, RoutedEventArgs e)
        {
            CategoriesWindow categoriesWindow = new CategoriesWindow(_username, _password, _fullName, _role);
            categoriesWindow.Show();
            this.Close();
        }
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