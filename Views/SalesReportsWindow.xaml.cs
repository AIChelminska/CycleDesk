using CycleDesk.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CycleDesk
{
    public partial class SalesReportsWindow : Window
    {
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;

        // Collections for mock data
        private List<SaleTransaction> _allTransactions;

        public SalesReportsWindow(string username, string password, string fullName, string role)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _fullName = fullName;
            _role = role;

            // Set user data in UI
            lblUserFullName.Text = fullName;
            lblUserRole.Text = role;

            // Set Reports as active button and expand submenu
            SetActiveButton(btnReports);
            submenuReports.Visibility = Visibility.Visible;

            // Initialize data and generate report
            LoadMockTransactions();
            InitializeFilters();
            GenerateReport();
        }

        // ===== DATA MODELS =====
        public class SaleTransaction
        {
            public string TransactionNumber { get; set; }
            public DateTime DateTime { get; set; }
            public List<TransactionItem> Items { get; set; }
            public string DocumentType { get; set; } // "Receipt" or "Invoice"
            public string PaymentMethod { get; set; } // "Cash" or "Card"
            public decimal Total { get; set; }
            public string CashierName { get; set; }
        }

        public class TransactionItem
        {
            public string ProductName { get; set; }
            public string SKU { get; set; }
            public string Category { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Subtotal => Quantity * UnitPrice;
        }

        public class TopProductData
        {
            public string ProductName { get; set; }
            public int QuantitySold { get; set; }
            public decimal Revenue { get; set; }
            public string RevenueFormatted => $"PLN {Revenue:N2}";
            public double PercentOfTotal { get; set; }
            public string PercentFormatted => $"{PercentOfTotal:F1}%";
        }

        public class CategorySalesData
        {
            public string CategoryName { get; set; }
            public int TransactionCount { get; set; }
            public decimal Revenue { get; set; }
            public string RevenueFormatted => $"PLN {Revenue:N2}";
            public decimal AvgTransaction { get; set; }
            public string AvgFormatted => $"PLN {AvgTransaction:N2}";
        }

        public class TopDayData
        {
            public DateTime Date { get; set; }
            public string DateFormatted => Date.ToString("dd.MM.yyyy");
            public string DayOfWeek => Date.ToString("dddd");
            public int TransactionCount { get; set; }
            public decimal Revenue { get; set; }
            public string RevenueFormatted => $"PLN {Revenue:N2}";
        }

        // ===== MOCK DATA =====
        private void LoadMockTransactions()
        {
            _allTransactions = new List<SaleTransaction>
            {
                // November transactions
                new SaleTransaction
                {
                    TransactionNumber = "#TRX202511161443522",
                    DateTime = new DateTime(2025, 11, 16, 14, 30, 0),
                    DocumentType = "Receipt",
                    PaymentMethod = "Cash",
                    Total = 3300.08m,
                    CashierName = "Admin User",
                    Items = new List<TransactionItem>
                    {
                        new TransactionItem { ProductName = "Mountain Bike Pro X", SKU = "BIKE-001", Category = "Bicycles", Quantity = 1, UnitPrice = 2499.00m },
                        new TransactionItem { ProductName = "Safety Helmet Pro", SKU = "HELM-001", Category = "Accessories", Quantity = 1, UnitPrice = 149.00m },
                        new TransactionItem { ProductName = "LED Light Set", SKU = "LIGH-001", Category = "Accessories", Quantity = 1, UnitPrice = 14.99m }
                    }
                },
                new SaleTransaction
                {
                    TransactionNumber = "#TRX202511161301245",
                    DateTime = new DateTime(2025, 11, 16, 13, 15, 0),
                    DocumentType = "Invoice",
                    PaymentMethod = "Card",
                    Total = 1899.00m,
                    CashierName = "John Smith",
                    Items = new List<TransactionItem>
                    {
                        new TransactionItem { ProductName = "Road Bike Elite", SKU = "BIKE-002", Category = "Bicycles", Quantity = 1, UnitPrice = 1799.00m },
                        new TransactionItem { ProductName = "Water Bottle", SKU = "ACCS-004", Category = "Accessories", Quantity = 2, UnitPrice = 50.00m }
                    }
                },
                new SaleTransaction
                {
                    TransactionNumber = "#TRX202511151745891",
                    DateTime = new DateTime(2025, 11, 15, 17, 45, 0),
                    DocumentType = "Receipt",
                    PaymentMethod = "Cash",
                    Total = 425.50m,
                    CashierName = "Admin User",
                    Items = new List<TransactionItem>
                    {
                        new TransactionItem { ProductName = "Bike Lock Premium", SKU = "ACCS-001", Category = "Accessories", Quantity = 2, UnitPrice = 89.99m },
                        new TransactionItem { ProductName = "Repair Kit", SKU = "TOOL-001", Category = "Tools", Quantity = 1, UnitPrice = 45.00m },
                        new TransactionItem { ProductName = "Chain Lubricant", SKU = "MAIN-001", Category = "Parts", Quantity = 3, UnitPrice = 12.00m },
                        new TransactionItem { ProductName = "Tire Pump", SKU = "TOOL-002", Category = "Tools", Quantity = 1, UnitPrice = 79.00m }
                    }
                },
                new SaleTransaction
                {
                    TransactionNumber = "#TRX202511151342167",
                    DateTime = new DateTime(2025, 11, 15, 13, 42, 0),
                    DocumentType = "Invoice",
                    PaymentMethod = "Card",
                    Total = 3499.00m,
                    CashierName = "Sarah Johnson",
                    Items = new List<TransactionItem>
                    {
                        new TransactionItem { ProductName = "Electric Bike Ultra", SKU = "BIKE-003", Category = "Bicycles", Quantity = 1, UnitPrice = 3499.00m }
                    }
                },
                new SaleTransaction
                {
                    TransactionNumber = "#TRX202511141028443",
                    DateTime = new DateTime(2025, 11, 14, 10, 28, 0),
                    DocumentType = "Receipt",
                    PaymentMethod = "Cash",
                    Total = 678.95m,
                    CashierName = "Admin User",
                    Items = new List<TransactionItem>
                    {
                        new TransactionItem { ProductName = "Cycling Gloves Pro", SKU = "APPR-001", Category = "Apparel", Quantity = 2, UnitPrice = 49.99m },
                        new TransactionItem { ProductName = "Bike Computer GPS", SKU = "ELEC-001", Category = "Accessories", Quantity = 1, UnitPrice = 299.00m },
                        new TransactionItem { ProductName = "Energy Bars Pack", SKU = "FOOD-001", Category = "Accessories", Quantity = 3, UnitPrice = 15.99m },
                        new TransactionItem { ProductName = "Hydration Pack", SKU = "ACCS-005", Category = "Accessories", Quantity = 1, UnitPrice = 199.00m }
                    }
                },
                new SaleTransaction
                {
                    TransactionNumber = "#TRX202511131612334",
                    DateTime = new DateTime(2025, 11, 13, 16, 12, 0),
                    DocumentType = "Invoice",
                    PaymentMethod = "Card",
                    Total = 2899.00m,
                    CashierName = "John Smith",
                    Items = new List<TransactionItem>
                    {
                        new TransactionItem { ProductName = "Mountain Bike Pro X", SKU = "BIKE-001", Category = "Bicycles", Quantity = 1, UnitPrice = 2499.00m },
                        new TransactionItem { ProductName = "Bike Stand", SKU = "ACCS-006", Category = "Accessories", Quantity = 1, UnitPrice = 400.00m }
                    }
                },
                new SaleTransaction
                {
                    TransactionNumber = "#TRX202511121534782",
                    DateTime = new DateTime(2025, 11, 12, 15, 34, 0),
                    DocumentType = "Receipt",
                    PaymentMethod = "Card",
                    Total = 1245.00m,
                    CashierName = "Admin User",
                    Items = new List<TransactionItem>
                    {
                        new TransactionItem { ProductName = "Road Bike Elite", SKU = "BIKE-002", Category = "Bicycles", Quantity = 1, UnitPrice = 1799.00m },
                        new TransactionItem { ProductName = "Safety Helmet Pro", SKU = "HELM-001", Category = "Accessories", Quantity = 1, UnitPrice = 149.00m }
                    }
                },
                new SaleTransaction
                {
                    TransactionNumber = "#TRX202511111234567",
                    DateTime = new DateTime(2025, 11, 11, 12, 34, 0),
                    DocumentType = "Receipt",
                    PaymentMethod = "Cash",
                    Total = 567.89m,
                    CashierName = "Sarah Johnson",
                    Items = new List<TransactionItem>
                    {
                        new TransactionItem { ProductName = "Bike Lock Premium", SKU = "ACCS-001", Category = "Accessories", Quantity = 3, UnitPrice = 89.99m },
                        new TransactionItem { ProductName = "LED Light Set", SKU = "LIGH-001", Category = "Accessories", Quantity = 5, UnitPrice = 14.99m },
                        new TransactionItem { ProductName = "Water Bottle", SKU = "ACCS-004", Category = "Accessories", Quantity = 4, UnitPrice = 50.00m }
                    }
                },
                new SaleTransaction
                {
                    TransactionNumber = "#TRX202511101045123",
                    DateTime = new DateTime(2025, 11, 10, 10, 45, 0),
                    DocumentType = "Invoice",
                    PaymentMethod = "Card",
                    Total = 4567.00m,
                    CashierName = "John Smith",
                    Items = new List<TransactionItem>
                    {
                        new TransactionItem { ProductName = "Electric Bike Ultra", SKU = "BIKE-003", Category = "Bicycles", Quantity = 1, UnitPrice = 3499.00m },
                        new TransactionItem { ProductName = "Bike Computer GPS", SKU = "ELEC-001", Category = "Accessories", Quantity = 2, UnitPrice = 299.00m },
                        new TransactionItem { ProductName = "Cycling Gloves Pro", SKU = "APPR-001", Category = "Apparel", Quantity = 3, UnitPrice = 49.99m }
                    }
                },
                new SaleTransaction
                {
                    TransactionNumber = "#TRX202511091623456",
                    DateTime = new DateTime(2025, 11, 9, 16, 23, 0),
                    DocumentType = "Receipt",
                    PaymentMethod = "Cash",
                    Total = 2890.50m,
                    CashierName = "Admin User",
                    Items = new List<TransactionItem>
                    {
                        new TransactionItem { ProductName = "Mountain Bike Pro X", SKU = "BIKE-001", Category = "Bicycles", Quantity = 1, UnitPrice = 2499.00m },
                        new TransactionItem { ProductName = "Safety Helmet Pro", SKU = "HELM-001", Category = "Accessories", Quantity = 2, UnitPrice = 149.00m },
                        new TransactionItem { ProductName = "Tire Pump", SKU = "TOOL-002", Category = "Tools", Quantity = 1, UnitPrice = 79.00m }
                    }
                },
                // Add more transactions for better data
                new SaleTransaction
                {
                    TransactionNumber = "#TRX202511081234890",
                    DateTime = new DateTime(2025, 11, 8, 12, 34, 0),
                    DocumentType = "Receipt",
                    PaymentMethod = "Card",
                    Total = 345.67m,
                    CashierName = "Sarah Johnson",
                    Items = new List<TransactionItem>
                    {
                        new TransactionItem { ProductName = "Chain Lubricant", SKU = "MAIN-001", Category = "Parts", Quantity = 5, UnitPrice = 12.00m },
                        new TransactionItem { ProductName = "Repair Kit", SKU = "TOOL-001", Category = "Tools", Quantity = 3, UnitPrice = 45.00m },
                        new TransactionItem { ProductName = "Water Bottle", SKU = "ACCS-004", Category = "Accessories", Quantity = 2, UnitPrice = 50.00m }
                    }
                },
                new SaleTransaction
                {
                    TransactionNumber = "#TRX202511071456234",
                    DateTime = new DateTime(2025, 11, 7, 14, 56, 0),
                    DocumentType = "Invoice",
                    PaymentMethod = "Card",
                    Total = 5234.99m,
                    CashierName = "John Smith",
                    Items = new List<TransactionItem>
                    {
                        new TransactionItem { ProductName = "Road Bike Elite", SKU = "BIKE-002", Category = "Bicycles", Quantity = 2, UnitPrice = 1799.00m },
                        new TransactionItem { ProductName = "Bike Stand", SKU = "ACCS-006", Category = "Accessories", Quantity = 2, UnitPrice = 400.00m },
                        new TransactionItem { ProductName = "LED Light Set", SKU = "LIGH-001", Category = "Accessories", Quantity = 3, UnitPrice = 14.99m }
                    }
                }
            };
        }

        // ===== FILTER INITIALIZATION =====
        private void InitializeFilters()
        {
            cmbPeriod.Items.Add("Last 7 Days");
            cmbPeriod.Items.Add("Last 30 Days");
            cmbPeriod.Items.Add("This Month");
            cmbPeriod.Items.Add("Last Month");
            cmbPeriod.Items.Add("This Year");
            cmbPeriod.Items.Add("Custom Range");
            cmbPeriod.SelectedIndex = 1; // Default: Last 30 Days
        }

        // ===== REPORT GENERATION =====
        private void GenerateReport()
        {
            // Get filtered transactions based on selected period
            var filteredTransactions = GetFilteredTransactions();

            if (filteredTransactions.Count == 0)
            {
                MessageBox.Show("No transactions found for the selected period.", "No Data",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Calculate KPIs
            CalculateKPIs(filteredTransactions);

            // Generate tables
            GenerateTopProducts(filteredTransactions);
            GenerateSalesByCategory(filteredTransactions);
            GenerateTopSellingDays(filteredTransactions);
        }

        private List<SaleTransaction> GetFilteredTransactions()
        {
            DateTime startDate;
            DateTime endDate = DateTime.Now;

            string selectedPeriod = cmbPeriod.SelectedItem?.ToString() ?? "Last 30 Days";

            switch (selectedPeriod)
            {
                case "Last 7 Days":
                    startDate = DateTime.Now.AddDays(-7);
                    break;
                case "Last 30 Days":
                    startDate = DateTime.Now.AddDays(-30);
                    break;
                case "This Month":
                    startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    break;
                case "Last Month":
                    startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);
                    endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);
                    break;
                case "This Year":
                    startDate = new DateTime(DateTime.Now.Year, 1, 1);
                    break;
                default:
                    startDate = DateTime.Now.AddDays(-30);
                    break;
            }

            return _allTransactions
                .Where(t => t.DateTime >= startDate && t.DateTime <= endDate)
                .ToList();
        }

        private void CalculateKPIs(List<SaleTransaction> transactions)
        {
            // Total Revenue
            decimal totalRevenue = transactions.Sum(t => t.Total);
            txtTotalRevenue.Text = $"PLN {totalRevenue:N2}";
            txtRevenueChange.Text = "↑ +12.5%"; // Mock comparison

            // Total Transactions
            int totalTransactions = transactions.Count;
            txtTotalTransactions.Text = totalTransactions.ToString();
            txtTransactionsChange.Text = "↑ +8.3%"; // Mock comparison

            // Average Transaction Value
            decimal avgTransaction = totalTransactions > 0 ? totalRevenue / totalTransactions : 0;
            txtAvgTransaction.Text = $"PLN {avgTransaction:N2}";
            txtAvgChange.Text = "↑ +3.7%"; // Mock comparison

            // Total Items Sold
            int totalItems = transactions.Sum(t => t.Items.Sum(i => i.Quantity));
            txtTotalItems.Text = totalItems.ToString();
            txtItemsChange.Text = "↑ +15.2%"; // Mock comparison

            // Receipts vs Invoices
            int receiptsCount = transactions.Count(t => t.DocumentType == "Receipt");
            int invoicesCount = transactions.Count(t => t.DocumentType == "Invoice");
            double receiptPercent = totalTransactions > 0 ? (receiptsCount * 100.0 / totalTransactions) : 0;

            txtReceiptsCount.Text = receiptsCount.ToString();
            txtReceiptsPercent.Text = $"{receiptPercent:F1}%";
            txtInvoicesCount.Text = invoicesCount.ToString();
            txtInvoicesPercent.Text = $"{100 - receiptPercent:F1}%";

            // Cash vs Card
            decimal cashTotal = transactions.Where(t => t.PaymentMethod == "Cash").Sum(t => t.Total);
            decimal cardTotal = transactions.Where(t => t.PaymentMethod == "Card").Sum(t => t.Total);
            double cashPercent = totalRevenue > 0 ? (double)(cashTotal * 100 / totalRevenue) : 0;

            txtCashAmount.Text = $"PLN {cashTotal:N2}";
            txtCashPercent.Text = $"{cashPercent:F1}%";
            txtCardAmount.Text = $"PLN {cardTotal:N2}";
            txtCardPercent.Text = $"{100 - cashPercent:F1}%";
        }

        private void GenerateTopProducts(List<SaleTransaction> transactions)
        {
            var productSales = new Dictionary<string, (int quantity, decimal revenue)>();

            foreach (var transaction in transactions)
            {
                foreach (var item in transaction.Items)
                {
                    if (productSales.ContainsKey(item.ProductName))
                    {
                        var current = productSales[item.ProductName];
                        productSales[item.ProductName] = (
                            current.quantity + item.Quantity,
                            current.revenue + item.Subtotal
                        );
                    }
                    else
                    {
                        productSales[item.ProductName] = (item.Quantity, item.Subtotal);
                    }
                }
            }

            decimal totalRevenue = productSales.Sum(p => p.Value.revenue);

            var topProducts = productSales
                .OrderByDescending(p => p.Value.revenue)
                .Take(5)
                .Select(p => new TopProductData
                {
                    ProductName = p.Key,
                    QuantitySold = p.Value.quantity,
                    Revenue = p.Value.revenue,
                    PercentOfTotal = totalRevenue > 0 ? (double)(p.Value.revenue * 100 / totalRevenue) : 0
                })
                .ToList();

            // Clear existing rows (keep header)
            while (topProductsPanel.Children.Count > 1)
            {
                topProductsPanel.Children.RemoveAt(1);
            }

            // Add product rows
            foreach (var product in topProducts)
            {
                AddTopProductRow(product);
            }
        }

        private void AddTopProductRow(TopProductData product)
        {
            var border = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDEE2E6")),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Background = Brushes.White,
                Padding = new Thickness(0, 12, 0, 12)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Product Name
            var txtName = new TextBlock
            {
                Text = product.ProductName,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                FontWeight = FontWeights.Medium,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(txtName, 0);
            grid.Children.Add(txtName);

            // Quantity
            var txtQty = new TextBlock
            {
                Text = product.QuantitySold.ToString(),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtQty, 1);
            grid.Children.Add(txtQty);

            // Revenue
            var txtRevenue = new TextBlock
            {
                Text = product.RevenueFormatted,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtRevenue, 2);
            grid.Children.Add(txtRevenue);

            // Percent
            var txtPercent = new TextBlock
            {
                Text = product.PercentFormatted,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF28A745")),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtPercent, 3);
            grid.Children.Add(txtPercent);

            border.Child = grid;
            topProductsPanel.Children.Add(border);
        }

        private void GenerateSalesByCategory(List<SaleTransaction> transactions)
        {
            var categorySales = new Dictionary<string, (int transactions, decimal revenue)>();

            foreach (var transaction in transactions)
            {
                var categoryGroups = transaction.Items.GroupBy(i => i.Category);
                foreach (var group in categoryGroups)
                {
                    string category = group.Key;
                    decimal revenue = group.Sum(i => i.Subtotal);

                    if (categorySales.ContainsKey(category))
                    {
                        var current = categorySales[category];
                        categorySales[category] = (
                            current.transactions + 1,
                            current.revenue + revenue
                        );
                    }
                    else
                    {
                        categorySales[category] = (1, revenue);
                    }
                }
            }

            var categoryData = categorySales
                .OrderByDescending(c => c.Value.revenue)
                .Select(c => new CategorySalesData
                {
                    CategoryName = c.Key,
                    TransactionCount = c.Value.transactions,
                    Revenue = c.Value.revenue,
                    AvgTransaction = c.Value.transactions > 0 ? c.Value.revenue / c.Value.transactions : 0
                })
                .ToList();

            // Clear existing rows (keep header)
            while (categoryPanel.Children.Count > 1)
            {
                categoryPanel.Children.RemoveAt(1);
            }

            // Add category rows
            foreach (var category in categoryData)
            {
                AddCategoryRow(category);
            }
        }

        private void AddCategoryRow(CategorySalesData category)
        {
            var border = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDEE2E6")),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Background = Brushes.White,
                Padding = new Thickness(0, 12, 0, 12)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });

            // Category Name
            var txtName = new TextBlock
            {
                Text = category.CategoryName,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                FontWeight = FontWeights.Medium,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(txtName, 0);
            grid.Children.Add(txtName);

            // Transaction Count
            var txtCount = new TextBlock
            {
                Text = category.TransactionCount.ToString(),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtCount, 1);
            grid.Children.Add(txtCount);

            // Revenue
            var txtRevenue = new TextBlock
            {
                Text = category.RevenueFormatted,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtRevenue, 2);
            grid.Children.Add(txtRevenue);

            // Average
            var txtAvg = new TextBlock
            {
                Text = category.AvgFormatted,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtAvg, 3);
            grid.Children.Add(txtAvg);

            border.Child = grid;
            categoryPanel.Children.Add(border);
        }

        private void GenerateTopSellingDays(List<SaleTransaction> transactions)
        {
            var dailySales = transactions
                .GroupBy(t => t.DateTime.Date)
                .Select(g => new TopDayData
                {
                    Date = g.Key,
                    TransactionCount = g.Count(),
                    Revenue = g.Sum(t => t.Total)
                })
                .OrderByDescending(d => d.Revenue)
                .Take(5)
                .ToList();

            // Clear existing rows (keep header)
            while (topDaysPanel.Children.Count > 1)
            {
                topDaysPanel.Children.RemoveAt(1);
            }

            // Add day rows
            foreach (var day in dailySales)
            {
                AddTopDayRow(day);
            }
        }

        private void AddTopDayRow(TopDayData day)
        {
            var border = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDEE2E6")),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Background = Brushes.White,
                Padding = new Thickness(0, 12, 0, 12)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });

            // Date
            var txtDate = new TextBlock
            {
                Text = day.DateFormatted,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                FontWeight = FontWeights.Medium,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(txtDate, 0);
            grid.Children.Add(txtDate);

            // Day of Week
            var txtDay = new TextBlock
            {
                Text = day.DayOfWeek,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtDay, 1);
            grid.Children.Add(txtDay);

            // Transaction Count
            var txtCount = new TextBlock
            {
                Text = day.TransactionCount.ToString(),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtCount, 2);
            grid.Children.Add(txtCount);

            // Revenue
            var txtRevenue = new TextBlock
            {
                Text = day.RevenueFormatted,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtRevenue, 3);
            grid.Children.Add(txtRevenue);

            border.Child = grid;
            topDaysPanel.Children.Add(border);
        }

        // ===== FILTER HANDLERS =====
        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            GenerateReport();
        }

        private void Period_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Auto-generate when period changes
            if (IsLoaded)
            {
                GenerateReport();
            }
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

        private void SalesHistory_Click(object sender, RoutedEventArgs e)
        {
            SalesHistoryWindow salesHistoryWindow = new SalesHistoryWindow(_username, _password, _fullName, _role);
            salesHistoryWindow.Show();
            this.Close();
        }

        private void Invoices_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Invoices view - coming soon!", "Info",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Already on this page
        private void SalesReports_Click(object sender, RoutedEventArgs e) { }

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
