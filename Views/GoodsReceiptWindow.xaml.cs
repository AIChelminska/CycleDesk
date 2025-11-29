using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CycleDesk.Models;
using CycleDesk.Services;
using Microsoft.Win32;

namespace CycleDesk
{
    public partial class GoodsReceiptWindow : Window
    {
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;

        // Serwis i dane
        private GoodsReceiptService _receiptService;
        private List<GoodsReceiptDisplayDto> _receipts;
        private List<Supplier> _suppliers;
        private List<User> _users;
        private List<Product> _products;
        private List<Product> _filteredProducts;

        // Lista produktów w nowym paragonie
        private ObservableCollection<GoodsReceiptItemDisplayDto> _receiptProducts;

        // Tryb edycji
        private bool _isEditMode = false;
        private int _editingReceiptId = 0;

        public GoodsReceiptWindow(string username, string password, string fullName, string role)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _fullName = fullName;
            _role = role;

            // Inicjalizuj serwis
            _receiptService = new GoodsReceiptService();
            _receiptProducts = new ObservableCollection<GoodsReceiptItemDisplayDto>();
            _filteredProducts = new List<Product>();

            // Inicjalizuj SideMenuControl
            sideMenu.Initialize(fullName, role);
            sideMenu.SetActiveMenu("Inventory", "GoodsReceipt");

            // Podłącz eventy menu
            ConnectMenuEvents();

            // Podłącz DataGrid do listy produktów
            dgProducts.ItemsSource = _receiptProducts;

            // Załaduj dane po załadowaniu okna
            this.Loaded += GoodsReceiptWindow_Loaded;
        }

