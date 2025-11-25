using CycleDesk.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CycleDesk
{
    public partial class GoodsReceiptWindow : Window
    {
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;

        // Lista produktów w paragonie (tylko prototyp)
        private ObservableCollection<dynamic> _receiptProducts = new ObservableCollection<dynamic>();

        public GoodsReceiptWindow(string username, string password, string fullName, string role)
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

            // Rozwiń submenu Inventory
            submenuInventory.Visibility = Visibility.Visible;

            // Podłącz DataGrid do listy produktów
            dgProducts.ItemsSource = _receiptProducts;
        }

        // ===== ACTIVE BUTTON MANAGEMENT =====
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

        // ===== SEARCH AND FILTER =====
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Wyszukiwanie zostanie zaimplementowane później z prawdziwymi danymi
        }



        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            //SearchTextBox.Text = string.Empty;
            MessageBox.Show("Dane zostały odświeżone.",
                          "Informacja",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);
        }

        private void ExportCSV_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Eksport do CSV będzie wkrótce dostępny.",
                          "Informacja",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);
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

        private void InventoryStatus_Click(object sender, RoutedEventArgs e)
        {
            InventoryStatusWindow inventoryStatusWindow = new InventoryStatusWindow(_username, _password, _fullName, _role);
            inventoryStatusWindow.Show();
            this.Close();
        }

        // Pusta metoda - już jesteśmy na tej stronie
        private void GoodsReceipt_Click(object sender, RoutedEventArgs e) { }

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

        // ===== MODAL MANAGEMENT =====
        private void AddNewReceipt_Click(object sender, RoutedEventArgs e)
        {
            // Auto-generate receipt number
            modalTxtReceiptNo.Text = GenerateReceiptNumber();

            // Set current date
            modalTxtDocumentDate.Text = DateTime.Now.ToString("dd.MM.yy");

            // Set received by to current user
            modalTxtReceivedBy.Text = _fullName;

            // Reset form fields
            modalTxtSupplierInvoiceNo.Clear();
            modalTxtSupplierInvoiceDate.Clear();
            modalCmbSupplier.SelectedIndex = 0;
            modalTxtTotalValue.Clear();
            modalTxtNotes.Clear();
            modalToggleSwitch.IsChecked = false; // FALSE = Accepted (zielony)
            modalWarningRejected.Visibility = Visibility.Collapsed;

            // Wyczyść listę produktów
            _receiptProducts.Clear();

            // Reset status label
            if (modalStatusLabel != null)
            {
                modalStatusLabel.Text = "Accepted";
                modalStatusLabel.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF059669"));
            }

            // Show modal
            modalOverlay.Visibility = Visibility.Visible;
        }

        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            modalOverlay.Visibility = Visibility.Collapsed;
        }

        private void SaveReceipt_Click(object sender, RoutedEventArgs e)
        {
            // Validate required fields
            if (modalCmbSupplier.SelectedIndex == 0)
            {
                MessageBox.Show("Please select a supplier.",
                               "Validation Error",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(modalTxtSupplierInvoiceNo.Text))
            {
                MessageBox.Show("Please enter supplier invoice number.",
                               "Validation Error",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(modalTxtSupplierInvoiceDate.Text))
            {
                MessageBox.Show("Please enter supplier invoice date.",
                               "Validation Error",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(modalTxtTotalValue.Text))
            {
                MessageBox.Show("Please enter total value.",
                               "Validation Error",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            // Additional validation for rejected deliveries - require photos
            if (modalToggleSwitch.IsChecked == true)
            {
                MessageBox.Show("Please upload photos for rejected delivery.\nThis feature will be implemented soon.",
                               "Photos Required",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);
                return;
            }

            // TODO: Save to database
            MessageBox.Show($"Receipt {modalTxtReceiptNo.Text} saved successfully!\n\n" +
                           $"Supplier: {((ComboBoxItem)modalCmbSupplier.SelectedItem).Content}\n" +
                           $"Products: {_receiptProducts.Count}\n" +
                           $"Total Value: {modalTxtTotalValue.Text} PLN\n" +
                           $"Status: {(modalToggleSwitch.IsChecked == true ? "Rejected" : "Accepted")}",
                           "Success",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);

            // Close modal
            modalOverlay.Visibility = Visibility.Collapsed;
        }

        private void ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            // Checked = Rejected (czerwony)
            if (modalWarningRejected != null)
            {
                modalWarningRejected.Visibility = Visibility.Visible;
            }

            // Zmień tekst i kolor na "Rejected"
            if (modalStatusLabel != null)
            {
                modalStatusLabel.Text = "Rejected";
                modalStatusLabel.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFDC3545"));
            }
        }

        private void ToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            // Unchecked = Accepted (zielony)
            if (modalWarningRejected != null)
            {
                modalWarningRejected.Visibility = Visibility.Collapsed;
            }

            // Zmień tekst i kolor na "Accepted"
            if (modalStatusLabel != null)
            {
                modalStatusLabel.Text = "Accepted";
                modalStatusLabel.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF059669"));
            }
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            // Pokaż modal dodawania produktu
            productModalOverlay.Visibility = Visibility.Visible;

            // Reset formularza
            productModalCmbProduct.SelectedIndex = 0;
            productModalTxtQuantity.Clear();
            productModalTxtPurchasePrice.Clear();
        }

        private void RemoveProduct_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button?.DataContext != null)
            {
                _receiptProducts.Remove(button.DataContext);
            }
        }

        private void CloseProductModal_Click(object sender, RoutedEventArgs e)
        {
            productModalOverlay.Visibility = Visibility.Collapsed;
        }

        private void AddProductToReceipt_Click(object sender, RoutedEventArgs e)
        {
            if (productModalCmbProduct.SelectedIndex > 0 &&
                !string.IsNullOrWhiteSpace(productModalTxtQuantity.Text) &&
                !string.IsNullOrWhiteSpace(productModalTxtPurchasePrice.Text))
            {
                string productName = ((ComboBoxItem)productModalCmbProduct.SelectedItem).Content.ToString();

                // Parsowanie ilości
                if (!int.TryParse(productModalTxtQuantity.Text, out int qty) || qty <= 0)
                {
                    MessageBox.Show("Please enter a valid quantity (number greater than 0)!",
                                   "Validation Error",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
                    return;
                }

                // Parsowanie ceny - akceptuje zarówno kropkę jak i przecinek
                string priceText = productModalTxtPurchasePrice.Text.Replace(',', '.');
                if (!decimal.TryParse(priceText,
                                     System.Globalization.NumberStyles.Any,
                                     System.Globalization.CultureInfo.InvariantCulture,
                                     out decimal price) || price <= 0)
                {
                    MessageBox.Show("Please enter a valid price (number greater than 0)!\nYou can use both comma (,) and dot (.) as decimal separator.",
                                   "Validation Error",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
                    return;
                }

                _receiptProducts.Add(new
                {
                    ProductName = productName,
                    Quantity = qty,
                    PurchasePrice = price,
                    TotalValue = qty * price
                });

                productModalOverlay.Visibility = Visibility.Collapsed;

                MessageBox.Show($"Product added successfully!", "Success",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please fill all fields!", "Validation Error",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private string GenerateReceiptNumber()
        {
            // TODO: Get last number from database and increment
            // For now, return next sequential number
            Random random = new Random();
            int nextNumber = random.Next(6, 100); // Temporary random number
            return $"PZ/{nextNumber:D3}";
        }
    }
}