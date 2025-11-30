using CycleDesk.Services;
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
using System.Windows.Threading;

namespace CycleDesk
{
    public partial class NewSaleWindow : Window
    {
        // ===== FIELDS =====
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;
        private int _userId;

        // Services
        private SaleService _saleService;

        // Cart
        private ObservableCollection<CartItemViewModel> _cartItems;
        private List<SaleProductDto> _availableProducts;

        // Search
        private DispatcherTimer _searchTimer;

        public NewSaleWindow(string username, string password, string fullName, string role)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _fullName = fullName;
            _role = role;

            // Initialize services
            _saleService = new SaleService();
            _userId = _saleService.GetUserIdByUsername(username);

            // Initialize cart
            _cartItems = new ObservableCollection<CartItemViewModel>();
            _cartItems.CollectionChanged += CartItems_CollectionChanged;
            cartItemsList.ItemsSource = _cartItems;

            // Initialize search timer (debounce)
            _searchTimer = new DispatcherTimer();
            _searchTimer.Interval = TimeSpan.FromMilliseconds(300);
            _searchTimer.Tick += SearchTimer_Tick;

            // Initialize SideMenuControl
            sideMenu.Initialize(fullName, role);
            sideMenu.SetActiveMenu("Sales", "NewSale");

            // Connect menu events
            ConnectMenuEvents();

            // Set transaction info
            lblCashierName.Text = fullName;
            lblTransactionNumber.Text = _saleService.GenerateSaleNumber();
            lblTransactionDate.Text = DateTime.Now.ToString("dd MMM yyyy, HH:mm");

            // Load products
            LoadProducts();

            // Focus on search box
            txtSearchProduct.Focus();
        }

        // ===== DATA LOADING =====
        private void LoadProducts()
        {
            try
            {
                _availableProducts = _saleService.GetAvailableProducts();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading products: {ex.Message}");
                _availableProducts = new List<SaleProductDto>();
            }
        }

        // ===== SEARCH =====
        private bool _isUpdatingSearch = false;

