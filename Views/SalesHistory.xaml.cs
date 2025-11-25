using CycleDesk.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace CycleDesk
{
    public partial class SalesHistoryWindow : Window
    {
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;
        private ObservableCollection<TransactionRecord> _allTransactions;
        private ObservableCollection<TransactionRecord> _filteredTransactions;

        public SalesHistoryWindow(string username, string password, string fullName, string role)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _fullName = fullName;
            _role = role;

            // Set user data in UI
            lblUserFullName.Text = fullName;
            lblUserRole.Text = role;

            // Set Sales as active button and expand submenu
            SetActiveButton(btnSales);
            submenuSales.Visibility = Visibility.Visible;

            // Initialize transactions
            LoadMockTransactions();
            ApplyFilters();
        }

        // ===== DATA MODELS =====
        public class TransactionRecord
        {
            public string TransactionNumber { get; set; }
            public DateTime DateTime { get; set; }
            public string DateTimeFormatted => DateTime.ToString("dd MMM yyyy, HH:mm");
            public int ItemsCount { get; set; }
            public string CashierName { get; set; }
            public string DocumentType { get; set; } // "Receipt" or "Invoice"
            public SolidColorBrush DocumentTypeColor
            {
                get
                {
                    return DocumentType == "Receipt"
                        ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D"))
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFC107"));
                }
            }
            public string PaymentMethod { get; set; }
            public SolidColorBrush PaymentMethodColor
            {
                get
                {
                    return PaymentMethod == "Cash"
                        ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF28A745"))
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF17A2B8"));
                }
            }
            public decimal Total { get; set; }
            public List<TransactionItem> Items { get; set; }
        }

        public class TransactionItem
        {
            public string ProductName { get; set; }
            public string SKU { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Subtotal => Quantity * UnitPrice;
        }

        // ===== MOCK DATA =====
        private void LoadMockTransactions()
        {
            _allTransactions = new ObservableCollection<TransactionRecord>
            {
                new TransactionRecord
                {
                    TransactionNumber = "#TRX202511161443522",
                    DateTime = new DateTime(2025, 11, 16, 14, 30, 0),
                    ItemsCount = 3,
                    CashierName = "Admin User",
                    DocumentType = "Receipt",
                    PaymentMethod = "Cash",
                    Total = 3300.08m,
                    Items = new List<TransactionItem>
                    {
                        new TransactionItem { ProductName = "Mountain Bike Pro X", SKU = "BIKE-001", Quantity = 1, UnitPrice = 2499.00m },
                        new TransactionItem { ProductName = "Safety Helmet Pro", SKU = "HELM-001", Quantity = 1, UnitPrice = 149.00m },
                        new TransactionItem { ProductName = "LED Light Set", SKU = "LIGH-001", Quantity = 1, UnitPrice = 14.99m }
                    }
                },
                new TransactionRecord
                {
                    TransactionNumber = "#TRX202511161301245",
                    DateTime = new DateTime(2025, 11, 16, 13, 15, 0),
                    ItemsCount = 2,
                    CashierName = "John Smith",
                    DocumentType = "Invoice",
                    PaymentMethod = "Card",
                    Total = 1899.00m,
                    Items = new List<TransactionItem>
                    {
                        new TransactionItem { ProductName = "Road Bike Elite", SKU = "BIKE-002", Quantity = 1, UnitPrice = 1799.00m },
                        new TransactionItem { ProductName = "Water Bottle", SKU = "ACCS-004", Quantity = 2, UnitPrice = 50.00m }
                    }
                },
                new TransactionRecord
                {
                    TransactionNumber = "#TRX202511151745891",
                    DateTime = new DateTime(2025, 11, 15, 17, 45, 0),
                    ItemsCount = 5,
                    CashierName = "Admin User",
                    DocumentType = "Receipt",
                    PaymentMethod = "Cash",
                    Total = 425.50m,
                    Items = new List<TransactionItem>
                    {
                        new TransactionItem { ProductName = "Bike Lock Premium", SKU = "ACCS-001", Quantity = 2, UnitPrice = 89.99m },
                        new TransactionItem { ProductName = "Repair Kit", SKU = "TOOL-001", Quantity = 1, UnitPrice = 45.00m },
                        new TransactionItem { ProductName = "Chain Lubricant", SKU = "MAIN-001", Quantity = 3, UnitPrice = 12.00m },
                        new TransactionItem { ProductName = "Tire Pump", SKU = "TOOL-002", Quantity = 1, UnitPrice = 79.00m }
                    }
                },
                new TransactionRecord
                {
                    TransactionNumber = "#TRX202511151342167",
                    DateTime = new DateTime(2025, 11, 15, 13, 42, 0),
                    ItemsCount = 1,
                    CashierName = "Sarah Johnson",
                    DocumentType = "Invoice",
                    PaymentMethod = "Card",
                    Total = 3499.00m,
                    Items = new List<TransactionItem>
                    {
                        new TransactionItem { ProductName = "Electric Bike Ultra", SKU = "BIKE-003", Quantity = 1, UnitPrice = 3499.00m }
                    }
                },
                new TransactionRecord
                {
                    TransactionNumber = "#TRX202511141028443",
                    DateTime = new DateTime(2025, 11, 14, 10, 28, 0),
                    ItemsCount = 4,
                    CashierName = "Admin User",
                    DocumentType = "Receipt",
                    PaymentMethod = "Cash",
                    Total = 678.95m,
                    Items = new List<TransactionItem>
                    {
                        new TransactionItem { ProductName = "Cycling Gloves Pro", SKU = "APPR-001", Quantity = 2, UnitPrice = 49.99m },
                        new TransactionItem { ProductName = "Bike Computer GPS", SKU = "ELEC-001", Quantity = 1, UnitPrice = 299.00m },
                        new TransactionItem { ProductName = "Energy Bars Pack", SKU = "FOOD-001", Quantity = 3, UnitPrice = 15.99m },
                        new TransactionItem { ProductName = "Hydration Pack", SKU = "ACCS-005", Quantity = 1, UnitPrice = 199.00m }
                    }
                },
                new TransactionRecord
                {
                    TransactionNumber = "#TRX202511131612334",
                    DateTime = new DateTime(2025, 11, 13, 16, 12, 0),
                    ItemsCount = 2,
                    CashierName = "John Smith",
                    DocumentType = "Invoice",
                    PaymentMethod = "Card",
                    Total = 2899.00m,
                    Items = new List<TransactionItem>
                    {
                        new TransactionItem { ProductName = "Mountain Bike Pro X", SKU = "BIKE-001", Quantity = 1, UnitPrice = 2499.00m },
                        new TransactionItem { ProductName = "Bike Stand", SKU = "ACCS-006", Quantity = 1, UnitPrice = 400.00m }
                    }
                }
            };

            _filteredTransactions = new ObservableCollection<TransactionRecord>(_allTransactions);
        }

        // ===== FILTERING & SEARCH =====
        private void ApplyFilters()
        {
            var filtered = _allTransactions.AsEnumerable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                string searchTerm = txtSearch.Text.ToLower();
                filtered = filtered.Where(t =>
                    t.TransactionNumber.ToLower().Contains(searchTerm) ||
                    t.CashierName.ToLower().Contains(searchTerm));
            }

            // Date filters
            if (dpDateFrom.SelectedDate.HasValue)
            {
                filtered = filtered.Where(t => t.DateTime.Date >= dpDateFrom.SelectedDate.Value.Date);
            }

            if (dpDateTo.SelectedDate.HasValue)
            {
                filtered = filtered.Where(t => t.DateTime.Date <= dpDateTo.SelectedDate.Value.Date);
            }

            // Payment method filter
            if (cmbPaymentMethod.SelectedIndex > 1) // Skip "Payment" and "All"
            {
                string selectedPayment = (cmbPaymentMethod.SelectedItem as ComboBoxItem)?.Content.ToString();
                filtered = filtered.Where(t => t.PaymentMethod == selectedPayment);
            }

            // Document type filter
            if (cmbDocumentType.SelectedIndex > 1) // Skip "Type" and "All"
            {
                string selectedDocType = (cmbDocumentType.SelectedItem as ComboBoxItem)?.Content.ToString();
                filtered = filtered.Where(t => t.DocumentType == selectedDocType);
            }

            _filteredTransactions = new ObservableCollection<TransactionRecord>(filtered.OrderByDescending(t => t.DateTime));
            transactionsList.ItemsSource = _filteredTransactions;

            UpdateSummary();
        }

        private void UpdateSummary()
        {
            lblTotalTransactions.Text = $"Total Transactions: {_filteredTransactions.Count}";
            lblTotalRevenue.Text = $"PLN {_filteredTransactions.Sum(t => t.Total):N2}";
        }

        // ===== SEARCH HANDLERS =====
        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        // ===== FILTER HANDLERS =====
        private void DateFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (transactionsList != null) // Check if controls are initialized
            {
                ApplyFilters();
            }
        }

        private void PaymentMethodFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (transactionsList != null)
            {
                ApplyFilters();
            }
        }

        private void DocumentTypeFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (transactionsList != null)
            {
                ApplyFilters();
            }
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            dpDateFrom.SelectedDate = null;
            dpDateTo.SelectedDate = null;
            cmbPaymentMethod.SelectedIndex = 0;
            cmbDocumentType.SelectedIndex = 0;
            txtSearch.Text = "";
            ApplyFilters();
        }

        // ===== CLICKABLE ROW LOGIC =====
        private void TransactionRow_Click(object sender, MouseButtonEventArgs e)
        {
            Border clickedRow = sender as Border;
            if (clickedRow == null) return;

            // Get transaction data from DataContext
            TransactionRecord transaction = clickedRow.DataContext as TransactionRecord;
            if (transaction == null) return;

            // Populate popup with transaction details
            popupTransactionNumber.Text = transaction.TransactionNumber;
            popupDateTime.Text = transaction.DateTimeFormatted;
            popupCashier.Text = transaction.CashierName;
            popupPaymentMethod.Text = transaction.PaymentMethod;

            // Set payment badge color
            popupPaymentBadge.Background = transaction.PaymentMethodColor;

            // Set items list
            popupItemsList.ItemsSource = transaction.Items;

            // Set total
            popupTotal.Text = $"PLN {transaction.Total:N2}";

            // Show popup
            transactionDetailsPopup.PlacementTarget = clickedRow;
            transactionDetailsPopup.Placement = System.Windows.Controls.Primitives.PlacementMode.Center;
            transactionDetailsPopup.IsOpen = true;
        }

        private void TransactionRow_MouseLeave(object sender, MouseEventArgs e)
        {
            // Not needed for this popup behavior
        }

        private void ViewTransaction_Click(object sender, RoutedEventArgs e)
        {
            // Close popup
            transactionDetailsPopup.IsOpen = false;

            // TODO: Open full transaction details window
            MessageBox.Show("View full transaction details - to be implemented", "View Transaction", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PrintTransaction_Click(object sender, RoutedEventArgs e)
        {
            // Close popup
            transactionDetailsPopup.IsOpen = false;

            // TODO: Print transaction receipt/invoice
            MessageBox.Show("Print transaction - to be implemented", "Print", MessageBoxButton.OK, MessageBoxImage.Information);
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
            InventoryStatusWindow inventoryWindow = new InventoryStatusWindow(_username, _password, _fullName, _role);
            inventoryWindow.Show();
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

        // Already on this page
        private void SalesHistory_Click(object sender, RoutedEventArgs e) { }

        private void Invoices_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Invoices view - coming soon!", "Info",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SalesReports_Click(object sender, RoutedEventArgs e)
        {
            SalesReportsWindow salesReportsWindow = new SalesReportsWindow(_username, _password, _fullName, _role);
            salesReportsWindow.Show();
            this.Close();
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
}