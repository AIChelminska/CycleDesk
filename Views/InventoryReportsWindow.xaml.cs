using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CycleDesk
{
    public partial class InventoryReportsWindow : Window
    {
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;

        // Collections for mock data
        private List<InventoryProduct> _allProducts;

        public InventoryReportsWindow(string username, string password, string fullName, string role)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _fullName = fullName;
            _role = role;

            // Inicjalizuj SideMenuControl
            sideMenu.Initialize(fullName, role);
            sideMenu.SetActiveMenu("Reports", "InventoryReports");

            // Podłącz eventy menu
            ConnectMenuEvents();

            // Initialize data and generate report
            LoadMockProducts();
            InitializeFilters();
            GenerateReport();
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
                new SalesHistoryWindow(_username, _password, _fullName, _role).Show();
                Close();
            };

            sideMenu.InvoicesClicked += (s, e) =>
            {
                new InvoicesWindow(_username, _password, _fullName, _role).Show();
                Close();
            };

            sideMenu.SalesReportsClicked += (s, e) =>
            {
                new SalesReportsWindow(_username, _password, _fullName, _role).Show();
                Close();
            };

            sideMenu.InventoryReportsClicked += (s, e) =>
            {
                // Już jesteśmy na tej stronie - nic nie rób
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
        public class InventoryProduct
        {
            public string SKU { get; set; }
            public string ProductName { get; set; }
            public string Category { get; set; }
            public int CurrentStock { get; set; }
            public int MinimumStock { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal StockValue => CurrentStock * UnitPrice;
            public string StockStatus
            {
                get
                {
                    if (CurrentStock == 0) return "Out of Stock";
                    if (CurrentStock < MinimumStock) return "Low Stock";
                    return "In Stock";
                }
            }
        }

        public class CategoryStockData
        {
            public string CategoryName { get; set; }
            public int ProductCount { get; set; }
            public int TotalStock { get; set; }
            public decimal StockValue { get; set; }
            public string StockValueFormatted => $"PLN {StockValue:N2}";
            public double PercentOfTotal { get; set; }
            public string PercentFormatted => $"{PercentOfTotal:F1}%";
        }

        public class LowStockProduct
        {
            public string SKU { get; set; }
            public string ProductName { get; set; }
            public string Category { get; set; }
            public int CurrentStock { get; set; }
            public int MinimumStock { get; set; }
            public int ToOrder => Math.Max(0, MinimumStock - CurrentStock);
            public decimal UnitPrice { get; set; }
            public string UnitPriceFormatted => $"PLN {UnitPrice:N2}";
        }

        public class ValuableProduct
        {
            public string ProductName { get; set; }
            public string Category { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public string UnitPriceFormatted => $"PLN {UnitPrice:N2}";
            public decimal TotalValue { get; set; }
            public string TotalValueFormatted => $"PLN {TotalValue:N2}";
        }

        // ===== MOCK DATA =====
        private void LoadMockProducts()
        {
            _allProducts = new List<InventoryProduct>
            {
                // Bicycles
                new InventoryProduct { SKU = "BIKE-001", ProductName = "Mountain Bike Pro X", Category = "Bicycles", CurrentStock = 5, MinimumStock = 3, UnitPrice = 2499.00m },
                new InventoryProduct { SKU = "BIKE-002", ProductName = "Road Bike Elite", Category = "Bicycles", CurrentStock = 3, MinimumStock = 3, UnitPrice = 1799.00m },
                new InventoryProduct { SKU = "BIKE-003", ProductName = "Electric Bike Ultra", Category = "Bicycles", CurrentStock = 2, MinimumStock = 2, UnitPrice = 3499.00m },
                new InventoryProduct { SKU = "BIKE-004", ProductName = "City Bike Comfort", Category = "Bicycles", CurrentStock = 8, MinimumStock = 4, UnitPrice = 899.00m },
                new InventoryProduct { SKU = "BIKE-005", ProductName = "Kids Bike Junior", Category = "Bicycles", CurrentStock = 12, MinimumStock = 6, UnitPrice = 449.00m },

                // Accessories
                new InventoryProduct { SKU = "HELM-001", ProductName = "Safety Helmet Pro", Category = "Accessories", CurrentStock = 25, MinimumStock = 15, UnitPrice = 149.00m },
                new InventoryProduct { SKU = "LIGH-001", ProductName = "LED Light Set", Category = "Accessories", CurrentStock = 45, MinimumStock = 20, UnitPrice = 14.99m },
                new InventoryProduct { SKU = "ACCS-001", ProductName = "Bike Lock Premium", Category = "Accessories", CurrentStock = 8, MinimumStock = 10, UnitPrice = 89.99m },
                new InventoryProduct { SKU = "ACCS-004", ProductName = "Water Bottle", Category = "Accessories", CurrentStock = 67, MinimumStock = 30, UnitPrice = 12.99m },
                new InventoryProduct { SKU = "ACCS-005", ProductName = "Hydration Pack", Category = "Accessories", CurrentStock = 15, MinimumStock = 8, UnitPrice = 199.00m },
                new InventoryProduct { SKU = "ACCS-006", ProductName = "Bike Stand", Category = "Accessories", CurrentStock = 5, MinimumStock = 5, UnitPrice = 400.00m },
                new InventoryProduct { SKU = "ELEC-001", ProductName = "Bike Computer GPS", Category = "Accessories", CurrentStock = 12, MinimumStock = 6, UnitPrice = 299.00m },

                // Parts
                new InventoryProduct { SKU = "MAIN-001", ProductName = "Chain Lubricant", Category = "Parts", CurrentStock = 34, MinimumStock = 20, UnitPrice = 12.00m },
                new InventoryProduct { SKU = "TIRE-001", ProductName = "Tire 26 inch", Category = "Parts", CurrentStock = 3, MinimumStock = 10, UnitPrice = 45.00m },
                new InventoryProduct { SKU = "TIRE-002", ProductName = "Tire 28 inch", Category = "Parts", CurrentStock = 2, MinimumStock = 10, UnitPrice = 48.00m },
                new InventoryProduct { SKU = "BRAK-001", ProductName = "Brake Pads Set", Category = "Parts", CurrentStock = 18, MinimumStock = 15, UnitPrice = 35.00m },
                new InventoryProduct { SKU = "CHAI-001", ProductName = "Bike Chain", Category = "Parts", CurrentStock = 22, MinimumStock = 12, UnitPrice = 28.00m },
                new InventoryProduct { SKU = "PEDA-001", ProductName = "Pedals Pro", Category = "Parts", CurrentStock = 0, MinimumStock = 8, UnitPrice = 65.00m },

                // Tools
                new InventoryProduct { SKU = "TOOL-001", ProductName = "Repair Kit", Category = "Tools", CurrentStock = 14, MinimumStock = 10, UnitPrice = 45.00m },
                new InventoryProduct { SKU = "TOOL-002", ProductName = "Tire Pump", Category = "Tools", CurrentStock = 9, MinimumStock = 8, UnitPrice = 79.00m },
                new InventoryProduct { SKU = "TOOL-003", ProductName = "Multi-Tool Set", Category = "Tools", CurrentStock = 11, MinimumStock = 6, UnitPrice = 89.00m },
                new InventoryProduct { SKU = "TOOL-004", ProductName = "Bike Work Stand", Category = "Tools", CurrentStock = 3, MinimumStock = 2, UnitPrice = 299.00m },

                // Apparel
                new InventoryProduct { SKU = "APPR-001", ProductName = "Cycling Gloves Pro", Category = "Apparel", CurrentStock = 28, MinimumStock = 15, UnitPrice = 49.99m },
                new InventoryProduct { SKU = "APPR-002", ProductName = "Cycling Jersey", Category = "Apparel", CurrentStock = 18, MinimumStock = 12, UnitPrice = 79.99m },
                new InventoryProduct { SKU = "APPR-003", ProductName = "Cycling Shorts", Category = "Apparel", CurrentStock = 15, MinimumStock = 12, UnitPrice = 89.99m },
                new InventoryProduct { SKU = "APPR-004", ProductName = "Cycling Shoes", Category = "Apparel", CurrentStock = 6, MinimumStock = 8, UnitPrice = 199.99m },
                new InventoryProduct { SKU = "APPR-005", ProductName = "Rain Jacket", Category = "Apparel", CurrentStock = 0, MinimumStock = 6, UnitPrice = 149.99m }
            };
        }

        // ===== FILTER INITIALIZATION =====
        private void InitializeFilters()
        {
            // Category filter
            cmbCategory.Items.Add("All Categories");
            cmbCategory.Items.Add("Bicycles");
            cmbCategory.Items.Add("Accessories");
            cmbCategory.Items.Add("Parts");
            cmbCategory.Items.Add("Tools");
            cmbCategory.Items.Add("Apparel");
            cmbCategory.SelectedIndex = 0;

            // Stock status filter
            cmbStockStatus.Items.Add("All Products");
            cmbStockStatus.Items.Add("In Stock");
            cmbStockStatus.Items.Add("Low Stock");
            cmbStockStatus.Items.Add("Out of Stock");
            cmbStockStatus.SelectedIndex = 0;
        }

        // ===== REPORT GENERATION =====
        private void GenerateReport()
        {
            // Get filtered products
            var filteredProducts = GetFilteredProducts();

            if (filteredProducts.Count == 0)
            {
                MessageBox.Show("No products found for the selected filters.", "No Data",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Calculate KPIs
            CalculateKPIs(filteredProducts);

            // Generate tables
            GenerateStockValueByCategory(filteredProducts);
            GenerateLowStockProducts(filteredProducts);
            GenerateMostValuableProducts(filteredProducts);
        }

        private List<InventoryProduct> GetFilteredProducts()
        {
            var filtered = _allProducts.AsEnumerable();

            // Category filter
            string selectedCategory = cmbCategory.SelectedItem?.ToString() ?? "All Categories";
            if (selectedCategory != "All Categories")
            {
                filtered = filtered.Where(p => p.Category == selectedCategory);
            }

            // Stock status filter
            string selectedStatus = cmbStockStatus.SelectedItem?.ToString() ?? "All Products";
            if (selectedStatus != "All Products")
            {
                filtered = filtered.Where(p => p.StockStatus == selectedStatus);
            }

            return filtered.ToList();
        }

        private void CalculateKPIs(List<InventoryProduct> products)
        {
            // Total Products
            int totalProducts = products.Count;
            txtTotalProducts.Text = totalProducts.ToString();

            // Total Stock Value
            decimal totalValue = products.Sum(p => p.StockValue);
            txtStockValue.Text = $"PLN {totalValue:N2}";

            // Low Stock Items
            int lowStockCount = products.Count(p => p.StockStatus == "Low Stock");
            txtLowStockCount.Text = lowStockCount.ToString();
            double lowStockPercent = totalProducts > 0 ? (lowStockCount * 100.0 / totalProducts) : 0;
            txtLowStockPercent.Text = $"{lowStockPercent:F1}%";

            // Out of Stock
            int outOfStockCount = products.Count(p => p.StockStatus == "Out of Stock");
            txtOutOfStockCount.Text = outOfStockCount.ToString();
            double outOfStockPercent = totalProducts > 0 ? (outOfStockCount * 100.0 / totalProducts) : 0;
            txtOutOfStockPercent.Text = $"{outOfStockPercent:F1}%";

            // Categories
            int categoriesCount = products.Select(p => p.Category).Distinct().Count();
            txtCategoriesCount.Text = categoriesCount.ToString();

            // Average Stock Level
            double avgStock = totalProducts > 0 ? products.Average(p => p.CurrentStock) : 0;
            txtAvgStock.Text = $"{avgStock:F1} units";

            // Stock Status Breakdown
            int inStockCount = products.Count(p => p.StockStatus == "In Stock");
            double inStockPercent = totalProducts > 0 ? (inStockCount * 100.0 / totalProducts) : 0;

            txtInStockCount.Text = inStockCount.ToString();
            txtInStockPercent.Text = $"{inStockPercent:F1}%";
            txtLowStockCountBreakdown.Text = lowStockCount.ToString();
            txtLowStockPercentBreakdown.Text = $"{lowStockPercent:F1}%";
            txtOutOfStockCountBreakdown.Text = outOfStockCount.ToString();
            txtOutOfStockPercentBreakdown.Text = $"{outOfStockPercent:F1}%";
        }

        private void GenerateStockValueByCategory(List<InventoryProduct> products)
        {
            var categoryData = products
                .GroupBy(p => p.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    ProductCount = g.Count(),
                    TotalStock = g.Sum(p => p.CurrentStock),
                    StockValue = g.Sum(p => p.StockValue)
                })
                .OrderByDescending(c => c.StockValue)
                .ToList();

            decimal totalValue = categoryData.Sum(c => c.StockValue);

            var categoryStockData = categoryData
                .Select(c => new CategoryStockData
                {
                    CategoryName = c.Category,
                    ProductCount = c.ProductCount,
                    TotalStock = c.TotalStock,
                    StockValue = c.StockValue,
                    PercentOfTotal = totalValue > 0 ? (double)(c.StockValue * 100 / totalValue) : 0
                })
                .ToList();

            // Clear existing rows (keep header)
            while (categoryStockPanel.Children.Count > 1)
            {
                categoryStockPanel.Children.RemoveAt(1);
            }

            // Add category rows
            foreach (var category in categoryStockData)
            {
                AddCategoryStockRow(category);
            }
        }

        private void AddCategoryStockRow(CategoryStockData category)
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
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

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

            // Product Count
            var txtProducts = new TextBlock
            {
                Text = category.ProductCount.ToString(),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtProducts, 1);
            grid.Children.Add(txtProducts);

            // Total Stock
            var txtStock = new TextBlock
            {
                Text = category.TotalStock.ToString(),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtStock, 2);
            grid.Children.Add(txtStock);

            // Stock Value
            var txtValue = new TextBlock
            {
                Text = category.StockValueFormatted,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtValue, 3);
            grid.Children.Add(txtValue);

            // Percent
            var txtPercent = new TextBlock
            {
                Text = category.PercentFormatted,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF28A745")),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtPercent, 4);
            grid.Children.Add(txtPercent);

            border.Child = grid;
            categoryStockPanel.Children.Add(border);
        }

        private void GenerateLowStockProducts(List<InventoryProduct> products)
        {
            var lowStockProducts = products
                .Where(p => p.StockStatus == "Low Stock" || p.StockStatus == "Out of Stock")
                .OrderBy(p => p.CurrentStock)
                .ThenBy(p => p.ProductName)
                .Take(10)
                .Select(p => new LowStockProduct
                {
                    SKU = p.SKU,
                    ProductName = p.ProductName,
                    Category = p.Category,
                    CurrentStock = p.CurrentStock,
                    MinimumStock = p.MinimumStock,
                    UnitPrice = p.UnitPrice
                })
                .ToList();

            // Clear existing rows (keep header)
            while (lowStockPanel.Children.Count > 1)
            {
                lowStockPanel.Children.RemoveAt(1);
            }

            // Add product rows
            foreach (var product in lowStockProducts)
            {
                AddLowStockRow(product);
            }
        }

        private void AddLowStockRow(LowStockProduct product)
        {
            var border = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDEE2E6")),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Background = Brushes.White,
                Padding = new Thickness(0, 12, 0, 12)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // SKU
            var txtSKU = new TextBlock
            {
                Text = product.SKU,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(txtSKU, 0);
            grid.Children.Add(txtSKU);

            // Product Name
            var txtName = new TextBlock
            {
                Text = product.ProductName,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(txtName, 1);
            grid.Children.Add(txtName);

            // Current Stock
            var txtCurrent = new TextBlock
            {
                Text = product.CurrentStock.ToString(),
                FontSize = 13,
                Foreground = product.CurrentStock == 0
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDC3545"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFC107")),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtCurrent, 2);
            grid.Children.Add(txtCurrent);

            // Minimum
            var txtMin = new TextBlock
            {
                Text = product.MinimumStock.ToString(),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtMin, 3);
            grid.Children.Add(txtMin);

            // To Order
            var txtOrder = new TextBlock
            {
                Text = product.ToOrder.ToString(),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF28A745")),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtOrder, 4);
            grid.Children.Add(txtOrder);

            border.Child = grid;
            lowStockPanel.Children.Add(border);
        }

        private void GenerateMostValuableProducts(List<InventoryProduct> products)
        {
            var valuableProducts = products
                .OrderByDescending(p => p.StockValue)
                .Take(10)
                .Select(p => new ValuableProduct
                {
                    ProductName = p.ProductName,
                    Category = p.Category,
                    Quantity = p.CurrentStock,
                    UnitPrice = p.UnitPrice,
                    TotalValue = p.StockValue
                })
                .ToList();

            // Clear existing rows (keep header)
            while (valuableProductsPanel.Children.Count > 1)
            {
                valuableProductsPanel.Children.RemoveAt(1);
            }

            // Add product rows
            foreach (var product in valuableProducts)
            {
                AddValuableProductRow(product);
            }
        }

        private void AddValuableProductRow(ValuableProduct product)
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
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });

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

            // Category
            var txtCategory = new TextBlock
            {
                Text = product.Category,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtCategory, 1);
            grid.Children.Add(txtCategory);

            // Quantity
            var txtQty = new TextBlock
            {
                Text = product.Quantity.ToString(),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtQty, 2);
            grid.Children.Add(txtQty);

            // Unit Price
            var txtPrice = new TextBlock
            {
                Text = product.UnitPriceFormatted,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtPrice, 3);
            grid.Children.Add(txtPrice);

            // Total Value
            var txtValue = new TextBlock
            {
                Text = product.TotalValueFormatted,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtValue, 4);
            grid.Children.Add(txtValue);

            border.Child = grid;
            valuableProductsPanel.Children.Add(border);
        }

        // ===== FILTER HANDLERS =====
        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            GenerateReport();
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Auto-generate when filter changes
            if (IsLoaded)
            {
                GenerateReport();
            }
        }
    }
}