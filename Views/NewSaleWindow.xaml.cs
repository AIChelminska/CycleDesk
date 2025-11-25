using CycleDesk.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CycleDesk
{
    public partial class NewSaleWindow : Window
    {
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;

        // Document type tracking
        private string _selectedDocumentType = "Receipt"; // Default: Receipt

        // Cart items collection
        private ObservableCollection<CartItem> _cartItems;

        // Mock product database
        private List<Product> _mockProducts;

        public NewSaleWindow(string username, string password, string fullName, string role)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _fullName = fullName;
            _role = role;

            // Set user data in UI
            lblUserFullName.Text = fullName;
            lblUserRole.Text = role;
            lblCashierName.Text = fullName;

            // Set Sales as active button
            SetActiveButton(btnSales);
            submenuSales.Visibility = Visibility.Visible;

            // Initialize cart
            _cartItems = new ObservableCollection<CartItem>();
            _cartItems.CollectionChanged += CartItems_CollectionChanged;
            cartItemsList.ItemsSource = _cartItems;

            // Initialize mock products
            InitializeMockProducts();

            // Set transaction info
            lblTransactionNumber.Text = GenerateTransactionNumber();
            lblTransactionDate.Text = DateTime.Now.ToString("dd MMM yyyy, HH:mm");

            // Focus on search box
            txtSearchProduct.Focus();
        }

        // ===== MOCK DATA =====
        private void InitializeMockProducts()
        {
            _mockProducts = new List<Product>
            {
                new Product { SKU = "BIKE-001", ProductName = "Mountain Bike Pro X", UnitPrice = 2499.00m, ImagePath = "/Images/bike1.jpg", StockQuantity = 5 },
                new Product { SKU = "BIKE-002", ProductName = "City Bike Classic", UnitPrice = 1299.00m, ImagePath = "/Images/bike2.jpg", StockQuantity = 8 },
                new Product { SKU = "TIRE-001", ProductName = "All-Terrain Tire 26\"", UnitPrice = 89.99m, ImagePath = "/Images/tire1.jpg", StockQuantity = 25 },
                new Product { SKU = "TIRE-002", ProductName = "Road Bike Tire 28\"", UnitPrice = 79.99m, ImagePath = "/Images/tire2.jpg", StockQuantity = 30 },
                new Product { SKU = "HELM-001", ProductName = "Safety Helmet Pro", UnitPrice = 149.00m, ImagePath = "/Images/helmet1.jpg", StockQuantity = 15 },
                new Product { SKU = "LOCK-001", ProductName = "Heavy Duty Lock", UnitPrice = 59.99m, ImagePath = "/Images/lock1.jpg", StockQuantity = 20 },
                new Product { SKU = "PUMP-001", ProductName = "Floor Pump Premium", UnitPrice = 45.00m, ImagePath = "/Images/pump1.jpg", StockQuantity = 12 },
                new Product { SKU = "LIGHT-001", ProductName = "LED Light Set", UnitPrice = 34.99m, ImagePath = "/Images/light1.jpg", StockQuantity = 18 }
            };
        }

        private string GenerateTransactionNumber()
        {
            return $"TRX{DateTime.Now:yyyyMMddHHmmss}";
        }

        // ===== KEYBOARD SHORTCUTS =====
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // F1 - Focus on search
            if (e.Key == Key.F1)
            {
                txtSearchProduct.Focus();
                txtSearchProduct.SelectAll();
                e.Handled = true;
            }
            // F2 - Cash payment
            else if (e.Key == Key.F2)
            {
                rbCash.IsChecked = true;
                txtAmountReceived.Focus();
                e.Handled = true;
            }
            // F3 - Card payment
            else if (e.Key == Key.F3)
            {
                rbCard.IsChecked = true;
                e.Handled = true;
            }
            // F9 - Cancel sale
            else if (e.Key == Key.F9)
            {
                CancelSale_Click(sender, new RoutedEventArgs());
                e.Handled = true;
            }
            // Enter - Complete sale (if enabled)
            else if (e.Key == Key.Enter && btnCompleteSale.IsEnabled)
            {
                CompleteSale_Click(sender, new RoutedEventArgs());
                e.Handled = true;
            }
            // Esc - Clear search or cancel
            else if (e.Key == Key.Escape)
            {
                if (!string.IsNullOrWhiteSpace(txtSearchProduct.Text) &&
                    txtSearchProduct.Text != "🔍 Scan barcode or search product (SKU, name)...")
                {
                    txtSearchProduct.Text = "";
                }
                e.Handled = true;
            }
        }

        // ===== SEARCH FUNCTIONALITY =====
        private void SearchProduct_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearchProduct.Text == "🔍 Scan barcode or search product (SKU, name)...")
            {
                txtSearchProduct.Text = "";
                txtSearchProduct.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF212529"));
            }
        }

        private void SearchProduct_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearchProduct.Text))
            {
                txtSearchProduct.Text = "🔍 Scan barcode or search product (SKU, name)...";
                txtSearchProduct.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D"));
            }
        }

        private void SearchProduct_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = txtSearchProduct.Text.Trim();

            // Ignore placeholder text
            if (searchText == "🔍 Scan barcode or search product (SKU, name)..." || string.IsNullOrWhiteSpace(searchText))
                return;

            // Search for product by SKU or Name
            var foundProduct = _mockProducts.FirstOrDefault(p =>
                p.SKU.Equals(searchText, StringComparison.OrdinalIgnoreCase) ||
                p.ProductName.ToLower().Contains(searchText.ToLower()));

            if (foundProduct != null)
            {
                AddProductToCart(foundProduct);
                txtSearchProduct.Text = "";
            }
        }

        // ===== CART MANAGEMENT =====
        private void AddProductToCart(Product product)
        {
            // Check if product already in cart
            var existingItem = _cartItems.FirstOrDefault(item => item.SKU == product.SKU);

            if (existingItem != null)
            {
                // Increase quantity
                existingItem.Quantity++;
            }
            else
            {
                // Add new item
                _cartItems.Add(new CartItem
                {
                    SKU = product.SKU,
                    ProductName = product.ProductName,
                    UnitPrice = product.UnitPrice,
                    Quantity = 1,
                    ImagePath = product.ImagePath
                });
            }

            // Visual feedback - flash effect (simple)
            UpdateCartDisplay();
        }

        private void CartItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateCartDisplay();
        }

        private void UpdateCartDisplay()
        {
            if (_cartItems.Count == 0)
            {
                emptyCartState.Visibility = Visibility.Visible;
                cartItemsList.Visibility = Visibility.Collapsed;
                btnCompleteSale.IsEnabled = false;
            }
            else
            {
                emptyCartState.Visibility = Visibility.Collapsed;
                cartItemsList.Visibility = Visibility.Visible;
                btnCompleteSale.IsEnabled = ValidatePayment();
            }

            // Update item count
            if (lblItemCount != null)
                lblItemCount.Text = _cartItems.Count.ToString();

            CalculateTotals();
        }

        private void CalculateTotals()
        {
            decimal subtotal = _cartItems.Sum(item => item.Subtotal);
            decimal taxRate = 0.23m; // 23% VAT
            decimal tax = subtotal * taxRate;
            decimal total = subtotal + tax;

            lblSubtotal.Text = $"PLN {subtotal:N2}";
            lblTax.Text = $"PLN {tax:N2}";
            lblTotal.Text = $"PLN {total:N2}";

            // Update change if cash payment
            if (rbCash.IsChecked == true)
            {
                CalculateChange();
            }
        }

        // ===== QUANTITY CONTROLS =====
        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.Tag is CartItem item)
            {
                item.Quantity++;
                CalculateTotals();
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.Tag is CartItem item)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                    CalculateTotals();
                }
                else
                {
                    // Remove item if quantity would be 0
                    _cartItems.Remove(item);
                }
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.Tag is CartItem item)
            {
                _cartItems.Remove(item);
            }
        }

        private void ClearCart_Click(object sender, RoutedEventArgs e)
        {
            if (_cartItems.Count == 0)
                return;

            var result = MessageBox.Show(
                "Are you sure you want to clear the cart?",
                "Clear Cart",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _cartItems.Clear();
                txtSearchProduct.Text = "";
                txtAmountReceived.Text = "";
                txtSearchProduct.Focus();
            }
        }

        // ===== PAYMENT METHOD =====
        private void PaymentMethod_Changed(object sender, RoutedEventArgs e)
        {
            // Null check -ważne podczas inicjalizacji
            if (cashPaymentSection == null || cardPaymentSection == null)
                return;

            if (rbCash.IsChecked == true)
            {
                cashPaymentSection.Visibility = Visibility.Visible;
                cardPaymentSection.Visibility = Visibility.Collapsed;
                txtAmountReceived?.Focus();
            }
            else if (rbCard.IsChecked == true)
            {
                cashPaymentSection.Visibility = Visibility.Collapsed;
                cardPaymentSection.Visibility = Visibility.Visible;
            }

            btnCompleteSale.IsEnabled = ValidatePayment();
        }

        private void DocumentType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Null check - important during initialization
            if (invoiceDetailsPanel == null)
                return;

            if (cmbDocumentType.SelectedIndex == 1) // Invoice selected
            {
                invoiceDetailsPanel.Visibility = Visibility.Visible;
            }
            else // Receipt selected
            {
                invoiceDetailsPanel.Visibility = Visibility.Collapsed;
            }

            btnCompleteSale.IsEnabled = ValidatePayment();
        }

        private void AmountReceived_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateChange();
            btnCompleteSale.IsEnabled = ValidatePayment();
        }

        private void CalculateChange()
        {
            if (rbCash.IsChecked != true)
                return;

            decimal total = _cartItems.Sum(item => item.Subtotal) * 1.23m; // With tax

            if (decimal.TryParse(txtAmountReceived.Text, out decimal amountReceived))
            {
                decimal change = amountReceived - total;

                if (change >= 0)
                {
                    lblChange.Text = $"PLN {change:N2}";
                    changeDisplay.Visibility = Visibility.Visible;
                }
                else
                {
                    changeDisplay.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                changeDisplay.Visibility = Visibility.Collapsed;
            }
        }

        private bool ValidatePayment()
        {
            // Must have items in cart
            if (_cartItems.Count == 0)
                return false;

            // If invoice is selected, validate invoice fields
            if (cmbDocumentType.SelectedIndex == 1) // Invoice
            {
                if (string.IsNullOrWhiteSpace(txtCompanyName?.Text) ||
                    string.IsNullOrWhiteSpace(txtNIP?.Text) ||
                    string.IsNullOrWhiteSpace(txtAddress?.Text) ||
                    string.IsNullOrWhiteSpace(txtCity?.Text) ||
                    string.IsNullOrWhiteSpace(txtPostalCode?.Text))
                {
                    return false;
                }
            }

            // Card payment - valid if cart has items and invoice data (if needed) is filled
            if (rbCard.IsChecked == true)
                return true;

            // Cash payment - must have sufficient amount
            if (rbCash.IsChecked == true)
            {
                decimal total = _cartItems.Sum(item => item.Subtotal) * 1.23m;

                if (decimal.TryParse(txtAmountReceived.Text, out decimal amountReceived))
                {
                    return amountReceived >= total;
                }
            }

            return false;
        }

        // ===== COMPLETE SALE =====
        private void CompleteSale_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (!ValidatePayment())
            {
                MessageBox.Show(
                    "Please ensure all payment information is correct.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Get totals
            decimal subtotal = _cartItems.Sum(item => item.Subtotal);
            decimal total = subtotal * 1.23m;
            string paymentMethod = rbCash.IsChecked == true ? "Cash" : "Card";

            // Get selected document type
            string documentType = cmbDocumentType.SelectedIndex == 0 ? "Receipt" : "Invoice";

            // Show success message
            string successMessage = $"Transaction #{lblTransactionNumber.Text}\n" +
                                  $"Document Type: {documentType}\n" +
                                  $"Total: PLN {total:N2}\n" +
                                  $"Payment Method: {paymentMethod}\n" +
                                  $"Items: {_cartItems.Count}";

            // Add invoice details if invoice is selected
            if (cmbDocumentType.SelectedIndex == 1)
            {
                successMessage += $"\n\nInvoice To:\n" +
                                $"{txtCompanyName.Text}\n" +
                                $"NIP: {txtNIP.Text}\n" +
                                $"{txtAddress.Text}\n" +
                                $"{txtPostalCode.Text} {txtCity.Text}";
            }

            if (rbCash.IsChecked == true && decimal.TryParse(txtAmountReceived.Text, out decimal received))
            {
                decimal change = received - total;
                successMessage += $"\n\nAmount Received: PLN {received:N2}\n" +
                                $"Change: PLN {change:N2}";
            }

            lblSuccessMessage.Text = successMessage;

            // TODO: Save to database
            // SaveTransaction();

            // Show success modal
            successModalOverlay.Visibility = Visibility.Visible;
        }

        private void NewSaleAfterSuccess_Click(object sender, RoutedEventArgs e)
        {
            // Close modal
            successModalOverlay.Visibility = Visibility.Collapsed;

            // Reset everything
            _cartItems.Clear();
            txtSearchProduct.Text = "";
            txtAmountReceived.Text = "";
            rbCash.IsChecked = true;
            cmbDocumentType.SelectedIndex = 0; // Reset to Receipt
            changeDisplay.Visibility = Visibility.Collapsed;

            // Clear invoice fields
            ClearInvoiceFields();

            // Generate new transaction number
            lblTransactionNumber.Text = GenerateTransactionNumber();
            lblTransactionDate.Text = DateTime.Now.ToString("dd MMM yyyy, HH:mm");

            // Focus on search
            txtSearchProduct.Focus();
        }

        private void CancelSale_Click(object sender, RoutedEventArgs e)
        {
            if (_cartItems.Count > 0)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to cancel this sale?\nAll items will be removed from the cart.",
                    "Cancel Sale",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                    return;
            }

            // Reset everything
            _cartItems.Clear();
            txtSearchProduct.Text = "";
            txtAmountReceived.Text = "";
            rbCash.IsChecked = true;
            cmbDocumentType.SelectedIndex = 0; // Reset to Receipt
            changeDisplay.Visibility = Visibility.Collapsed;
            ClearInvoiceFields();
            txtSearchProduct.Focus();
        }

        private void ClearInvoiceFields()
        {
            if (txtCompanyName != null) txtCompanyName.Text = "";
            if (txtNIP != null) txtNIP.Text = "";
            if (txtAddress != null) txtAddress.Text = "";
            if (txtCity != null) txtCity.Text = "";
            if (txtPostalCode != null) txtPostalCode.Text = "";
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

        // Empty method - we're already on this page
        private void NewSale_Click(object sender, RoutedEventArgs e) { }

        private void SalesHistory_Click(object sender, RoutedEventArgs e)
        {
            SalesHistoryWindow salesHistoryWindow = new SalesHistoryWindow(_username, _password, _fullName, _role);
            salesHistoryWindow.Show();
            this.Close();
        }

        private void Invoices_Click(object sender, RoutedEventArgs e)
        {
            InvoicesWindow invoicesWindow = new InvoicesWindow(_username, _password, _fullName, _role);
            invoicesWindow.Show();
            this.Close();
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

    // ===== DATA MODELS =====
    public class CartItem : INotifyPropertyChanged
    {
        private int _quantity;

        public string SKU { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public string ImagePath { get; set; }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        public decimal Subtotal => UnitPrice * Quantity;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Product
    {
        public string SKU { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public string ImagePath { get; set; }
        public int StockQuantity { get; set; }
    }
}