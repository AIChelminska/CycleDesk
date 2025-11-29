using CycleDesk.Models;
using CycleDesk.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CycleDesk
{
    public partial class CategoriesWindow : Window
    {
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;

        // Serwis i dane
        private CategoryService _categoryService;
        private List<CategoryDisplayDto> _categories;
        private CategoryDisplayDto _selectedCategory;
        private bool _isEditMode = false;

        // Słownik do przechowywania referencji do StackPanel subcategories
        private Dictionary<int, StackPanel> _subcategoryPanels = new Dictionary<int, StackPanel>();
        private Dictionary<int, TextBlock> _expandIcons = new Dictionary<int, TextBlock>();

        public CategoriesWindow(string username, string password, string fullName, string role)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _fullName = fullName;
            _role = role;

            // Inicjalizuj serwis
            _categoryService = new CategoryService();

            // Inicjalizuj menu
            sideMenu.Initialize(fullName, role);
            sideMenu.SetActiveMenu("Inventory", "Categories");

            // Podłącz eventy nawigacji
            ConnectMenuEvents();

            // Załaduj dane
            LoadCategories();
        }

        // ===== ŁADOWANIE DANYCH =====
        private void LoadCategories()
        {
            try
            {
                _categories = _categoryService.GetAllCategoriesWithDetails();
                DisplayCategories();
                UpdateCategoryCount();
                PopulateParentCategoryComboBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCategoryCount()
        {
            int totalCategories = 0;
            foreach (var cat in _categories)
            {
                totalCategories++; // główna kategoria
                totalCategories += cat.Subcategories.Count; // podkategorie
            }

            txtCategoryCount.Text = $"({totalCategories} categories)";
        }

        private void PopulateParentCategoryComboBox()
        {
            cmbParentCategory.Items.Clear();
            cmbParentCategory.Items.Add(new ComboBoxItem
            {
                Content = "None (Main Category)",
                Tag = null,
                IsSelected = true
            });

            foreach (var category in _categories)
            {
                cmbParentCategory.Items.Add(new ComboBoxItem
                {
                    Content = category.CategoryName,
                    Tag = category.CategoryId
                });
            }
        }

        // ===== WYŚWIETLANIE KATEGORII =====
        private void DisplayCategories()
        {
            categoriesContainer.Children.Clear();
            _subcategoryPanels.Clear();
            _expandIcons.Clear();

            foreach (var category in _categories)
            {
                // Główna kategoria
                var categoryRow = CreateCategoryRow(category, true);
                categoriesContainer.Children.Add(categoryRow);

                // Panel na podkategorie (początkowo ukryty)
                var subcategoriesPanel = new StackPanel
                {
                    Visibility = Visibility.Collapsed,
                    Margin = new Thickness(30, 5, 0, 10)
                };

                foreach (var subcategory in category.Subcategories)
                {
                    var subRow = CreateSubcategoryRow(subcategory);
                    subcategoriesPanel.Children.Add(subRow);
                }

                _subcategoryPanels[category.CategoryId] = subcategoriesPanel;
                categoriesContainer.Children.Add(subcategoriesPanel);

                // Divider
                var divider = new Rectangle
                {
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDEE2E6")),
                    Height = 1,
                    Margin = new Thickness(0, 10, 0, 10)
                };
                categoriesContainer.Children.Add(divider);
            }
        }

        private Border CreateCategoryRow(CategoryDisplayDto category, bool isMainCategory)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Colors.White),
                Padding = new Thickness(0, 15, 0, 15),
                Cursor = Cursors.Hand,
                Tag = category
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Kolumna 0: Nazwa z ikoną rozwinięcia (jeśli ma podkategorie)
            var namePanel = new StackPanel { Orientation = Orientation.Horizontal };

            if (category.Subcategories.Count > 0)
            {
                var expandIcon = new TextBlock
                {
                    Text = "▶",
                    FontSize = 12,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                    Margin = new Thickness(0, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                _expandIcons[category.CategoryId] = expandIcon;
                namePanel.Children.Add(expandIcon);

                // Kliknięcie na wiersz rozwija/zwija
                border.MouseLeftButtonDown += (s, e) => ToggleSubcategories(category.CategoryId);
            }
            else
            {
                // Spacer dla wyrównania
                namePanel.Children.Add(new Border { Width = 22 });
            }

            var nameText = new TextBlock
            {
                Text = category.CategoryName,
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            };
            namePanel.Children.Add(nameText);

            var nameBorder = new Border { Padding = new Thickness(0, 0, 15, 0) };
            Grid.SetColumn(nameBorder, 0);
            nameBorder.Child = namePanel;
            grid.Children.Add(nameBorder);

            // Kolumna 1: Liczba produktów
            var countText = new TextBlock
            {
                Text = category.ProductCount.ToString(),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            var countBorder = new Border { Padding = new Thickness(15, 0, 15, 0) };
            Grid.SetColumn(countBorder, 1);
            countBorder.Child = countText;
            grid.Children.Add(countBorder);

            // Kolumna 2: Status (zielona/żółta/czerwona kropka)
            var statusEllipse = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(
                    category.IsActive ? "#FF28A745" : "#FFDC3545")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var statusBorder = new Border { Padding = new Thickness(15, 0, 15, 0) };
            Grid.SetColumn(statusBorder, 2);
            statusBorder.Child = statusEllipse;
            grid.Children.Add(statusBorder);

            border.Child = grid;

            // Menu kontekstowe (Edit/Delete)
            border.ContextMenu = CreateCategoryContextMenu(category);

            return border;
        }

        private Border CreateSubcategoryRow(CategoryDisplayDto subcategory)
        {
            var border = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8F9FA")),
                Margin = new Thickness(0, 0, 0, 2),
                Cursor = Cursors.Hand,
                Tag = subcategory
            };

            var grid = new Grid { Height = 45 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Kolumna 0: Nazwa
            var nameText = new TextBlock
            {
                Text = subcategory.CategoryName,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF495057")),
                VerticalAlignment = VerticalAlignment.Center
            };
            var nameBorder = new Border { Padding = new Thickness(15, 0, 15, 0) };
            Grid.SetColumn(nameBorder, 0);
            nameBorder.Child = nameText;
            grid.Children.Add(nameBorder);

            // Kolumna 1: Liczba produktów
            var countText = new TextBlock
            {
                Text = subcategory.ProductCount.ToString(),
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF495057")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            var countBorder = new Border { Padding = new Thickness(15, 0, 15, 0) };
            Grid.SetColumn(countBorder, 1);
            countBorder.Child = countText;
            grid.Children.Add(countBorder);

            // Kolumna 2: Status
            var statusEllipse = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(
                    subcategory.IsActive ? "#FF28A745" : "#FFDC3545")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var statusBorder = new Border { Padding = new Thickness(15, 0, 15, 0) };
            Grid.SetColumn(statusBorder, 2);
            statusBorder.Child = statusEllipse;
            grid.Children.Add(statusBorder);

            border.Child = grid;

            // Menu kontekstowe
            border.ContextMenu = CreateCategoryContextMenu(subcategory);

            return border;
        }

        private ContextMenu CreateCategoryContextMenu(CategoryDisplayDto category)
        {
            var contextMenu = new ContextMenu();

            var editItem = new MenuItem { Header = "✏️ Edit" };
            editItem.Click += (s, e) => OpenEditCategoryModal(category);
            contextMenu.Items.Add(editItem);

            var deleteItem = new MenuItem { Header = "🗑️ Delete" };
            deleteItem.Click += (s, e) => DeleteCategory(category);
            contextMenu.Items.Add(deleteItem);

            return contextMenu;
        }

        private void ToggleSubcategories(int categoryId)
        {
            if (_subcategoryPanels.TryGetValue(categoryId, out var panel) &&
                _expandIcons.TryGetValue(categoryId, out var icon))
            {
                if (panel.Visibility == Visibility.Visible)
                {
                    panel.Visibility = Visibility.Collapsed;
                    icon.Text = "▶";
                }
                else
                {
                    panel.Visibility = Visibility.Visible;
                    icon.Text = "▼";
                }
            }
        }

        // ===== MODAL HANDLING =====
        private void OpenAddCategoryModal_Click(object sender, RoutedEventArgs e)
        {
            _isEditMode = false;
            _selectedCategory = null;
            ClearModalFields();
            txtModalTitle.Text = "Add New Category";
            modalOverlay.Visibility = Visibility.Visible;
            txtCategoryName.Focus();
        }

        private void OpenEditCategoryModal(CategoryDisplayDto category)
        {
            _isEditMode = true;
            _selectedCategory = category;
            txtModalTitle.Text = "Edit Category";

            txtCategoryName.Text = category.CategoryName;
            txtCategoryDescription.Text = category.Description;

            // Ustaw parent category
            if (category.ParentCategoryId.HasValue)
            {
                for (int i = 0; i < cmbParentCategory.Items.Count; i++)
                {
                    var item = cmbParentCategory.Items[i] as ComboBoxItem;
                    if (item?.Tag != null && (int)item.Tag == category.ParentCategoryId.Value)
                    {
                        cmbParentCategory.SelectedIndex = i;
                        break;
                    }
                }
            }
            else
            {
                cmbParentCategory.SelectedIndex = 0;
            }

            modalOverlay.Visibility = Visibility.Visible;
        }

        private void ClearModalFields()
        {
            txtCategoryName.Text = "";
            txtCategoryDescription.Text = "";
            cmbParentCategory.SelectedIndex = 0;
        }

        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            modalOverlay.Visibility = Visibility.Collapsed;
            _selectedCategory = null;
            _isEditMode = false;
        }

        private void CancelModal_Click(object sender, RoutedEventArgs e)
        {
            modalOverlay.Visibility = Visibility.Collapsed;
            _selectedCategory = null;
            _isEditMode = false;
        }

        private void SaveCategory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string categoryName = txtCategoryName.Text.Trim();

                // Walidacja
                if (string.IsNullOrEmpty(categoryName))
                {
                    MessageBox.Show("Please enter a category name.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtCategoryName.Focus();
                    return;
                }

                // Sprawdź unikalność nazwy
                if (!_categoryService.IsCategoryNameAvailable(categoryName, _selectedCategory?.CategoryId))
                {
                    MessageBox.Show("A category with this name already exists.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Pobierz parent category ID
                int? parentCategoryId = null;
                var selectedParent = cmbParentCategory.SelectedItem as ComboBoxItem;
                if (selectedParent?.Tag != null)
                {
                    parentCategoryId = (int)selectedParent.Tag;
                }

                if (_isEditMode && _selectedCategory != null)
                {
                    // EDYCJA
                    var categoryToUpdate = _categoryService.GetCategoryById(_selectedCategory.CategoryId);
                    if (categoryToUpdate != null)
                    {
                        categoryToUpdate.CategoryName = categoryName;
                        categoryToUpdate.Description = txtCategoryDescription.Text.Trim();
                        categoryToUpdate.ParentCategoryId = parentCategoryId;

                        bool success = _categoryService.UpdateCategory(categoryToUpdate);
                        if (success)
                        {
                            MessageBox.Show("Category updated successfully!", "Success",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                else
                {
                    // DODAWANIE
                    var newCategory = new Category
                    {
                        CategoryName = categoryName,
                        Description = txtCategoryDescription.Text.Trim(),
                        ParentCategoryId = parentCategoryId,
                        IsActive = true
                    };

                    int categoryId = _categoryService.AddCategory(newCategory);

                    string message = parentCategoryId.HasValue
                        ? $"Subcategory '{categoryName}' has been added successfully!"
                        : $"Category '{categoryName}' has been added successfully!";

                    MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                modalOverlay.Visibility = Visibility.Collapsed;
                _selectedCategory = null;
                _isEditMode = false;
                LoadCategories(); // Odśwież listę
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving category: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteCategory(CategoryDisplayDto category)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete category '{category.CategoryName}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool success = _categoryService.DeactivateCategory(category.CategoryId);
                    if (success)
                    {
                        MessageBox.Show("Category deleted successfully!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadCategories();
                    }
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "Cannot Delete",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting category: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ===== MENU EVENTS =====
        private void ConnectMenuEvents()
        {
            sideMenu.DashboardClicked += (s, e) => { new MainDashboardWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.ProductsClicked += (s, e) => { new ProductsWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.CategoriesClicked += (s, e) => { LoadCategories(); }; // Odśwież
            sideMenu.InventoryStatusClicked += (s, e) => { new InventoryStatusWindow(_username, _password, _fullName, _role).Show(); Close(); };
            sideMenu.GoodsReceiptClicked += (s, e) => { new GoodsReceiptWindow(_username, _password, _fullName, _role).Show(); Close(); };
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
}