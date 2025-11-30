using CycleDesk.Services;
using CycleDesk.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CycleDesk
{
    public partial class MainDashboardWindow : Window
    {
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;

        private DashboardService _dashboardService;
        private int _currentChartDays = 7;

        public MainDashboardWindow(string username, string password, string fullName, string role)
        {
            InitializeComponent();

            _username = username;
            _password = password;
            _fullName = fullName;
            _role = role;

            _dashboardService = new DashboardService();

            // Inicjalizuj menu
            sideMenu.Initialize(fullName, role);
            sideMenu.SetActiveMenu("Dashboard");

            // Podłącz eventy
            sideMenu.ProductsClicked += (s, e) => NavigateToProducts();
            sideMenu.CategoriesClicked += (s, e) => NavigateToCategories();
            sideMenu.InventoryStatusClicked += (s, e) => NavigateToInventoryStatus();
            sideMenu.GoodsReceiptClicked += (s, e) => NavigateToGoodsReceipt();
            sideMenu.SuppliersClicked += (s, e) => NavigateToSuppliers();
            sideMenu.NewSaleClicked += (s, e) => NavigateToNewSale();
            sideMenu.SalesHistoryClicked += (s, e) => NavigateToSalesHistory();
            
            sideMenu.SalesReportsClicked += (s, e) => NavigateToSalesReports();
            sideMenu.InventoryReportsClicked += (s, e) => NavigateToInventoryReports();
            sideMenu.ProductsToOrderClicked += (s, e) => NavigateToProductsToOrder();
            sideMenu.UsersClicked += (s, e) => NavigateToUsers();
            sideMenu.SettingsClicked += (s, e) => NavigateToSettings();
            sideMenu.LogoutClicked += (s, e) => HandleLogout();

            // Załaduj dane dashboardu
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                LoadKpiCards();
                LoadTopProducts();
                LoadProductsToOrder();
                LoadRecentGoodsReceipts();
                LoadSalesChart(_currentChartDays);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard: {ex.Message}");
            }
        }

        #region KPI Cards

        private void LoadKpiCards()
        {
            try
            {
                var kpi = _dashboardService.GetKpiData();

                // Today's Sales
                if (txtTodaySales != null)
                    txtTodaySales.Text = kpi.TodaySales.ToString("N2", CultureInfo.GetCultureInfo("pl-PL")) + " PLN";

                if (txtSalesChange != null)
                {
                    txtSalesChange.Text = (kpi.TodaySalesUp ? "↑ " : "↓ ") + kpi.TodaySalesChange.ToString("N1") + "%";
                    txtSalesChange.Foreground = new SolidColorBrush(
                        kpi.TodaySalesUp ? (Color)ColorConverter.ConvertFromString("#FF28A745")
                                         : (Color)ColorConverter.ConvertFromString("#FFDC3545"));
                }

                // Transactions
                if (txtTransactions != null)
                    txtTransactions.Text = kpi.TodayTransactions.ToString();

                if (txtTransactionsChange != null)
                {
                    txtTransactionsChange.Text = (kpi.TransactionsUp ? "↑ " : "↓ ") + kpi.TransactionsChange.ToString("N1") + "%";
                    txtTransactionsChange.Foreground = new SolidColorBrush(
                        kpi.TransactionsUp ? (Color)ColorConverter.ConvertFromString("#FF28A745")
                                           : (Color)ColorConverter.ConvertFromString("#FFDC3545"));
                }

                // Low Stock
                if (txtLowStock != null)
                    txtLowStock.Text = kpi.LowStockCount.ToString();

                // Warehouse Value
                if (txtWarehouseValue != null)
                    txtWarehouseValue.Text = kpi.WarehouseValue.ToString("N0", CultureInfo.GetCultureInfo("pl-PL")) + " PLN";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading KPI: {ex.Message}");
            }
        }

        #endregion

        #region Top Products

        private void LoadTopProducts()
        {
            try
            {
                var topProducts = _dashboardService.GetTopProducts(30, 5);

                if (topProductsPanel == null) return;

                topProductsPanel.Children.Clear();

                foreach (var product in topProducts)
                {
                    var border = CreateTopProductRow(product);
                    topProductsPanel.Children.Add(border);
                }

                // Jeśli mniej niż 5 produktów, dodaj puste wiersze
                for (int i = topProducts.Count; i < 5; i++)
                {
                    var emptyBorder = CreateEmptyProductRow();
                    topProductsPanel.Children.Add(emptyBorder);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading top products: {ex.Message}");
            }
        }

        private Border CreateTopProductRow(TopProductDto product)
        {
            var border = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF1F3F5")),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(0, 12, 0, 12)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var nameText = new TextBlock
            {
                Text = TruncateText(product.ProductName, 20),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50"))
            };
            Grid.SetColumn(nameText, 0);
            grid.Children.Add(nameText);

            var qtyText = new TextBlock
            {
                Text = product.QuantitySold + " pcs.",
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(qtyText, 1);
            grid.Children.Add(qtyText);

            var revenueText = new TextBlock
            {
                Text = product.Revenue.ToString("N2", CultureInfo.GetCultureInfo("pl-PL")) + " zł",
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                TextAlignment = TextAlignment.Right
            };
            Grid.SetColumn(revenueText, 2);
            grid.Children.Add(revenueText);

            border.Child = grid;
            return border;
        }

        private Border CreateEmptyProductRow()
        {
            var border = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF1F3F5")),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(0, 12, 0, 12)
            };

            var text = new TextBlock
            {
                Text = "—",
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADB5BD"))
            };

            border.Child = text;
            return border;
        }

        #endregion

        #region Products to Order

        private void LoadProductsToOrder()
        {
            try
            {
                var products = _dashboardService.GetProductsToOrder(5);

                if (productsToOrderPanel == null) return;

                productsToOrderPanel.Children.Clear();

                foreach (var product in products)
                {
                    var border = CreateProductToOrderRow(product);
                    productsToOrderPanel.Children.Add(border);
                }

                // Jeśli mniej niż 5, dodaj puste
                for (int i = products.Count; i < 5; i++)
                {
                    var emptyBorder = CreateEmptyProductRow();
                    productsToOrderPanel.Children.Add(emptyBorder);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading products to order: {ex.Message}");
            }
        }

        private Border CreateProductToOrderRow(ProductToOrderDto product)
        {
            var border = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF1F3F5")),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(0, 12, 0, 12)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var nameText = new TextBlock
            {
                Text = TruncateText(product.ProductName, 18),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50"))
            };
            Grid.SetColumn(nameText, 0);
            grid.Children.Add(nameText);

            var stockText = new TextBlock
            {
                Text = product.CurrentStock.ToString(),
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(product.StockColor)),
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(stockText, 1);
            grid.Children.Add(stockText);

            var minText = new TextBlock
            {
                Text = product.MinimumStock.ToString(),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(minText, 2);
            grid.Children.Add(minText);

            border.Child = grid;
            return border;
        }

        #endregion

        #region Recent Goods Receipts

        private void LoadRecentGoodsReceipts()
        {
            try
            {
                var receipts = _dashboardService.GetRecentGoodsReceipts(6);

                if (goodsReceiptsPanel == null) return;

                goodsReceiptsPanel.Children.Clear();

                foreach (var receipt in receipts)
                {
                    var border = CreateGoodsReceiptRow(receipt);
                    goodsReceiptsPanel.Children.Add(border);
                }

                // Jeśli mniej niż 6, dodaj puste
                for (int i = receipts.Count; i < 6; i++)
                {
                    var emptyBorder = CreateEmptyReceiptRow();
                    goodsReceiptsPanel.Children.Add(emptyBorder);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading goods receipts: {ex.Message}");
            }
        }

        private Border CreateGoodsReceiptRow(RecentGoodsReceiptDto receipt)
        {
            var border = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF1F3F5")),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(0, 15, 0, 15)
            };

            var grid = new Grid();
            for (int i = 0; i < 6; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Document No.
            var docText = new TextBlock
            {
                Text = receipt.ReceiptNumber,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                Margin = new Thickness(0, 0, 20, 0)
            };
            Grid.SetColumn(docText, 0);
            grid.Children.Add(docText);

            // Supplier
            var supplierText = new TextBlock
            {
                Text = TruncateText(receipt.SupplierName, 15),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                Margin = new Thickness(0, 0, 20, 0)
            };
            Grid.SetColumn(supplierText, 1);
            grid.Children.Add(supplierText);

            // Date
            var dateText = new TextBlock
            {
                Text = receipt.ReceiptDate.ToString("dd.MM.yyyy"),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                Margin = new Thickness(0, 0, 20, 0)
            };
            Grid.SetColumn(dateText, 2);
            grid.Children.Add(dateText);

            // Value
            var valueText = new TextBlock
            {
                Text = receipt.TotalAmount.ToString("N2", CultureInfo.GetCultureInfo("pl-PL")) + " PLN",
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                Margin = new Thickness(0, 0, 20, 0)
            };
            Grid.SetColumn(valueText, 3);
            grid.Children.Add(valueText);

            // Created By
            var createdByText = new TextBlock
            {
                Text = TruncateText(receipt.CreatedByName, 15),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                Margin = new Thickness(0, 0, 20, 0)
            };
            Grid.SetColumn(createdByText, 4);
            grid.Children.Add(createdByText);

            // Status badge
            var statusBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(receipt.StatusBackground)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(8, 4, 8, 4),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var statusPanel = new StackPanel { Orientation = Orientation.Horizontal };

            var iconText = new TextBlock
            {
                Text = receipt.StatusIcon,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(receipt.StatusColor)),
                Margin = new Thickness(0, 0, 4, 0)
            };
            statusPanel.Children.Add(iconText);

            var statusText = new TextBlock
            {
                Text = receipt.Status,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(receipt.StatusColor))
            };
            statusPanel.Children.Add(statusText);

            statusBorder.Child = statusPanel;
            Grid.SetColumn(statusBorder, 5);
            grid.Children.Add(statusBorder);

            border.Child = grid;
            return border;
        }

        private Border CreateEmptyReceiptRow()
        {
            var border = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF1F3F5")),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(0, 15, 0, 15)
            };

            var text = new TextBlock
            {
                Text = "—",
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADB5BD"))
            };

            border.Child = text;
            return border;
        }

        #endregion

        #region Sales Chart

        private void LoadSalesChart(int days)
        {
            try
            {
                var chartData = _dashboardService.GetSalesChartData(days);

                if (chartCanvas == null || chartData == null || chartData.Count == 0) return;

                chartCanvas.Children.Clear();

                double canvasWidth = 700;
                double canvasHeight = 250;
                double barWidth = (canvasWidth - 60) / chartData.Count / 2 - 4;
                double maxValue = Math.Max(
                    chartData.Max(d => (double)d.Income), 
                    chartData.Max(d => (double)d.Expenses)
                );

                if (maxValue == 0) maxValue = 1000; // Prevent division by zero

                double xOffset = 40;

                for (int i = 0; i < chartData.Count; i++)
                {
                    var data = chartData[i];
                    double x = xOffset + i * ((canvasWidth - 60) / chartData.Count);

                    // Income bar (blue)
                    double incomeHeight = ((double)data.Income / maxValue) * (canvasHeight - 40);
                    var incomeBar = new Rectangle
                    {
                        Width = barWidth,
                        Height = Math.Max(2, incomeHeight),
                        Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF4169E1")),
                        RadiusX = 3,
                        RadiusY = 3
                    };
                    Canvas.SetLeft(incomeBar, x);
                    Canvas.SetBottom(incomeBar, 30);
                    chartCanvas.Children.Add(incomeBar);

                    // Expenses bar (green)
                    double expenseHeight = ((double)data.Expenses / maxValue) * (canvasHeight - 40);
                    var expenseBar = new Rectangle
                    {
                        Width = barWidth,
                        Height = Math.Max(2, expenseHeight),
                        Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF20C997")),
                        RadiusX = 3,
                        RadiusY = 3
                    };
                    Canvas.SetLeft(expenseBar, x + barWidth + 2);
                    Canvas.SetBottom(expenseBar, 30);
                    chartCanvas.Children.Add(expenseBar);

                    // Label
                    var label = new TextBlock
                    {
                        Text = data.Label,
                        FontSize = 10,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D"))
                    };
                    Canvas.SetLeft(label, x + barWidth / 2);
                    Canvas.SetBottom(label, 8);
                    chartCanvas.Children.Add(label);
                }

                // Y-axis labels
                for (int i = 0; i <= 4; i++)
                {
                    double value = maxValue * i / 4;
                    var yLabel = new TextBlock
                    {
                        Text = FormatChartValue(value),
                        FontSize = 9,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D"))
                    };
                    Canvas.SetLeft(yLabel, 0);
                    Canvas.SetBottom(yLabel, 30 + (canvasHeight - 40) * i / 4 - 6);
                    chartCanvas.Children.Add(yLabel);

                    // Grid line
                    var gridLine = new Line
                    {
                        X1 = 35,
                        X2 = canvasWidth - 10,
                        Y1 = 0,
                        Y2 = 0,
                        Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE9ECEF")),
                        StrokeThickness = 1
                    };
                    Canvas.SetLeft(gridLine, 0);
                    Canvas.SetBottom(gridLine, 30 + (canvasHeight - 40) * i / 4);
                    chartCanvas.Children.Add(gridLine);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading chart: {ex.Message}");
            }
        }

        private string FormatChartValue(double value)
        {
            if (value >= 1000)
                return (value / 1000).ToString("N0") + "k";
            return value.ToString("N0");
        }

        // Chart period buttons click handlers
        private void ChartPeriod7Days_Click(object sender, RoutedEventArgs e)
        {
            _currentChartDays = 7;
            UpdateChartPeriodButtons(7);
            LoadSalesChart(7);
        }

        private void ChartPeriod30Days_Click(object sender, RoutedEventArgs e)
        {
            _currentChartDays = 30;
            UpdateChartPeriodButtons(30);
            LoadSalesChart(30);
        }

        private void ChartPeriod90Days_Click(object sender, RoutedEventArgs e)
        {
            _currentChartDays = 90;
            UpdateChartPeriodButtons(90);
            LoadSalesChart(90);
        }

        private void UpdateChartPeriodButtons(int selectedDays)
        {
            // Update button styles
            if (btn7Days != null)
            {
                btn7Days.Background = selectedDays == 7
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50"))
                    : Brushes.Transparent;
                if (btn7Days.Child is TextBlock tb7)
                    tb7.Foreground = selectedDays == 7 ? Brushes.White
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D"));
            }

            if (btn30Days != null)
            {
                btn30Days.Background = selectedDays == 30
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50"))
                    : Brushes.Transparent;
                if (btn30Days.Child is TextBlock tb30)
                    tb30.Foreground = selectedDays == 30 ? Brushes.White
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D"));
            }

            if (btn90Days != null)
            {
                btn90Days.Background = selectedDays == 90
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50"))
                    : Brushes.Transparent;
                if (btn90Days.Child is TextBlock tb90)
                    tb90.Foreground = selectedDays == 90 ? Brushes.White
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D"));
            }
        }

        #endregion

        #region Helpers

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return "";
            if (text.Length <= maxLength) return text;
            return text.Substring(0, maxLength - 3) + "...";
        }

        #endregion

        #region Navigation

        private void NavigateToProducts()
        {
            ProductsWindow productsWindow = new ProductsWindow(_username, _password, _fullName, _role);
            productsWindow.Show();
            this.Close();
        }

        private void NavigateToCategories()
        {
            CategoriesWindow categoriesWindow = new CategoriesWindow(_username, _password, _fullName, _role);
            categoriesWindow.Show();
            this.Close();
        }

        private void NavigateToInventoryStatus()
        {
            InventoryStatusWindow inventoryWindow = new InventoryStatusWindow(_username, _password, _fullName, _role);
            inventoryWindow.Show();
            this.Close();
        }

        private void NavigateToGoodsReceipt()
        {
            GoodsReceiptWindow goodsReceiptWindow = new GoodsReceiptWindow(_username, _password, _fullName, _role);
            goodsReceiptWindow.Show();
            this.Close();
        }

        private void NavigateToSuppliers()
        {
            SuppliersWindow suppliersWindow = new SuppliersWindow(_username, _password, _fullName, _role);
            suppliersWindow.Show();
            this.Close();
        }

        private void NavigateToNewSale()
        {
            NewSaleWindow newSaleWindow = new NewSaleWindow(_username, _password, _fullName, _role);
            newSaleWindow.Show();
            this.Close();
        }

        private void NavigateToSalesHistory()
        {
            SalesHistoryWindow salesHistoryWindow = new SalesHistoryWindow(_username, _password, _fullName, _role);
            salesHistoryWindow.Show();
            this.Close();
        }

        private void NavigateToInvoices()
        {
            MessageBox.Show("Invoices view - coming soon!", "Info",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void NavigateToSalesReports()
        {
            SalesReportsWindow salesReportsWindow = new SalesReportsWindow(_username, _password, _fullName, _role);
            salesReportsWindow.Show();
            this.Close();
        }

        private void NavigateToInventoryReports()
        {
            InventoryReportsWindow inventoryReportsWindow = new InventoryReportsWindow(_username, _password, _fullName, _role);
            inventoryReportsWindow.Show();
            this.Close();
        }

        private void NavigateToProductsToOrder()
        {
            ProductsToOrderWindow productsToOrderWindow = new ProductsToOrderWindow(_username, _password, _fullName, _role);
            productsToOrderWindow.Show();
            this.Close();
        }

        private void NavigateToUsers()
        {
            UsersWindow usersWindow = new UsersWindow(_username, _password, _fullName, _role);
            usersWindow.Show();
            this.Close();
        }

        private void NavigateToSettings()
        {
            SettingsWindow settingsWindow = new SettingsWindow(_username, _password, _fullName, _role);
            settingsWindow.Show();
            this.Close();
        }

        private void HandleLogout()
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

        #endregion
    }
}
