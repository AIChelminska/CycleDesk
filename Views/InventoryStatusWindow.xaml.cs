using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using CycleDesk.Models;
using CycleDesk.Services;
using Microsoft.Win32;

namespace CycleDesk
{
    public partial class InventoryStatusWindow : Window
    {
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;

        // Serwis i dane
        private InventoryService _inventoryService;
        private List<InventoryStatusDto> _inventoryItems;
        private List<Category> _categories;

        public InventoryStatusWindow(string username, string password, string fullName, string role)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _fullName = fullName;
            _role = role;

            // Inicjalizuj serwis
            _inventoryService = new InventoryService();

            // Inicjalizuj SideMenuControl
            sideMenu.Initialize(fullName, role);
            sideMenu.SetActiveMenu("Inventory", "InventoryStatus");

            // Podłącz eventy menu
            ConnectMenuEvents();

            // Załaduj dane
            LoadCategories();
            LoadInventoryData();
        }

        // ===== ŁADOWANIE DANYCH =====

        private void LoadCategories()
        {
            try
            {
                _categories = _inventoryService.GetCategories();

                cmbCategory.Items.Clear();
                cmbCategory.Items.Add(new ComboBoxItem { Content = "All Categories", Tag = 0, IsSelected = true });

                foreach (var category in _categories)
                {
                    cmbCategory.Items.Add(new ComboBoxItem
                    {
                        Content = category.CategoryName,
                        Tag = category.CategoryId
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadInventoryData()
        {
            try
            {
                // Pobierz KPI
                var kpi = _inventoryService.GetInventoryKpi();
                UpdateKpiCards(kpi);

                // Pobierz wszystkie produkty
                _inventoryItems = _inventoryService.GetAllInventoryStatus();

                // Wyświetl w tabeli
                DisplayInventoryItems(_inventoryItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading inventory data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateKpiCards(InventoryKpiDto kpi)
        {
            // Out of Stock
            txtOutOfStockCount.Text = kpi.OutOfStockCount.ToString();

            // Critical (pokazujemy razem z Out of Stock lub osobno)
            // Low Stock
            txtLowStockCount.Text = (kpi.LowStockCount + kpi.CriticalCount).ToString();

            // In Stock
            txtInStockCount.Text = kpi.InStockCount.ToString();

            // Total
            txtTotalCount.Text = kpi.TotalProducts.ToString();
        }

        private void DisplayInventoryItems(List<InventoryStatusDto> items)
        {
            inventoryContainer.Children.Clear();

            if (items == null || items.Count == 0)
            {
                var emptyMessage = new TextBlock
                {
                    Text = "No products found matching your criteria.",
                    FontSize = 14,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 30, 0, 30)
                };
                inventoryContainer.Children.Add(emptyMessage);
                return;
            }

            foreach (var item in items)
            {
                var row = CreateInventoryRow(item);
                inventoryContainer.Children.Add(row);
            }
        }

        private Border CreateInventoryRow(InventoryStatusDto item)
        {
            var rowBorder = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDEE2E6")),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Background = Brushes.White
            };

            var grid = new Grid { Height = 60 };

            // Definicje kolumn (takie same jak w XAML header)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });   // Image
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });   // SKU
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });   // Product Name
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });   // Category
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });   // Stock
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });   // Min
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });   // Status

            // Column 0 - Image
            var imageCell = CreateCell(0, true);
            var imageContainer = new Border
            {
                Width = 40,
                Height = 40,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8F9FA")),
                CornerRadius = new CornerRadius(5)
            };
            imageContainer.Child = new TextBlock
            {
                Text = item.CategoryEmoji,
                FontSize = 24,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            imageCell.Child = imageContainer;
            grid.Children.Add(imageCell);

            // Column 1 - SKU
            var skuCell = CreateCell(1, true);
            skuCell.Child = new TextBlock
            {
                Text = item.SKU,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            grid.Children.Add(skuCell);

            // Column 2 - Product Name
            var nameCell = CreateCell(2, true);
            nameCell.Child = new TextBlock
            {
                Text = item.ProductName,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            grid.Children.Add(nameCell);

            // Column 3 - Category
            var categoryCell = CreateCell(3, true);
            categoryCell.Child = new TextBlock
            {
                Text = item.CategoryName,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            grid.Children.Add(categoryCell);

            // Column 4 - Stock
            var stockCell = CreateCell(4, true);
            var stockColor = item.QuantityInStock == 0 ? "#FFDC3545" :
                            item.QuantityInStock <= item.MinimumStock ? "#FFDC3545" :
                            item.QuantityInStock < item.ReorderLevel ? "#FFFFC107" : "#FF2D3E50";
            stockCell.Child = new TextBlock
            {
                Text = item.QuantityInStock.ToString(),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(stockColor)),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            grid.Children.Add(stockCell);

            // Column 5 - Min (ReorderLevel)
            var minCell = CreateCell(5, true);
            minCell.Child = new TextBlock
            {
                Text = item.ReorderLevel.ToString(),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            grid.Children.Add(minCell);

            // Column 6 - Status Badge
            var statusCell = CreateCell(6, false);
            var statusBadge = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(item.StatusBackground)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(8, 4, 8, 4),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Height = 28
            };

            var statusIcon = item.Status switch
            {
                "Out of Stock" => "⚠",
                "Critical" => "⚠",
                "Low Stock" => "⚠",
                "In Stock" => "✓",
                _ => ""
            };

            statusBadge.Child = new TextBlock
            {
                Text = $"{statusIcon} {item.Status}",
                FontSize = 11,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(item.StatusColor)),
                FontWeight = FontWeights.SemiBold
            };
            statusCell.Child = statusBadge;
            grid.Children.Add(statusCell);

            rowBorder.Child = grid;

            // Hover effect
            rowBorder.MouseEnter += (s, e) =>
            {
                rowBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8F9FA"));
            };
            rowBorder.MouseLeave += (s, e) =>
            {
                rowBorder.Background = Brushes.White;
            };

            return rowBorder;
        }

        private Border CreateCell(int column, bool hasBorder)
        {
            var cell = new Border
            {
                Padding = new Thickness(15, 0, 15, 0)
            };

            if (hasBorder)
            {
                cell.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDEE2E6"));
                cell.BorderThickness = new Thickness(0, 0, 1, 0);
            }

            Grid.SetColumn(cell, column);
            return cell;
        }

        // ===== FILTROWANIE I WYSZUKIWANIE =====

        private void ApplyFilters()
        {
            try
            {
                var searchTerm = txtSearch.Text?.Trim();

                // Pobierz wybraną kategorię
                int? categoryId = null;
                if (cmbCategory.SelectedItem is ComboBoxItem catItem && catItem.Tag != null)
                {
                    var tagValue = Convert.ToInt32(catItem.Tag);
                    if (tagValue > 0)
                        categoryId = tagValue;
                }

                // Pobierz wybrany status
                string status = null;
                if (cmbStatus.SelectedItem is ComboBoxItem statusItem)
                {
                    status = statusItem.Content?.ToString();
                }

                // Filtruj
                _inventoryItems = _inventoryService.Filter(searchTerm, categoryId, status);

                // Wyświetl
                DisplayInventoryItems(_inventoryItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filtering data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void cmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
                ApplyFilters();
        }

        private void cmbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
                ApplyFilters();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset filtrów
            txtSearch.Text = "";
            cmbCategory.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;

            // Przeładuj dane
            LoadInventoryData();
        }

        // ===== EXPORT CSV =====

        private void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_inventoryItems == null || _inventoryItems.Count == 0)
                {
                    MessageBox.Show("No data to export.", "Export CSV",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    DefaultExt = ".csv",
                    FileName = $"InventoryStatus_{DateTime.Now:yyyy-MM-dd}"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var csv = new StringBuilder();

                    // Header
                    csv.AppendLine("SKU,Product Name,Category,Stock,Min Stock,Reorder Level,Status");

                    // Data rows
                    foreach (var item in _inventoryItems)
                    {
                        csv.AppendLine($"\"{item.SKU}\",\"{item.ProductName}\",\"{item.CategoryName}\"," +
                                      $"{item.QuantityInStock},{item.MinimumStock},{item.ReorderLevel},\"{item.Status}\"");
                    }

                    File.WriteAllText(saveDialog.FileName, csv.ToString(), Encoding.UTF8);

                    MessageBox.Show($"Successfully exported {_inventoryItems.Count} items to:\n{saveDialog.FileName}",
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting CSV: {ex.Message}", "Export Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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
                // Już jesteśmy na tej stronie - nic nie rób
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

        protected override void OnClosed(EventArgs e)
        {
            _inventoryService?.Dispose();
            base.OnClosed(e);
        }
    }
}