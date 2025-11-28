using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CycleDesk
{
    public partial class ProductsToOrderWindow : Window
    {
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;

        private List<ProductToOrder> _allProductsToOrder;
        private ObservableCollection<ProductToOrder> _filteredProducts;

        public ProductsToOrderWindow(string username, string password, string fullName, string role)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _fullName = fullName;
            _role = role;

            // Inicjalizuj SideMenuControl
            sideMenu.Initialize(fullName, role);
            sideMenu.SetActiveMenu("Reports", "ProductsToOrder");

            // Podłącz eventy menu
            ConnectMenuEvents();

            // Initialize data
            LoadProductsToOrder();
            InitializeFilters();
            ApplyFilters();
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
                new InventoryReportsWindow(_username, _password, _fullName, _role).Show();
                Close();
            };

            sideMenu.ProductsToOrderClicked += (s, e) =>
            {
                // Już jesteśmy na tej stronie - nic nie rób
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
        public class ProductToOrder
        {
            public string SKU { get; set; }
            public string ProductName { get; set; }
            public string Category { get; set; }
            public int CurrentStock { get; set; }
            public int MinimumStock { get; set; }
            public int RecommendedOrder => Math.Max(0, (MinimumStock * 2) - CurrentStock); // Order to reach 2x minimum
            public decimal UnitPrice { get; set; }
            public string UnitPriceFormatted => $"PLN {UnitPrice:N2}";
            public decimal TotalCost => RecommendedOrder * UnitPrice;
            public string TotalCostFormatted => $"PLN {TotalCost:N2}";
            public string Status
            {
                get
                {
                    if (CurrentStock == 0) return "Out of Stock";
                    if (CurrentStock < MinimumStock) return "Low Stock";
                    return "Normal";
                }
            }
            public SolidColorBrush StatusColor
            {
                get
                {
                    if (CurrentStock == 0) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDC3545"));
                    if (CurrentStock < MinimumStock) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFC107"));
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF28A745"));
                }
            }
            public int Priority
            {
                get
                {
                    if (CurrentStock == 0) return 3; // Highest priority
                    if (CurrentStock < MinimumStock / 2) return 2; // High priority
                    if (CurrentStock < MinimumStock) return 1; // Medium priority
                    return 0; // Low priority
                }
            }
            public string PriorityText
            {
                get
                {
                    switch (Priority)
                    {
                        case 3: return "🔴 Critical";
                        case 2: return "🟠 High";
                        case 1: return "🟡 Medium";
                        default: return "🟢 Low";
                    }
                }
            }
            public bool IsSelected { get; set; }
        }

        // ===== MOCK DATA =====
        private void LoadProductsToOrder()
        {
            _allProductsToOrder = new List<ProductToOrder>
            {
                // Out of Stock - Critical
                new ProductToOrder { SKU = "PEDA-001", ProductName = "Pedals Pro", Category = "Parts", CurrentStock = 0, MinimumStock = 8, UnitPrice = 65.00m },
                new ProductToOrder { SKU = "APPR-005", ProductName = "Rain Jacket", Category = "Apparel", CurrentStock = 0, MinimumStock = 6, UnitPrice = 149.99m },
                
                // Very Low Stock - High Priority
                new ProductToOrder { SKU = "TIRE-002", ProductName = "Tire 28 inch", Category = "Parts", CurrentStock = 2, MinimumStock = 10, UnitPrice = 48.00m },
                new ProductToOrder { SKU = "TIRE-001", ProductName = "Tire 26 inch", Category = "Parts", CurrentStock = 3, MinimumStock = 10, UnitPrice = 45.00m },
                new ProductToOrder { SKU = "TOOL-004", ProductName = "Bike Work Stand", Category = "Tools", CurrentStock = 3, MinimumStock = 2, UnitPrice = 299.00m },
                
                // Low Stock - Medium Priority
                new ProductToOrder { SKU = "ACCS-001", ProductName = "Bike Lock Premium", Category = "Accessories", CurrentStock = 8, MinimumStock = 10, UnitPrice = 89.99m },
                new ProductToOrder { SKU = "APPR-004", ProductName = "Cycling Shoes", Category = "Apparel", CurrentStock = 6, MinimumStock = 8, UnitPrice = 199.99m },
                new ProductToOrder { SKU = "BIKE-002", ProductName = "Road Bike Elite", Category = "Bicycles", CurrentStock = 3, MinimumStock = 3, UnitPrice = 1799.00m },
                new ProductToOrder { SKU = "ACCS-006", ProductName = "Bike Stand", Category = "Accessories", CurrentStock = 5, MinimumStock = 5, UnitPrice = 400.00m },
                new ProductToOrder { SKU = "TOOL-002", ProductName = "Tire Pump", Category = "Tools", CurrentStock = 9, MinimumStock = 8, UnitPrice = 79.00m },
                new ProductToOrder { SKU = "BRAK-001", ProductName = "Brake Pads Set", Category = "Parts", CurrentStock = 18, MinimumStock = 15, UnitPrice = 35.00m },
                new ProductToOrder { SKU = "TOOL-001", ProductName = "Repair Kit", Category = "Tools", CurrentStock = 14, MinimumStock = 10, UnitPrice = 45.00m },
                new ProductToOrder { SKU = "APPR-003", ProductName = "Cycling Shorts", Category = "Apparel", CurrentStock = 15, MinimumStock = 12, UnitPrice = 89.99m },
                new ProductToOrder { SKU = "HELM-001", ProductName = "Safety Helmet Pro", Category = "Accessories", CurrentStock = 25, MinimumStock = 15, UnitPrice = 149.00m },
                new ProductToOrder { SKU = "CHAI-001", ProductName = "Bike Chain", Category = "Parts", CurrentStock = 22, MinimumStock = 12, UnitPrice = 28.00m }
            };

            _filteredProducts = new ObservableCollection<ProductToOrder>(_allProductsToOrder);
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

            // Priority filter
            cmbPriority.Items.Add("All Priorities");
            cmbPriority.Items.Add("Critical");
            cmbPriority.Items.Add("High");
            cmbPriority.Items.Add("Medium");
            cmbPriority.Items.Add("Low");
            cmbPriority.SelectedIndex = 0;
        }

        // ===== FILTERING =====
        private void ApplyFilters()
        {
            var filtered = _allProductsToOrder.AsEnumerable();

            // Category filter
            string selectedCategory = cmbCategory.SelectedItem?.ToString() ?? "All Categories";
            if (selectedCategory != "All Categories")
            {
                filtered = filtered.Where(p => p.Category == selectedCategory);
            }

            // Priority filter
            string selectedPriority = cmbPriority.SelectedItem?.ToString() ?? "All Priorities";
            if (selectedPriority != "All Priorities")
            {
                int priorityLevel = selectedPriority switch
                {
                    "Critical" => 3,
                    "High" => 2,
                    "Medium" => 1,
                    "Low" => 0,
                    _ => -1
                };
                if (priorityLevel >= 0)
                {
                    filtered = filtered.Where(p => p.Priority == priorityLevel);
                }
            }

            // Sort by priority (highest first), then by product name
            filtered = filtered.OrderByDescending(p => p.Priority).ThenBy(p => p.ProductName);

            _filteredProducts = new ObservableCollection<ProductToOrder>(filtered);

            // Update UI
            UpdateKPIs();
            GenerateProductTable();
        }

        private void UpdateKPIs()
        {
            // Total Products to Order
            txtTotalProducts.Text = _filteredProducts.Count.ToString();

            // Critical Items
            int criticalCount = _filteredProducts.Count(p => p.Priority == 3);
            txtCriticalItems.Text = criticalCount.ToString();

            // Total Order Value
            decimal totalValue = _filteredProducts.Sum(p => p.TotalCost);
            txtTotalValue.Text = $"PLN {totalValue:N2}";

            // Total Units to Order
            int totalUnits = _filteredProducts.Sum(p => p.RecommendedOrder);
            txtTotalUnits.Text = totalUnits.ToString();
        }

        private void GenerateProductTable()
        {
            // Clear existing rows (keep header)
            while (productsPanel.Children.Count > 1)
            {
                productsPanel.Children.RemoveAt(1);
            }

            // Add product rows
            foreach (var product in _filteredProducts)
            {
                AddProductRow(product);
            }

            // Update selection summary
            UpdateSelectionSummary();
        }

        private void AddProductRow(ProductToOrder product)
        {
            var border = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDEE2E6")),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Background = Brushes.White,
                Padding = new Thickness(0, 12, 0, 12)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.8, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.8, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.8, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });

            // Checkbox
            var checkbox = new CheckBox
            {
                IsChecked = product.IsSelected,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Tag = product
            };
            checkbox.Checked += ProductCheckbox_Changed;
            checkbox.Unchecked += ProductCheckbox_Changed;
            Grid.SetColumn(checkbox, 0);
            grid.Children.Add(checkbox);

            // Priority
            var txtPriority = new TextBlock
            {
                Text = product.PriorityText,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtPriority, 1);
            grid.Children.Add(txtPriority);

            // Product Name
            var stackProduct = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var txtName = new TextBlock
            {
                Text = product.ProductName,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                FontWeight = FontWeights.Medium
            };
            var txtSKU = new TextBlock
            {
                Text = product.SKU,
                FontSize = 11,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                Margin = new Thickness(0, 2, 0, 0)
            };
            stackProduct.Children.Add(txtName);
            stackProduct.Children.Add(txtSKU);
            Grid.SetColumn(stackProduct, 2);
            grid.Children.Add(stackProduct);

            // Category
            var txtCategory = new TextBlock
            {
                Text = product.Category,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtCategory, 3);
            grid.Children.Add(txtCategory);

            // Current Stock
            var txtCurrent = new TextBlock
            {
                Text = product.CurrentStock.ToString(),
                FontSize = 13,
                Foreground = product.StatusColor,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtCurrent, 4);
            grid.Children.Add(txtCurrent);

            // Min Stock
            var txtMin = new TextBlock
            {
                Text = product.MinimumStock.ToString(),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtMin, 5);
            grid.Children.Add(txtMin);

            // Recommended Order
            var txtOrder = new TextBlock
            {
                Text = product.RecommendedOrder.ToString(),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF28A745")),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtOrder, 6);
            grid.Children.Add(txtOrder);

            // Unit Price
            var txtPrice = new TextBlock
            {
                Text = product.UnitPriceFormatted,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtPrice, 7);
            grid.Children.Add(txtPrice);

            // Total Cost
            var txtTotal = new TextBlock
            {
                Text = product.TotalCostFormatted,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtTotal, 8);
            grid.Children.Add(txtTotal);

            // Status Badge
            var statusBorder = new Border
            {
                Background = product.CurrentStock == 0
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8D7DA"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFEF3CD")),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(8, 4, 8, 4),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var txtStatus = new TextBlock
            {
                Text = product.Status,
                FontSize = 11,
                Foreground = product.StatusColor,
                FontWeight = FontWeights.SemiBold
            };
            statusBorder.Child = txtStatus;
            Grid.SetColumn(statusBorder, 9);
            grid.Children.Add(statusBorder);

            border.Child = grid;
            productsPanel.Children.Add(border);
        }

        private void ProductCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            var product = checkbox?.Tag as ProductToOrder;
            if (product != null)
            {
                product.IsSelected = checkbox.IsChecked ?? false;
                UpdateSelectionSummary();
            }
        }

        private void UpdateSelectionSummary()
        {
            var selectedProducts = _filteredProducts.Where(p => p.IsSelected).ToList();

            txtSelectedCount.Text = selectedProducts.Count.ToString();

            int selectedUnits = selectedProducts.Sum(p => p.RecommendedOrder);
            txtSelectedUnits.Text = selectedUnits.ToString();

            decimal selectedValue = selectedProducts.Sum(p => p.TotalCost);
            txtSelectedValue.Text = $"PLN {selectedValue:N2}";
        }

        // ===== ACTION HANDLERS =====
        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var product in _filteredProducts)
            {
                product.IsSelected = true;
            }
            GenerateProductTable();
        }

        private void SelectNone_Click(object sender, RoutedEventArgs e)
        {
            foreach (var product in _filteredProducts)
            {
                product.IsSelected = false;
            }
            GenerateProductTable();
        }

        private void SelectCritical_Click(object sender, RoutedEventArgs e)
        {
            foreach (var product in _filteredProducts)
            {
                product.IsSelected = product.Priority == 3;
            }
            GenerateProductTable();
        }

        private void GeneratePurchaseOrder_Click(object sender, RoutedEventArgs e)
        {
            var selectedProducts = _filteredProducts.Where(p => p.IsSelected).ToList();

            if (selectedProducts.Count == 0)
            {
                MessageBox.Show("Please select at least one product to order.", "No Products Selected",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Calculate summary
            int totalProducts = selectedProducts.Count;
            int totalUnits = selectedProducts.Sum(p => p.RecommendedOrder);
            decimal totalValue = selectedProducts.Sum(p => p.TotalCost);

            // Show confirmation
            string message = $"Generate Purchase Order?\n\n" +
                           $"Products: {totalProducts}\n" +
                           $"Total Units: {totalUnits}\n" +
                           $"Total Value: PLN {totalValue:N2}\n\n" +
                           $"This will create a new purchase order document.";

            var result = MessageBox.Show(message, "Confirm Purchase Order",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // TODO: Implement actual purchase order generation
                MessageBox.Show($"Purchase Order generated successfully!\n\n" +
                              $"Order Number: PO/2025/{DateTime.Now:MMddHHmm}\n" +
                              $"Products: {totalProducts}\n" +
                              $"Total Value: PLN {totalValue:N2}",
                              "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear selections
                SelectNone_Click(null, null);
            }
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                ApplyFilters();
            }
        }
    }
}