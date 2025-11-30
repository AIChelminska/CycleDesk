using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CycleDesk.Services;

namespace CycleDesk
{
    public partial class SalesHistoryWindow : Window
    {
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;
        private SaleService _saleService;
        private ObservableCollection<TransactionRecord> _allTransactions;
        private ObservableCollection<TransactionRecord> _filteredTransactions;

        // Pagination
        private const int PageSize = 30;
        private int _currentPage = 1;
        private int _totalPages = 1;

        public SalesHistoryWindow(string username, string password, string fullName, string role)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _fullName = fullName;
            _role = role;
            _saleService = new SaleService();

            // Inicjalizuj SideMenuControl
            sideMenu.Initialize(fullName, role);
            sideMenu.SetActiveMenu("Sales", "SalesHistory");

            // Podłącz eventy menu
            ConnectMenuEvents();

            // Load transactions from database
            LoadTransactions();
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
                new NewSaleWindow(_username, _password, _fullName, _role).Show();
                Close();
            };

            sideMenu.SalesHistoryClicked += (s, e) =>
            {
                // Already on this page - refresh data
                LoadTransactions();
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

        // ===== DATA MODELS =====
        public class TransactionRecord
        {
            public int SaleId { get; set; }
            public string TransactionNumber { get; set; }
            public DateTime DateTime { get; set; }
            public string DateTimeFormatted => DateTime.ToString("dd MMM yyyy, HH:mm");
            public int ItemsCount { get; set; }
            public string CashierName { get; set; }
            public string DocumentType { get; set; } // "Receipt" or "Invoice"
            public string Status { get; set; }
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
            public SolidColorBrush StatusColor
            {
                get
                {
                    return Status == "Completed"
                        ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF28A745"))
                        : Status == "Cancelled"
                            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDC3545"))
                            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFC107"));
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

        // ===== LOAD DATA FROM DATABASE =====
        private void LoadTransactions()
        {
            try
            {
                var salesHistory = _saleService.GetSalesHistory();

                _allTransactions = new ObservableCollection<TransactionRecord>(
                    salesHistory.Select(s => new TransactionRecord
                    {
                        SaleId = s.SaleId,
                        TransactionNumber = s.SaleNumber,
                        DateTime = s.SaleDate,
                        ItemsCount = s.ItemsCount,
                        CashierName = s.CashierName,
                        DocumentType = s.DocumentType,
                        PaymentMethod = s.PaymentMethod,
                        Status = s.Status,
                        Total = s.TotalAmount,
                        Items = new List<TransactionItem>() // Loaded on demand
                    })
                );

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sales history: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                _allTransactions = new ObservableCollection<TransactionRecord>();
                _filteredTransactions = new ObservableCollection<TransactionRecord>();
            }
        }

        private void LoadTransactionItems(TransactionRecord transaction)
        {
            if (transaction.Items == null || transaction.Items.Count == 0)
            {
                var items = _saleService.GetSaleItems(transaction.SaleId);
                transaction.Items = items.Select(i => new TransactionItem
                {
                    ProductName = i.ProductName,
                    SKU = i.SKU,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList();
            }
        }

        // ===== FILTERING & SEARCH =====
        private void ApplyFilters()
        {
            if (_allTransactions == null) return;

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
            
            // Reset to first page when filters change
            _currentPage = 1;
            UpdatePagination();
        }

        private void UpdatePagination()
        {
            if (_filteredTransactions == null || _filteredTransactions.Count == 0)
            {
                _totalPages = 1;
                _currentPage = 1;
                transactionsList.ItemsSource = new ObservableCollection<TransactionRecord>();
            }
            else
            {
                _totalPages = (int)Math.Ceiling((double)_filteredTransactions.Count / PageSize);
                if (_currentPage > _totalPages) _currentPage = _totalPages;
                if (_currentPage < 1) _currentPage = 1;

                var pagedData = _filteredTransactions
                    .Skip((_currentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                transactionsList.ItemsSource = new ObservableCollection<TransactionRecord>(pagedData);
            }

            // Update UI
            lblPageInfo.Text = $"Page {_currentPage} of {_totalPages}";
            btnFirstPage.IsEnabled = _currentPage > 1;
            btnPrevPage.IsEnabled = _currentPage > 1;
            btnNextPage.IsEnabled = _currentPage < _totalPages;
            btnLastPage.IsEnabled = _currentPage < _totalPages;

            UpdateSummary();
        }

        // ===== PAGINATION HANDLERS =====
        private void FirstPage_Click(object sender, RoutedEventArgs e)
        {
            _currentPage = 1;
            UpdatePagination();
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                UpdatePagination();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                UpdatePagination();
            }
        }

        private void LastPage_Click(object sender, RoutedEventArgs e)
        {
            _currentPage = _totalPages;
            UpdatePagination();
        }

        private void UpdateSummary()
        {
            if (_filteredTransactions == null)
            {
                lblTotalTransactions.Text = "Total Transactions: 0";
                lblTotalRevenue.Text = "PLN 0.00";
                return;
            }

            // Only count completed transactions for revenue (from ALL filtered, not just current page)
            var completedTransactions = _filteredTransactions.Where(t => t.Status == "Completed").ToList();
            
            lblTotalTransactions.Text = $"Total Transactions: {_filteredTransactions.Count} (showing {Math.Min(PageSize, _filteredTransactions.Count - (_currentPage - 1) * PageSize)})";
            lblTotalRevenue.Text = $"PLN {completedTransactions.Sum(t => t.Total):N2}";
        }

        // ===== SEARCH HANDLERS =====
        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        // ===== FILTER HANDLERS =====
        private void DateFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (transactionsList != null)
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

            // Load items if not already loaded
            LoadTransactionItems(transaction);

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
            MessageBox.Show("View full transaction details - to be implemented", "View Transaction", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PrintTransaction_Click(object sender, RoutedEventArgs e)
        {
            // Close popup
            transactionDetailsPopup.IsOpen = false;

            // TODO: Print transaction receipt/invoice
            MessageBox.Show("Print transaction - to be implemented", "Print", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