        private void GoodsReceiptWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // WAŻNE: Najpierw ładuj produkty, bo LoadSuppliers może wywołać event
            // który potrzebuje _products
            LoadProducts();
            LoadUsers();
            LoadSuppliers();
            LoadReceipts();
        }

        // ===== ŁADOWANIE DANYCH =====

        private void LoadSuppliers()
        {
            try
            {
                _suppliers = _receiptService.GetSuppliers() ?? new List<Supplier>();

                // ComboBox filtra (na głównej stronie)
                if (cmbSupplier != null)
                {
                    cmbSupplier.Items.Clear();
                    cmbSupplier.Items.Add(new ComboBoxItem { Content = "All Suppliers", Tag = 0, IsSelected = true });

                    foreach (var supplier in _suppliers)
                    {
                        cmbSupplier.Items.Add(new ComboBoxItem { Content = supplier.SupplierName, Tag = supplier.SupplierId });
                    }
                }

                // ComboBox modalu
                if (modalCmbSupplier != null)
                {
                    modalCmbSupplier.Items.Clear();
                    modalCmbSupplier.Items.Add(new ComboBoxItem { Content = "Select supplier...", Tag = 0, IsSelected = true });

                    foreach (var supplier in _suppliers)
                    {
                        modalCmbSupplier.Items.Add(new ComboBoxItem { Content = supplier.SupplierName, Tag = supplier.SupplierId });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in LoadSuppliers:\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUsers()
        {
            try
            {
                _users = _receiptService.GetUsers();

                // ComboBox filtra Created By
                cmbCreatedBy.Items.Clear();
                cmbCreatedBy.Items.Add(new ComboBoxItem { Content = "All Users", Tag = 0, IsSelected = true });
                foreach (var user in _users)
                {
                    cmbCreatedBy.Items.Add(new ComboBoxItem { Content = user.FullName, Tag = user.UserId });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProducts()
        {
            try
            {
                _products = _receiptService.GetProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateProductsForSupplier(int supplierId)
        {
            productModalCmbProduct.Items.Clear();
            productModalCmbProduct.Items.Add(new ComboBoxItem { Content = "Select product...", Tag = 0, IsSelected = true });

            // Sprawdź czy produkty są już załadowane
            if (_products == null)
                return;

            if (supplierId > 0)
            {
                _filteredProducts = _products.Where(p => p.SupplierId == supplierId).ToList();
            }
            else
            {
                _filteredProducts = _products.ToList();
            }

            foreach (var product in _filteredProducts)
            {
                productModalCmbProduct.Items.Add(new ComboBoxItem
                {
                    Content = $"{product.SKU} - {product.ProductName}",
                    Tag = product.ProductId
                });
            }
        }

        private void LoadReceipts()
        {
            try
            {
                // Pobierz KPI
                var kpi = _receiptService.GetKpi();
                UpdateKpiCards(kpi);

                // Pobierz wszystkie przyjęcia
                _receipts = _receiptService.GetAllReceipts();

                // Wyświetl w tabeli
                DisplayReceipts(_receipts);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading receipts: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateKpiCards(GoodsReceiptKpiDto kpi)
        {
            txtPendingCount.Text = kpi.PendingCount.ToString();
            txtRejectedCount.Text = kpi.CancelledCount.ToString();
            txtApprovedCount.Text = kpi.ReceivedCount.ToString();
            txtTotalValue.Text = $"{kpi.TotalValueThisMonth:N0} PLN";
        }

        private void DisplayReceipts(List<GoodsReceiptDisplayDto> receipts)
        {
            receiptsContainer.Children.Clear();

            if (receipts == null || receipts.Count == 0)
            {
                var emptyMessage = new TextBlock
                {
                    Text = "No goods receipts found.",
                    FontSize = 14,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 30, 0, 30)
                };
                receiptsContainer.Children.Add(emptyMessage);
                return;
            }

            foreach (var receipt in receipts)
            {
                var row = CreateReceiptRow(receipt);
                receiptsContainer.Children.Add(row);
            }
        }

        private Border CreateReceiptRow(GoodsReceiptDisplayDto receipt)
        {
            var rowBorder = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDEE2E6")),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Background = Brushes.White
            };

            var grid = new Grid { Height = 60 };

            for (int i = 0; i < 7; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            // Column 0 - Doc No.
            var docNoCell = CreateCell(0, true);
            docNoCell.Child = new TextBlock
            {
                Text = receipt.ReceiptNumber,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            grid.Children.Add(docNoCell);

            // Column 1 - Supplier
            var supplierCell = CreateCell(1, true);
            supplierCell.Child = new TextBlock
            {
                Text = receipt.SupplierName,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            grid.Children.Add(supplierCell);

            // Column 2 - Date
            var dateCell = CreateCell(2, true);
            dateCell.Child = new TextBlock
            {
                Text = receipt.ReceiptDate.ToString("dd.MM.yyyy"),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            grid.Children.Add(dateCell);

            // Column 3 - Value
            var valueCell = CreateCell(3, true);
            valueCell.Child = new TextBlock
            {
                Text = $"{receipt.TotalAmount:N2} PLN",
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            grid.Children.Add(valueCell);

            // Column 4 - Items
            var itemsCell = CreateCell(4, true);
            itemsCell.Child = new TextBlock
            {
                Text = receipt.ItemCount.ToString(),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            grid.Children.Add(itemsCell);

            // Column 5 - Status
            var statusCell = CreateCell(5, true);
            var statusBadge = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(receipt.StatusBackground)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(8, 4, 8, 4),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Height = 28
            };
            statusBadge.Child = new TextBlock
            {
                Text = receipt.Status,
                FontSize = 11,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(receipt.StatusColor)),
                FontWeight = FontWeights.SemiBold
            };
            statusCell.Child = statusBadge;
            grid.Children.Add(statusCell);

            // Column 6 - Created By
            var createdByCell = CreateCell(6, false);
            createdByCell.Child = new TextBlock
            {
                Text = receipt.ReceivedByName,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            grid.Children.Add(createdByCell);

            rowBorder.Child = grid;

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
            var cell = new Border { Padding = new Thickness(15, 0, 15, 0) };

            if (hasBorder)
            {
                cell.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDEE2E6"));
                cell.BorderThickness = new Thickness(0, 0, 1, 0);
            }

            Grid.SetColumn(cell, column);
            return cell;
        }

        // ===== FILTROWANIE =====

        private void ApplyFilters()
        {
            try
            {
                var searchTerm = txtSearch.Text?.Trim();

                int? supplierId = null;
                if (cmbSupplier.SelectedItem is ComboBoxItem suppItem && suppItem.Tag != null)
                {
                    var tagValue = Convert.ToInt32(suppItem.Tag);
                    if (tagValue > 0) supplierId = tagValue;
                }

                string status = null;
                if (cmbStatus.SelectedItem is ComboBoxItem statusItem)
                {
                    status = statusItem.Content?.ToString();
                }

                int? createdByUserId = null;
                if (cmbCreatedBy.SelectedItem is ComboBoxItem userItem && userItem.Tag != null)
                {
                    var tagValue = Convert.ToInt32(userItem.Tag);
                    if (tagValue > 0) createdByUserId = tagValue;
                }

                _receipts = _receiptService.Filter(searchTerm, supplierId, status, createdByUserId);
                DisplayReceipts(_receipts);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filtering data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded) ApplyFilters();
        }

        private void cmbSupplier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) ApplyFilters();
        }

        private void cmbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) ApplyFilters();
        }

        private void cmbCreatedBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) ApplyFilters();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            cmbSupplier.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
            cmbCreatedBy.SelectedIndex = 0;
            LoadReceipts();
        }

        // ===== EXPORT CSV =====

        private void ExportCSV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_receipts == null || _receipts.Count == 0)
                {
                    MessageBox.Show("No data to export.", "Export CSV", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    DefaultExt = ".csv",
                    FileName = $"GoodsReceipts_{DateTime.Now:yyyy-MM-dd}"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var csv = new StringBuilder();
                    csv.AppendLine("Receipt Number,Supplier,Date,Value,Items,Status,Created By");

                    foreach (var receipt in _receipts)
                    {
                        csv.AppendLine($"\"{receipt.ReceiptNumber}\",\"{receipt.SupplierName}\"," +
                                      $"{receipt.ReceiptDate:yyyy-MM-dd},{receipt.TotalAmount:F2}," +
                                      $"{receipt.ItemCount},\"{receipt.Status}\",\"{receipt.ReceivedByName}\"");
                    }

                    File.WriteAllText(saveDialog.FileName, csv.ToString(), Encoding.UTF8);

                    MessageBox.Show($"Successfully exported {_receipts.Count} receipts.", "Export Complete",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting CSV: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===== MODAL - NOWE PRZYJĘCIE =====

        private void AddNewReceipt_Click(object sender, RoutedEventArgs e)
        {
            _isEditMode = false;
            _editingReceiptId = 0;

            modalTxtReceiptNo.Text = _receiptService.GenerateReceiptNumber();
            modalDatePickerDocument.SelectedDate = DateTime.Now;
            modalTxtReceivedBy.Text = _fullName;

            modalTxtSupplierInvoiceNo.Clear();
            modalDatePickerSupplierInvoice.SelectedDate = null;
            modalCmbSupplier.SelectedIndex = 0;
            modalTxtTotalValue.Text = "0.00";
            modalTxtNotes.Clear();
            modalToggleSwitch.IsChecked = false;
            modalWarningRejected.Visibility = Visibility.Collapsed;

            _receiptProducts.Clear();

            productModalCmbProduct.Items.Clear();
            productModalCmbProduct.Items.Add(new ComboBoxItem { Content = "Select supplier first...", Tag = 0, IsSelected = true });

            if (modalStatusLabel != null)
            {
                modalStatusLabel.Text = "Received";
                modalStatusLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF059669"));
            }

            modalOverlay.Visibility = Visibility.Visible;
        }

        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            modalOverlay.Visibility = Visibility.Collapsed;
        }

        private void ModalCmbSupplier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (modalCmbSupplier.SelectedItem is ComboBoxItem supplierItem && supplierItem.Tag != null)
            {
                int supplierId = Convert.ToInt32(supplierItem.Tag);
                UpdateProductsForSupplier(supplierId);

                _receiptProducts.Clear();
                UpdateTotalValue();
            }
        }

        private void SaveReceipt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Walidacja
                if (modalCmbSupplier.SelectedIndex == 0)
                {
                    MessageBox.Show("Please select a supplier.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(modalTxtSupplierInvoiceNo.Text))
                {
                    MessageBox.Show("Please enter supplier invoice number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_receiptProducts.Count == 0)
                {
                    MessageBox.Show("Please add at least one product.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Pobierz dane
                var supplierItem = modalCmbSupplier.SelectedItem as ComboBoxItem;
                int supplierId = Convert.ToInt32(supplierItem.Tag);

                var currentUser = _users.FirstOrDefault(u => u.FullName == _fullName);
                int userId = currentUser?.UserId ?? 1;

                decimal totalAmount = _receiptProducts.Sum(p => p.TotalPrice);

                DateTime receiptDate = modalDatePickerDocument.SelectedDate ?? DateTime.Now;
                DateTime? deliveryDate = modalDatePickerSupplierInvoice.SelectedDate;

                // Status: "Received" lub "Cancelled"
                var receipt = new GoodsReceipt
                {
                    ReceiptNumber = modalTxtReceiptNo.Text ?? "",
                    SupplierId = supplierId,
                    ReceiptDate = receiptDate,
                    DeliveryDate = deliveryDate ?? receiptDate,
                    InvoiceNumber = modalTxtSupplierInvoiceNo.Text ?? "",
                    TotalAmount = totalAmount,
                    Notes = modalTxtNotes.Text ?? "",
                    ReceivedByUserId = userId,
                    Status = modalToggleSwitch.IsChecked == true ? "Cancelled" : "Received",
                    CreatedDate = DateTime.Now
                };

                var items = _receiptProducts.Select(p => new GoodsReceiptItem
                {
                    ProductId = p.ProductId,
                    Quantity = p.Quantity,
                    UnitPrice = p.UnitPrice,
                    TotalPrice = p.TotalPrice,
                    BatchNumber = p.BatchNumber ?? "",
                    ExpiryDate = null
                }).ToList();

                _receiptService.CreateGoodsReceipt(receipt, items);

                MessageBox.Show($"Receipt {receipt.ReceiptNumber} saved successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                modalOverlay.Visibility = Visibility.Collapsed;
                LoadReceipts();
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += "\n\nInner Exception: " + ex.InnerException.Message;
                }
                MessageBox.Show($"Error saving receipt: {errorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            if (modalWarningRejected != null)
                modalWarningRejected.Visibility = Visibility.Visible;

            if (modalStatusLabel != null)
            {
                modalStatusLabel.Text = "Cancelled";
                modalStatusLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDC3545"));
            }
        }

        private void ToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            if (modalWarningRejected != null)
                modalWarningRejected.Visibility = Visibility.Collapsed;

            if (modalStatusLabel != null)
            {
                modalStatusLabel.Text = "Received";
                modalStatusLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF059669"));
            }
        }

        // ===== MODAL - DODAWANIE PRODUKTÓW =====

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (modalCmbSupplier.SelectedIndex == 0)
            {
                MessageBox.Show("Please select a supplier first.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            productModalOverlay.Visibility = Visibility.Visible;
            productModalCmbProduct.SelectedIndex = 0;
            productModalTxtQuantity.Clear();
            productModalTxtPurchasePrice.Clear();
        }

        private void CloseProductModal_Click(object sender, RoutedEventArgs e)
        {
            productModalOverlay.Visibility = Visibility.Collapsed;
        }

        private void ProductModalCmbProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (productModalCmbProduct.SelectedItem is ComboBoxItem productItem && productItem.Tag != null)
            {
                int productId = Convert.ToInt32(productItem.Tag);
                if (productId > 0)
                {
                    var product = _filteredProducts.FirstOrDefault(p => p.ProductId == productId);
                    if (product != null)
                    {
                        productModalTxtPurchasePrice.Text = product.PurchasePrice.ToString("N2");
                    }
                }
            }
        }

        private void AddProductToReceipt_Click(object sender, RoutedEventArgs e)
        {
            if (productModalCmbProduct.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select a product.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(productModalTxtQuantity.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Please enter a valid quantity.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string priceText = productModalTxtPurchasePrice.Text.Replace(',', '.');
            if (!decimal.TryParse(priceText, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal price) || price <= 0)
            {
                MessageBox.Show("Please enter a valid price.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var productItem = productModalCmbProduct.SelectedItem as ComboBoxItem;
            int productId = Convert.ToInt32(productItem.Tag);
            var product = _filteredProducts.FirstOrDefault(p => p.ProductId == productId);

            _receiptProducts.Add(new GoodsReceiptItemDisplayDto
            {
                ProductId = productId,
                ProductName = product?.ProductName ?? "Unknown",
                SKU = product?.SKU ?? "",
                Quantity = qty,
                UnitPrice = price,
                TotalPrice = qty * price,
                BatchNumber = ""
            });

            productModalOverlay.Visibility = Visibility.Collapsed;
            UpdateTotalValue();
        }

        private void RemoveProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is GoodsReceiptItemDisplayDto item)
            {
                _receiptProducts.Remove(item);
                UpdateTotalValue();
            }
        }

        private void UpdateTotalValue()
        {
            decimal total = _receiptProducts.Sum(p => p.TotalPrice);
            modalTxtTotalValue.Text = total.ToString("N2");
        }

        // ===== MENU EVENTS =====

        private void ConnectMenuEvents()
        {
            sideMenu.DashboardClicked += (s, e) => { new MainDashboardWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.ProductsClicked += (s, e) => { new ProductsWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.CategoriesClicked += (s, e) => { new CategoriesWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.InventoryStatusClicked += (s, e) => { new InventoryStatusWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.GoodsReceiptClicked += (s, e) => { /* Already here */ };
            sideMenu.SuppliersClicked += (s, e) => { new SuppliersWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.NewSaleClicked += (s, e) => { new NewSaleWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.SalesHistoryClicked += (s, e) => { new SalesHistoryWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.InvoicesClicked += (s, e) => { new InvoicesWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.SalesReportsClicked += (s, e) => { new SalesReportsWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.InventoryReportsClicked += (s, e) => { new InventoryReportsWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.ProductsToOrderClicked += (s, e) => { new ProductsToOrderWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.UsersClicked += (s, e) => { new UsersWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.SettingsClicked += (s, e) => { new SettingsWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.LogoutClicked += (s, e) => HandleLogout();
        }

        private void HandleLogout()
        {
            if (MessageBox.Show("Are you sure you want to logout?", "Confirm Logout",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                new MainWindow().Show();
                Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _receiptService?.Dispose();
            base.OnClosed(e);
        }
    }
}