        private void SearchProduct_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearchProduct.Text.StartsWith("🔍"))
            {
                txtSearchProduct.Text = "";
                txtSearchProduct.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF212529"));
            }
        }

        private void SearchProduct_LostFocus(object sender, RoutedEventArgs e)
        {
            // Delay to allow click on popup items
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (searchResultsPopup != null && !searchResultsPopup.IsKeyboardFocusWithin && string.IsNullOrWhiteSpace(txtSearchProduct.Text))
                {
                    txtSearchProduct.Text = "🔍 Scan barcode or search product (SKU, name)...";
                    txtSearchProduct.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D"));
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void SearchProduct_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingSearch)
                return;

            if (txtSearchProduct.Text.StartsWith("🔍"))
            {
                if (searchResultsPopup != null)
                    searchResultsPopup.IsOpen = false;
                return;
            }

            // Reset and start timer for debounce
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            _searchTimer.Stop();

            // Safety check - ensure controls are initialized
            if (searchResultsPopup == null || searchResultsList == null || noResultsMessage == null)
                return;

            string searchText = txtSearchProduct.Text.Trim();
            
            if (string.IsNullOrEmpty(searchText))
            {
                searchResultsPopup.IsOpen = false;
                return;
            }

            // Try exact SKU match first (barcode scan)
            var exactProduct = _saleService.GetProductBySKU(searchText);
            if (exactProduct != null)
            {
                AddProductToCart(exactProduct);
                ClearSearchBox();
                searchResultsPopup.IsOpen = false;
                return;
            }

            // Search products
            var results = _saleService.SearchProducts(searchText);
            
            if (results.Count > 0)
            {
                searchResultsList.ItemsSource = results;
                searchResultsList.Visibility = Visibility.Visible;
                noResultsMessage.Visibility = Visibility.Collapsed;
                searchResultsPopup.IsOpen = true;
                
                // Select first item for keyboard navigation
                if (results.Count > 0)
                {
                    searchResultsList.SelectedIndex = 0;
                }
            }
            else
            {
                searchResultsList.ItemsSource = null;
                searchResultsList.Visibility = Visibility.Collapsed;
                noResultsMessage.Visibility = Visibility.Visible;
                searchResultsPopup.IsOpen = true;
            }
        }

        private void SearchProduct_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (searchResultsPopup == null || !searchResultsPopup.IsOpen)
                return;

            if (e.Key == Key.Down)
            {
                // Move focus to list
                if (searchResultsList.Items.Count > 0)
                {
                    searchResultsList.Focus();
                    if (searchResultsList.SelectedIndex < 0)
                        searchResultsList.SelectedIndex = 0;
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                // Move to last item
                if (searchResultsList.Items.Count > 0)
                {
                    searchResultsList.Focus();
                    searchResultsList.SelectedIndex = searchResultsList.Items.Count - 1;
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                // Select first result if popup is open
                if (searchResultsList.Items.Count > 0)
                {
                    var selectedProduct = searchResultsList.SelectedItem as SaleProductDto 
                        ?? searchResultsList.Items[0] as SaleProductDto;
                    if (selectedProduct != null)
                    {
                        SelectProductFromSearch(selectedProduct);
                    }
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                searchResultsPopup.IsOpen = false;
                e.Handled = true;
            }
        }

        private void SearchResultsList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var selectedProduct = searchResultsList?.SelectedItem as SaleProductDto;
                if (selectedProduct != null)
                {
                    SelectProductFromSearch(selectedProduct);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                if (searchResultsPopup != null)
                    searchResultsPopup.IsOpen = false;
                txtSearchProduct.Focus();
                e.Handled = true;
            }
        }

        private void SearchResultsList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var selectedProduct = searchResultsList?.SelectedItem as SaleProductDto;
            if (selectedProduct != null)
            {
                SelectProductFromSearch(selectedProduct);
            }
        }

        private void SelectProductFromSearch(SaleProductDto product)
        {
            if (product == null)
                return;

            AddProductToCart(product);
            ClearSearchBox();
            if (searchResultsPopup != null)
                searchResultsPopup.IsOpen = false;
            txtSearchProduct.Focus();
        }

        private void ClearSearchBox()
        {
            _isUpdatingSearch = true;
            txtSearchProduct.Text = "";
            _isUpdatingSearch = false;
        }

        // ===== CART MANAGEMENT =====
        private void AddProductToCart(SaleProductDto product)
        {
            if (product == null)
                return;

            if (product.StockQuantity <= 0)
            {
                MessageBox.Show($"'{product.ProductName}' is out of stock.",
                    "Out of Stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if already in cart
            var existingItem = _cartItems.FirstOrDefault(i => i.ProductId == product.ProductId);
            if (existingItem != null)
            {
                if (existingItem.Quantity < existingItem.MaxQuantity)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    MessageBox.Show($"Cannot add more. Maximum stock available: {existingItem.MaxQuantity}",
                        "Stock Limit", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                var newItem = new CartItemViewModel
                {
                    ProductId = product.ProductId,
                    SKU = product.SKU,
                    ProductName = product.ProductName,
                    UnitPrice = product.UnitPrice,
                    VATRate = product.VATRate,
                    Quantity = 1,
                    MaxQuantity = product.StockQuantity
                };
                _cartItems.Add(newItem);
            }

            UpdateSummary();
        }

        private void CartItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateCartVisibility();
            UpdateSummary();
        }

        private void UpdateCartVisibility()
        {
            bool hasItems = _cartItems.Count > 0;
            emptyCartState.Visibility = hasItems ? Visibility.Collapsed : Visibility.Visible;
            cartItemsList.Visibility = hasItems ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateSummary()
        {
            int itemCount = _cartItems.Sum(i => i.Quantity);
            decimal subtotal = _cartItems.Sum(i => i.NetAmount);
            decimal tax = _cartItems.Sum(i => i.VATAmount);
            decimal total = _cartItems.Sum(i => i.GrossAmount);

            lblItemCount.Text = itemCount.ToString();
            lblSubtotal.Text = $"PLN {subtotal:N2}";
            lblTax.Text = $"PLN {tax:N2}";
            lblTotal.Text = $"PLN {total:N2}";

            // Update payment validation
            ValidatePayment();
            CalculateChange();
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as CartItemViewModel;

            if (item != null && item.Quantity > 1)
            {
                item.Quantity--;
                UpdateSummary();
            }
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as CartItemViewModel;

            if (item != null && item.Quantity < item.MaxQuantity)
            {
                item.Quantity++;
                UpdateSummary();
            }
            else if (item != null)
            {
                MessageBox.Show($"Cannot add more. Maximum stock available: {item.MaxQuantity}",
                    "Stock Limit", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as CartItemViewModel;

            if (item != null)
            {
                _cartItems.Remove(item);
            }
        }

        private void ClearCart_Click(object sender, RoutedEventArgs e)
        {
            if (_cartItems.Count == 0)
                return;

            var result = MessageBox.Show("Are you sure you want to clear the cart?",
                "Clear Cart", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _cartItems.Clear();
            }
        }

        // ===== DOCUMENT TYPE =====
        private void DocumentType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDocumentType == null || invoiceDetailsPanel == null)
                return;

            bool isInvoice = cmbDocumentType.SelectedIndex == 1;
            invoiceDetailsPanel.Visibility = isInvoice ? Visibility.Visible : Visibility.Collapsed;
            ValidatePayment();
        }

        // ===== PAYMENT =====
        private void PaymentMethod_Changed(object sender, RoutedEventArgs e)
        {
            if (cashPaymentSection == null || cardPaymentSection == null)
                return;

            bool isCash = rbCash.IsChecked == true;
            cashPaymentSection.Visibility = isCash ? Visibility.Visible : Visibility.Collapsed;
            cardPaymentSection.Visibility = isCash ? Visibility.Collapsed : Visibility.Visible;

            ValidatePayment();
        }

        private void AmountReceived_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateChange();
            ValidatePayment();
        }

        private void CalculateChange()
        {
            if (rbCash.IsChecked != true || changeDisplay == null)
                return;

            decimal total = _cartItems.Sum(item => item.GrossAmount);

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
            bool isValid = false;

            // Must have items in cart
            if (_cartItems.Count == 0)
            {
                btnCompleteSale.IsEnabled = false;
                return false;
            }

            // If invoice is selected, validate invoice fields
            if (cmbDocumentType?.SelectedIndex == 1)
            {
                if (string.IsNullOrWhiteSpace(txtCompanyName?.Text) ||
                    string.IsNullOrWhiteSpace(txtNIP?.Text) ||
                    string.IsNullOrWhiteSpace(txtAddress?.Text) ||
                    string.IsNullOrWhiteSpace(txtCity?.Text) ||
                    string.IsNullOrWhiteSpace(txtPostalCode?.Text))
                {
                    btnCompleteSale.IsEnabled = false;
                    return false;
                }
            }

            // Card payment - valid if cart has items and invoice data (if needed) is filled
            if (rbCard.IsChecked == true)
            {
                isValid = true;
            }
            // Cash payment - must have sufficient amount
            else if (rbCash.IsChecked == true)
            {
                decimal total = _cartItems.Sum(item => item.GrossAmount);

                if (decimal.TryParse(txtAmountReceived.Text, out decimal amountReceived))
                {
                    isValid = amountReceived >= total;
                }
            }

            btnCompleteSale.IsEnabled = isValid;
            return isValid;
        }

        // ===== COMPLETE SALE =====
        private void CompleteSale_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidatePayment())
            {
                MessageBox.Show("Please ensure all payment information is correct.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Prepare cart items for service
            var cartItemDtos = _cartItems.Select(i => new CartItemDto
            {
                ProductId = i.ProductId,
                SKU = i.SKU,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                VATRate = i.VATRate,
                Quantity = i.Quantity,
                MaxQuantity = i.MaxQuantity
            }).ToList();

            decimal total = _cartItems.Sum(i => i.GrossAmount);
            decimal paidAmount = 0;
            decimal changeAmount = 0;

            if (rbCash.IsChecked == true && decimal.TryParse(txtAmountReceived.Text, out decimal received))
            {
                paidAmount = received;
                changeAmount = received - total;
            }
            else
            {
                paidAmount = total;
            }

            var request = new CompleteSaleRequest
            {
                Items = cartItemDtos,
                PaymentMethod = rbCash.IsChecked == true ? "Cash" : "Card",
                PaidAmount = paidAmount,
                ChangeAmount = changeAmount > 0 ? changeAmount : 0,
                DocumentType = cmbDocumentType?.SelectedIndex == 1 ? "Invoice" : "Receipt",
                CompanyName = txtCompanyName?.Text,
                NIP = txtNIP?.Text,
                Address = txtAddress?.Text,
                City = txtCity?.Text,
                PostalCode = txtPostalCode?.Text,
                SoldByUserId = _userId > 0 ? _userId : 1,
                Notes = ""
            };

            // Process sale
            var result = _saleService.CompleteSale(request);

            if (result.Success)
            {
                // Build success message
                string successMessage = $"Transaction #{result.SaleNumber}\n" +
                                       $"Document Type: {request.DocumentType}\n" +
                                       $"Total: PLN {total:N2}\n" +
                                       $"Payment Method: {request.PaymentMethod}\n" +
                                       $"Items: {_cartItems.Count}";

                if (request.DocumentType == "Invoice" && !string.IsNullOrEmpty(result.InvoiceNumber))
                {
                    successMessage += $"\n\nInvoice: {result.InvoiceNumber}\n" +
                                     $"Company: {request.CompanyName}\n" +
                                     $"NIP: {request.NIP}";
                }

                if (request.PaymentMethod == "Cash")
                {
                    successMessage += $"\n\nAmount Received: PLN {paidAmount:N2}\n" +
                                     $"Change: PLN {changeAmount:N2}";
                }

                lblSuccessMessage.Text = successMessage;
                successModalOverlay.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show(result.Message, "Sale Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewSaleAfterSuccess_Click(object sender, RoutedEventArgs e)
        {
            // Close modal
            successModalOverlay.Visibility = Visibility.Collapsed;

            // Reset everything
            _cartItems.Clear();
            txtSearchProduct.Text = "🔍 Scan barcode or search product (SKU, name)...";
            txtSearchProduct.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D"));
            txtAmountReceived.Text = "";
            rbCash.IsChecked = true;
            
            if (cmbDocumentType != null)
                cmbDocumentType.SelectedIndex = 0;
                
            changeDisplay.Visibility = Visibility.Collapsed;

            // Clear invoice fields
            ClearInvoiceFields();

            // Generate new transaction number
            lblTransactionNumber.Text = _saleService.GenerateSaleNumber();
            lblTransactionDate.Text = DateTime.Now.ToString("dd MMM yyyy, HH:mm");

            // Reload products (refresh stock)
            LoadProducts();

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
            txtSearchProduct.Text = "🔍 Scan barcode or search product (SKU, name)...";
            txtSearchProduct.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D"));
            txtAmountReceived.Text = "";
            rbCash.IsChecked = true;
            
            if (cmbDocumentType != null)
                cmbDocumentType.SelectedIndex = 0;
                
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

        // ===== KEYBOARD SHORTCUTS =====
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // F1 - Focus on search
            if (e.Key == Key.F1)
            {
                txtSearchProduct.Focus();
                if (txtSearchProduct.Text.StartsWith("🔍"))
                {
                    txtSearchProduct.Text = "";
                    txtSearchProduct.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF212529"));
                }
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
                // Don't trigger if in search box
                if (txtSearchProduct.IsFocused || txtAmountReceived.IsFocused)
                    return;

                CompleteSale_Click(sender, new RoutedEventArgs());
                e.Handled = true;
            }
        }

        // ===== MENU EVENTS CONNECTION =====
        private void ConnectMenuEvents()
        {
            sideMenu.DashboardClicked += (s, e) =>
            {
                new MainDashboardWindow(_username, _password, _fullName, _role).Show();
                Close();
            };

            sideMenu.ProductsClicked += (s, e) =>
            {
                new ProductsWindow(_username, _password, _fullName, _role).Show();
                Close();
            };

            sideMenu.CategoriesClicked += (s, e) =>
            {
                new CategoriesWindow(_username, _password, _fullName, _role).Show();
                Close();
            };

            sideMenu.InventoryStatusClicked += (s, e) =>
            {
                new InventoryStatusWindow(_username, _password, _fullName, _role).Show();
                Close();
            };

            sideMenu.GoodsReceiptClicked += (s, e) =>
            {
                new GoodsReceiptWindow(_username, _password, _fullName, _role).Show();
                Close();
            };

            sideMenu.SuppliersClicked += (s, e) =>
            {
                new SuppliersWindow(_username, _password, _fullName, _role).Show();
                Close();
            };

            sideMenu.NewSaleClicked += (s, e) =>
            {
                // Already on this page
            };

            sideMenu.SalesHistoryClicked += (s, e) =>
            {
                new SalesHistoryWindow(_username, _password, _fullName, _role).Show();
                Close();
            };

            

            sideMenu.SalesReportsClicked += (s, e) =>
            {
                new SalesReportsWindow(_username, _password, _fullName, _role).Show();
                Close();
            };

            sideMenu.InventoryReportsClicked += (s, e) =>
            {
                new InventoryReportsWindow(_username, _password, _fullName, _role).Show();
                Close();
            };

            sideMenu.ProductsToOrderClicked += (s, e) =>
            {
                new ProductsToOrderWindow(_username, _password, _fullName, _role).Show();
                Close();
            };

            sideMenu.UsersClicked += (s, e) =>
            {
                new UsersWindow(_username, _password, _fullName, _role).Show();
                Close();
            };

            sideMenu.SettingsClicked += (s, e) =>
            {
                new SettingsWindow(_username, _password, _fullName, _role).Show();
                Close();
            };

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
    }

    // ===== VIEW MODEL FOR CART ITEM =====
    public class CartItemViewModel : INotifyPropertyChanged
    {
        private int _quantity;

        public int ProductId { get; set; }
        public string SKU { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal VATRate { get; set; }
        public int MaxQuantity { get; set; }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(NetAmount));
                OnPropertyChanged(nameof(VATAmount));
                OnPropertyChanged(nameof(GrossAmount));
                OnPropertyChanged(nameof(Subtotal)); // For XAML binding
            }
        }

        public decimal NetAmount => UnitPrice * Quantity;
        public decimal VATAmount => NetAmount * (VATRate / 100);
        public decimal GrossAmount => NetAmount + VATAmount;
        public decimal Subtotal => GrossAmount; // Alias for XAML binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
