using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CycleDesk.Models;
using CycleDesk.Services;

namespace CycleDesk
{
    public partial class ProductsWindow : Window
    {
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;
        private Border _currentlySelectedRow = null;

        // Serwis i dane
        private ProductService _productService;
        private List<ProductDisplayDto> _allProducts;
        private List<ProductDisplayDto> _filteredProducts;
        private ProductDisplayDto _selectedProduct;

        // Listy dla ComboBox
        private List<Category> _categories;
        private List<Supplier> _suppliers;

        public ProductsWindow(string username, string password, string fullName, string role)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _fullName = fullName;
            _role = role;

            // Inicjalizuj serwis
            _productService = new ProductService();

            // Inicjalizuj SideMenuControl
            sideMenu.Initialize(fullName, role);
            sideMenu.SetActiveMenu("Inventory", "Products");

            // Podłącz eventy menu
            ConnectMenuEvents();

            // Załaduj dane
            LoadData();

            // Podłącz eventy filtrów
            ConnectFilterEvents();
        }

        // ===== ŁADOWANIE DANYCH =====
        private void LoadData()
        {
            try
            {
                // Załaduj kategorie i dostawców dla filtrów
                _categories = _productService.GetAllCategories();
                _suppliers = _productService.GetAllSuppliers();

                // Wypełnij ComboBox kategorii
                PopulateCategoryFilter();
                PopulateSupplierFilter();
                PopulateModalComboBoxes();

                // Załaduj produkty
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProducts()
        {
            try
            {
                _allProducts = _productService.GetAllProductsWithDetails();
                _filteredProducts = _allProducts;
                DisplayProducts(_filteredProducts);

                // Aktualizuj licznik
                UpdateProductCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateProductCount()
        {
            // Znajdź TextBlock z liczbą produktów i zaktualizuj
            // Szukamy elementu o nazwie np. txtProductCount
            var countText = FindName("txtProductCount") as TextBlock;
            if (countText != null)
            {
                countText.Text = $"({_filteredProducts?.Count ?? 0} products)";
            }
        }

        // ===== WYPEŁNIANIE FILTRÓW =====
        private void PopulateCategoryFilter()
        {
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add(new ComboBoxItem { Content = "All Categories", IsSelected = true });

            foreach (var category in _categories)
            {
                cmbCategory.Items.Add(new ComboBoxItem
                {
                    Content = category.CategoryName,
                    Tag = category.CategoryId
                });
            }
        }

        private void PopulateSupplierFilter()
        {
            cmbSupplier.Items.Clear();
            cmbSupplier.Items.Add(new ComboBoxItem { Content = "All Suppliers", IsSelected = true });

            foreach (var supplier in _suppliers)
            {
                cmbSupplier.Items.Add(new ComboBoxItem
                {
                    Content = supplier.SupplierName,
                    Tag = supplier.SupplierId
                });
            }
        }

        private void PopulateModalComboBoxes()
        {
            // Znajdź ComboBox kategorii w modalu (musisz nadać mu x:Name w XAML!)
            var modalCategoryCombo = FindName("cmbModalCategory") as ComboBox;
            if (modalCategoryCombo != null)
            {
                modalCategoryCombo.Items.Clear();
                modalCategoryCombo.Items.Add(new ComboBoxItem { Content = "Choose", IsSelected = true });
                foreach (var category in _categories)
                {
                    modalCategoryCombo.Items.Add(new ComboBoxItem
                    {
                        Content = category.CategoryName,
                        Tag = category.CategoryId
                    });
                }
            }

            var modalSupplierCombo = FindName("cmbModalSupplier") as ComboBox;
            if (modalSupplierCombo != null)
            {
                modalSupplierCombo.Items.Clear();
                modalSupplierCombo.Items.Add(new ComboBoxItem { Content = "Choose", IsSelected = true });
                foreach (var supplier in _suppliers)
                {
                    modalSupplierCombo.Items.Add(new ComboBoxItem
                    {
                        Content = supplier.SupplierName,
                        Tag = supplier.SupplierId
                    });
                }
            }
        }

        // ===== WYŚWIETLANIE PRODUKTÓW =====
        private void DisplayProducts(List<ProductDisplayDto> products)
        {
            // Znajdź StackPanel z wierszami produktów
            var productsContainer = FindName("productsStackPanel") as StackPanel;
            if (productsContainer == null)
            {
                // Jeśli nie znaleziono po nazwie, spróbuj znaleźć w strukturze
                productsContainer = FindProductsContainer();
            }

            if (productsContainer == null)
            {
                MessageBox.Show("Cannot find products container!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            productsContainer.Children.Clear();

            foreach (var product in products)
            {
                var row = CreateProductRow(product);
                productsContainer.Children.Add(row);
            }
        }

        private StackPanel FindProductsContainer()
        {
            // Szukamy StackPanel który jest w ScrollViewer i zawiera wiersze produktów
            // To jest fallback jeśli nie masz nadanej nazwy x:Name="productsStackPanel"
            return null; // Musisz nadać x:Name w XAML
        }

        private Border CreateProductRow(ProductDisplayDto product)
        {
            var border = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDEE2E6")),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Background = new SolidColorBrush(Colors.White),
                Cursor = Cursors.Hand,
                Tag = product // Przechowaj dane produktu
            };

            border.MouseLeftButtonDown += ProductRow_Click;
            border.MouseLeave += ProductRow_MouseLeave;

            var grid = new Grid { Height = 60 };

            // Definiuj kolumny
            for (int i = 0; i < 10; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            // Kolumna 0: Obrazek
            var imgBorder = CreateCell(0, CreateProductImage(product));
            grid.Children.Add(imgBorder);

            // Kolumna 1: SKU
            var skuBorder = CreateCell(1, CreateTextBlock(product.SKU, true));
            grid.Children.Add(skuBorder);

            // Kolumna 2: Nazwa produktu
            var nameBorder = CreateCell(2, CreateTextBlock(product.ProductName));
            grid.Children.Add(nameBorder);

            // Kolumna 3: Kategoria
            var categoryBorder = CreateCell(3, CreateTextBlock(product.CategoryName, false, true));
            grid.Children.Add(categoryBorder);

            // Kolumna 4: Stock
            var stockBorder = CreateCell(4, CreateTextBlock(product.QuantityInStock.ToString(), true));
            grid.Children.Add(stockBorder);

            // Kolumna 5: Min
            var minBorder = CreateCell(5, CreateTextBlock(product.MinimumStock.ToString(), false, true));
            grid.Children.Add(minBorder);

            // Kolumna 6: Status
            var statusBorder = CreateCell(6, CreateStatusBadge(product));
            grid.Children.Add(statusBorder);

            // Kolumna 7: Purchase Price
            var purchaseBorder = CreateCell(7, CreateTextBlock($"{product.PurchasePrice:N2} PLN"));
            grid.Children.Add(purchaseBorder);

            // Kolumna 8: Selling Price
            var sellingBorder = CreateCell(8, CreateTextBlock($"{product.SellingPrice:N2} PLN", true));
            grid.Children.Add(sellingBorder);

            // Kolumna 9: Supplier (bez prawego obramowania)
            var supplierBorder = CreateCell(9, CreateTextBlock(product.SupplierName, false, true), false);
            grid.Children.Add(supplierBorder);

            border.Child = grid;
            return border;
        }

        private Border CreateCell(int column, UIElement content, bool hasRightBorder = true)
        {
            var border = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDEE2E6")),
                BorderThickness = new Thickness(0, 0, hasRightBorder ? 1 : 0, 0),
                Padding = new Thickness(15, 0, 15, 0)
            };
            Grid.SetColumn(border, column);
            border.Child = content;
            return border;
        }

        private Border CreateProductImage(ProductDisplayDto product)
        {
            var imageBorder = new Border
            {
                Width = 40,
                Height = 40,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8F9FA")),
                CornerRadius = new CornerRadius(5)
            };

            var emoji = new TextBlock
            {
                Text = GetCategoryEmoji(product.CategoryName),
                FontSize = 24,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            imageBorder.Child = emoji;
            return imageBorder;
        }

        private string GetCategoryEmoji(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName)) return "📦";

            var lower = categoryName.ToLower();
            if (lower.Contains("bike") || lower.Contains("rower")) return "🚲";
            if (lower.Contains("wheel") || lower.Contains("koło")) return "⭕";
            if (lower.Contains("tire") || lower.Contains("opona")) return "🔘";
            if (lower.Contains("brake") || lower.Contains("hamulec")) return "🛑";
            if (lower.Contains("light") || lower.Contains("światło")) return "💡";
            if (lower.Contains("helmet") || lower.Contains("kask")) return "⛑️";
            if (lower.Contains("lock") || lower.Contains("zamek")) return "🔒";
            if (lower.Contains("pump") || lower.Contains("pompka")) return "💨";
            if (lower.Contains("tool") || lower.Contains("narzędzie")) return "🔧";

            return "📦";
        }

        private TextBlock CreateTextBlock(string text, bool bold = false, bool gray = false)
        {
            return new TextBlock
            {
                Text = text ?? "",
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(
                    gray ? "#FF6C757D" : "#FF2D3E50")),
                FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
        }

        private Border CreateStatusBadge(ProductDisplayDto product)
        {
            string statusText;
            string bgColor;
            string fgColor;

            switch (product.StockStatus)
            {
                case "In Stock":
                    statusText = "✓ In Stock";
                    bgColor = "#FFD4EDDA";
                    fgColor = "#FF28A745";
                    break;
                case "Low Stock":
                    statusText = "⚠ Low Stock";
                    bgColor = "#FFFFF3CD";
                    fgColor = "#FFC107";
                    break;
                case "Critical":
                    statusText = "⚠ Critical";
                    bgColor = "#FFF8D7DA";
                    fgColor = "#DC3545";
                    break;
                case "Out of Stock":
                    statusText = "✗ Out of Stock";
                    bgColor = "#FFF8D7DA";
                    fgColor = "#DC3545";
                    break;
                default:
                    statusText = product.StockStatus;
                    bgColor = "#FFE9ECEF";
                    fgColor = "#6C757D";
                    break;
            }

            var badge = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgColor)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(8, 4, 8, 4),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Height = 28
            };

            badge.Child = new TextBlock
            {
                Text = statusText,
                FontSize = 11,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fgColor)),
                FontWeight = FontWeights.SemiBold
            };

            return badge;
        }

        // ===== FILTROWANIE =====
        private void ConnectFilterEvents()
        {
            // Wyszukiwarka tekstowa
            var searchBox = FindName("txtSearch") as TextBox;
            if (searchBox != null)
            {
                searchBox.TextChanged += SearchBox_TextChanged;
            }

            // Filtry ComboBox
            cmbCategory.SelectionChanged += Filter_SelectionChanged;
            cmbStatus.SelectionChanged += Filter_SelectionChanged;
            cmbSupplier.SelectionChanged += Filter_SelectionChanged;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allProducts == null) return;

            var searchBox = FindName("txtSearch") as TextBox;
            string searchTerm = searchBox?.Text;

            // Pobierz wartości filtrów
            int? categoryId = null;
            var selectedCategory = cmbCategory.SelectedItem as ComboBoxItem;
            if (selectedCategory?.Tag != null)
            {
                categoryId = (int)selectedCategory.Tag;
            }

            int? supplierId = null;
            var selectedSupplier = cmbSupplier.SelectedItem as ComboBoxItem;
            if (selectedSupplier?.Tag != null)
            {
                supplierId = (int)selectedSupplier.Tag;
            }

            string stockStatus = null;
            var selectedStatus = cmbStatus.SelectedItem as ComboBoxItem;
            if (selectedStatus != null)
            {
                stockStatus = selectedStatus.Content?.ToString();
            }

            // Zastosuj filtry
            _filteredProducts = _productService.FilterProducts(searchTerm, categoryId, supplierId, stockStatus);
            DisplayProducts(_filteredProducts);
            UpdateProductCount();
        }

        private void RefreshProducts_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();

            // Reset filtrów
            cmbCategory.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
            cmbSupplier.SelectedIndex = 0;

            var searchBox = FindName("txtSearch") as TextBox;
            if (searchBox != null) searchBox.Text = "";
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
                // Już jesteśmy na tej stronie - odśwież
                LoadProducts();
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

        // ===== CLICKABLE ROW LOGIC =====
        private void ProductRow_Click(object sender, MouseButtonEventArgs e)
        {
            Border clickedRow = sender as Border;
            if (clickedRow == null) return;

            // Pobierz dane produktu z Tag
            _selectedProduct = clickedRow.Tag as ProductDisplayDto;

            // Resetuj poprzednie podświetlenie
            if (_currentlySelectedRow != null && _currentlySelectedRow != clickedRow)
            {
                _currentlySelectedRow.Background = new SolidColorBrush(Colors.White);
            }

            // Podświetl nowy wiersz
            clickedRow.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8F9FA"));
            _currentlySelectedRow = clickedRow;

            // Pokaż popup z przyciskami
            actionButtonsPopup.PlacementTarget = clickedRow;
            actionButtonsPopup.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
            actionButtonsPopup.HorizontalOffset = 10;
            actionButtonsPopup.VerticalOffset = 5;
            actionButtonsPopup.IsOpen = true;
        }

        private void ProductRow_MouseLeave(object sender, MouseEventArgs e)
        {
            Border row = sender as Border;
            if (row == null) return;

            if (actionButtonsPopup.IsOpen)
            {
                if (!IsMouseOverPopup())
                {
                    actionButtonsPopup.IsOpen = false;

                    if (_currentlySelectedRow != null)
                    {
                        _currentlySelectedRow.Background = new SolidColorBrush(Colors.White);
                        _currentlySelectedRow = null;
                    }
                }
            }
        }

        private bool IsMouseOverPopup()
        {
            if (actionButtonsPopup.Child == null) return false;

            Point mousePos = Mouse.GetPosition(actionButtonsPopup.Child as UIElement);

            if (actionButtonsPopup.Child is FrameworkElement element)
            {
                return mousePos.X >= 0 && mousePos.X <= element.ActualWidth &&
                       mousePos.Y >= 0 && mousePos.Y <= element.ActualHeight;
            }

            return false;
        }

        private void ActionButtonsPopup_Closed(object sender, EventArgs e)
        {
            if (_currentlySelectedRow != null)
            {
                _currentlySelectedRow.Background = new SolidColorBrush(Colors.White);
                _currentlySelectedRow = null;
            }
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProduct == null)
            {
                MessageBox.Show("No product selected!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            actionButtonsPopup.IsOpen = false;

            // Otwórz modal edycji z danymi produktu
            OpenEditModal(_selectedProduct);
        }

        private void OpenEditModal(ProductDisplayDto product)
        {
            // Wypełnij pola modalu danymi produktu
            var txtSKU = FindName("txtModalSKU") as TextBox;
            var txtName = FindName("txtModalName") as TextBox;
            var txtDescription = FindName("txtModalDescription") as TextBox;
            var cmbCategory = FindName("cmbModalCategory") as ComboBox;
            var cmbSupplier = FindName("cmbModalSupplier") as ComboBox;
            var txtPurchasePrice = FindName("txtModalPurchasePrice") as TextBox;
            var txtSellingPrice = FindName("txtModalSellingPrice") as TextBox;
            var txtMinStock = FindName("txtModalMinStock") as TextBox;
            var txtReorderLevel = FindName("txtModalReorderLevel") as TextBox;

            if (txtSKU != null) txtSKU.Text = product.SKU;
            if (txtName != null) txtName.Text = product.ProductName;
            if (txtDescription != null) txtDescription.Text = product.Description;
            if (txtPurchasePrice != null) txtPurchasePrice.Text = product.PurchasePrice.ToString("F2");
            if (txtSellingPrice != null) txtSellingPrice.Text = product.SellingPrice.ToString("F2");
            if (txtMinStock != null) txtMinStock.Text = product.MinimumStock.ToString();
            if (txtReorderLevel != null) txtReorderLevel.Text = product.ReorderLevel.ToString();

            // Ustaw kategorię
            if (cmbCategory != null)
            {
                for (int i = 0; i < cmbCategory.Items.Count; i++)
                {
                    var item = cmbCategory.Items[i] as ComboBoxItem;
                    if (item?.Tag != null && (int)item.Tag == product.CategoryId)
                    {
                        cmbCategory.SelectedIndex = i;
                        break;
                    }
                }
            }

            // Ustaw dostawcę
            if (cmbSupplier != null && product.SupplierId.HasValue)
            {
                for (int i = 0; i < cmbSupplier.Items.Count; i++)
                {
                    var item = cmbSupplier.Items[i] as ComboBoxItem;
                    if (item?.Tag != null && (int)item.Tag == product.SupplierId.Value)
                    {
                        cmbSupplier.SelectedIndex = i;
                        break;
                    }
                }
            }

            // Pokaż modal
            modalOverlay.Visibility = Visibility.Visible;
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProduct == null)
            {
                MessageBox.Show("No product selected!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete:\n\n{_selectedProduct.SKU} - {_selectedProduct.ProductName}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool success = _productService.DeactivateProduct(_selectedProduct.ProductId);

                    if (success)
                    {
                        MessageBox.Show($"Product {_selectedProduct.SKU} deleted successfully!",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        actionButtonsPopup.IsOpen = false;
                        LoadProducts(); // Odśwież listę
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete product.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting product: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }
            return null;
        }

        // ===== MODAL HANDLERS =====
        private void OpenAddProductModal_Click(object sender, RoutedEventArgs e)
        {
            _selectedProduct = null; // Tryb dodawania
            ClearModalFields();

            // Wygeneruj nowe SKU
            var txtSKU = FindName("txtModalSKU") as TextBox;
            if (txtSKU != null)
            {
                txtSKU.Text = _productService.GenerateSKU("PRD");
            }

            modalOverlay.Visibility = Visibility.Visible;
        }

        private void ClearModalFields()
        {
            var txtSKU = FindName("txtModalSKU") as TextBox;
            var txtName = FindName("txtModalName") as TextBox;
            var txtDescription = FindName("txtModalDescription") as TextBox;
            var cmbCategory = FindName("cmbModalCategory") as ComboBox;
            var cmbSupplier = FindName("cmbModalSupplier") as ComboBox;
            var txtPurchasePrice = FindName("txtModalPurchasePrice") as TextBox;
            var txtSellingPrice = FindName("txtModalSellingPrice") as TextBox;
            var txtMinStock = FindName("txtModalMinStock") as TextBox;
            var txtReorderLevel = FindName("txtModalReorderLevel") as TextBox;

            if (txtSKU != null) txtSKU.Text = "";
            if (txtName != null) txtName.Text = "";
            if (txtDescription != null) txtDescription.Text = "";
            if (cmbCategory != null) cmbCategory.SelectedIndex = 0;
            if (cmbSupplier != null) cmbSupplier.SelectedIndex = 0;
            if (txtPurchasePrice != null) txtPurchasePrice.Text = "";
            if (txtSellingPrice != null) txtSellingPrice.Text = "";
            if (txtMinStock != null) txtMinStock.Text = "0";
            if (txtReorderLevel != null) txtReorderLevel.Text = "5";
        }

        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            modalOverlay.Visibility = Visibility.Collapsed;
            _selectedProduct = null;
        }

        private void SaveProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Pobierz wartości z modalu
                var txtSKU = FindName("txtModalSKU") as TextBox;
                var txtName = FindName("txtModalName") as TextBox;
                var txtDescription = FindName("txtModalDescription") as TextBox;
                var cmbCategoryModal = FindName("cmbModalCategory") as ComboBox;
                var cmbSupplierModal = FindName("cmbModalSupplier") as ComboBox;
                var txtPurchasePrice = FindName("txtModalPurchasePrice") as TextBox;
                var txtSellingPrice = FindName("txtModalSellingPrice") as TextBox;
                var txtMinStock = FindName("txtModalMinStock") as TextBox;
                var txtReorderLevel = FindName("txtModalReorderLevel") as TextBox;

                // Walidacja
                if (string.IsNullOrWhiteSpace(txtSKU?.Text))
                {
                    MessageBox.Show("SKU is required!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtName?.Text))
                {
                    MessageBox.Show("Product name is required!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Sprawdź czy SKU jest unikalne
                if (!_productService.IsSKUAvailable(txtSKU.Text, _selectedProduct?.ProductId))
                {
                    MessageBox.Show("This SKU already exists!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Pobierz CategoryId
                int categoryId = 1; // domyślna wartość
                var selectedCategory = cmbCategoryModal?.SelectedItem as ComboBoxItem;
                if (selectedCategory?.Tag != null)
                {
                    categoryId = (int)selectedCategory.Tag;
                }

                // Pobierz SupplierId
                int? supplierId = null;
                var selectedSupplier = cmbSupplierModal?.SelectedItem as ComboBoxItem;
                if (selectedSupplier?.Tag != null)
                {
                    supplierId = (int)selectedSupplier.Tag;
                }

                // Parsuj ceny
                if (!decimal.TryParse(txtPurchasePrice?.Text, out decimal purchasePrice))
                {
                    MessageBox.Show("Invalid purchase price!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtSellingPrice?.Text, out decimal sellingPrice))
                {
                    MessageBox.Show("Invalid selling price!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (sellingPrice < purchasePrice)
                {
                    MessageBox.Show("Selling price cannot be lower than purchase price!",
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int.TryParse(txtMinStock?.Text, out int minStock);
                int.TryParse(txtReorderLevel?.Text, out int reorderLevel);

                if (_selectedProduct == null)
                {
                    // DODAWANIE nowego produktu
                    var newProduct = new Product
                    {
                        SKU = txtSKU.Text,
                        ProductName = txtName.Text,
                        Description = txtDescription?.Text,
                        CategoryId = categoryId,
                        SupplierId = supplierId,
                        PurchasePrice = purchasePrice,
                        SellingPrice = sellingPrice,
                        VAT = 23.00m,
                        MinimumStock = minStock,
                        ReorderLevel = reorderLevel,
                        Unit = "pcs",
                        IsActive = true
                    };

                    int productId = _productService.AddProduct(newProduct, 0, null);

                    MessageBox.Show($"Product added successfully! (ID: {productId})",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // EDYCJA istniejącego produktu
                    var existingProduct = _productService.GetProductEntityById(_selectedProduct.ProductId);
                    if (existingProduct != null)
                    {
                        existingProduct.SKU = txtSKU.Text;
                        existingProduct.ProductName = txtName.Text;
                        existingProduct.Description = txtDescription?.Text;
                        existingProduct.CategoryId = categoryId;
                        existingProduct.SupplierId = supplierId;
                        existingProduct.PurchasePrice = purchasePrice;
                        existingProduct.SellingPrice = sellingPrice;
                        existingProduct.MinimumStock = minStock;
                        existingProduct.ReorderLevel = reorderLevel;

                        bool success = _productService.UpdateProduct(existingProduct);

                        if (success)
                        {
                            MessageBox.Show("Product updated successfully!",
                                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }

                modalOverlay.Visibility = Visibility.Collapsed;
                _selectedProduct = null;
                LoadProducts(); // Odśwież listę
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving product: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}