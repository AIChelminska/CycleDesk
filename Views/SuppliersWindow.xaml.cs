using CycleDesk.Models;
using CycleDesk.Services;
using CycleDesk.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CycleDesk
{
    public partial class SuppliersWindow : Window
    {
        // ===== FIELDS =====
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;

        private SupplierService _supplierService;
        private List<Supplier> _allSuppliers;
        private ObservableCollection<SupplierDisplayDto> _displaySuppliers;

        private bool _isEditMode = false;
        private int _editingSupplierId = 0;

        public SuppliersWindow(string username, string password, string fullName, string role)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _fullName = fullName;
            _role = role;

            // Initialize service
            _supplierService = new SupplierService();
            _displaySuppliers = new ObservableCollection<SupplierDisplayDto>();

            // Initialize SideMenuControl
            sideMenu.Initialize(fullName, role);
            sideMenu.SetActiveMenu("Inventory", "Suppliers");

            // Connect menu events
            ConnectMenuEvents();

            // Load data after window is loaded
            this.Loaded += SuppliersWindow_Loaded;
        }

        private void SuppliersWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSuppliers();
            LoadFilters();
        }

        // ===== DATA LOADING =====
        private void LoadSuppliers()
        {
            try
            {
                _allSuppliers = _supplierService.GetAllSuppliers();

                _displaySuppliers.Clear();
                foreach (var supplier in _allSuppliers)
                {
                    _displaySuppliers.Add(SupplierDisplayDto.FromSupplier(supplier));
                }

                // Bind to ItemsControl
                icSuppliers.ItemsSource = _displaySuppliers;

                // Update total count
                if (txtTotalCount != null)
                {
                    txtTotalCount.Text = $"Showing {_displaySuppliers.Count} of {_allSuppliers.Count} suppliers";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading suppliers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFilters()
        {
            try
            {
                // Load countries to filter
                var countries = _supplierService.GetUniqueCountries();

                cmbCountry.Items.Clear();
                cmbCountry.Items.Add(new ComboBoxItem { Content = "All Countries", IsSelected = true });
                foreach (var country in countries)
                {
                    cmbCountry.Items.Add(new ComboBoxItem { Content = country });
                }

                // Status filter already has static items in XAML
                cmbStatus.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading filters: {ex.Message}");
            }
        }

        // ===== SEARCH AND FILTER =====
        private void ApplyFilters()
        {
            try
            {
                string searchText = txtSearch.Text ?? "";

                string statusFilter = "All Status";
                if (cmbStatus.SelectedItem is ComboBoxItem statusItem)
                {
                    statusFilter = statusItem.Content?.ToString() ?? "All Status";
                }

                string countryFilter = "All Countries";
                if (cmbCountry.SelectedItem is ComboBoxItem countryItem)
                {
                    countryFilter = countryItem.Content?.ToString() ?? "All Countries";
                }

                var filteredSuppliers = _supplierService.SearchSuppliers(searchText, statusFilter, countryFilter);

                _displaySuppliers.Clear();
                foreach (var supplier in filteredSuppliers)
                {
                    _displaySuppliers.Add(SupplierDisplayDto.FromSupplier(supplier));
                }

                // Update count
                if (txtTotalCount != null)
                {
                    txtTotalCount.Text = $"Showing {_displaySuppliers.Count} of {_allSuppliers?.Count ?? 0} suppliers";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filtering: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded)
            {
                ApplyFilters();
            }
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Placeholder is handled by VisualBrush in XAML
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Placeholder is handled by VisualBrush in XAML
        }

        private void cmbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) ApplyFilters();
        }

        private void cmbCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) ApplyFilters();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            cmbStatus.SelectedIndex = 0;
            cmbCountry.SelectedIndex = 0;

            LoadSuppliers();
            LoadFilters();

            MessageBox.Show("Data has been refreshed.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ===== EXPORT CSV =====
        private void ExportCSV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    FileName = $"Suppliers_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (dialog.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("ID,Supplier Name,Contact Person,Phone,Email,Address,City,Postal Code,Country,Tax Number,Status,Created Date");

                    foreach (var supplier in _displaySuppliers)
                    {
                        sb.AppendLine($"\"{supplier.SupplierCode}\",\"{supplier.SupplierName}\",\"{supplier.ContactPerson}\"," +
                                     $"\"{supplier.Phone}\",\"{supplier.Email}\",\"{supplier.Address}\"," +
                                     $"\"{supplier.City}\",\"{supplier.PostalCode}\",\"{supplier.Country}\"," +
                                     $"\"{supplier.TaxNumber}\"," +
                                     $"\"{(supplier.IsActive ? "Active" : "Inactive")}\",\"{supplier.CreatedDate:yyyy-MM-dd}\"");
                    }

                    File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show($"Exported {_displaySuppliers.Count} suppliers to CSV.", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===== ROW ACTIONS =====
        private void EditSupplier_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var supplier = button?.DataContext as SupplierDisplayDto;

            if (supplier != null)
            {
                OpenEditModal(supplier);
            }
        }

        private void DeleteSupplier_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var supplier = button?.DataContext as SupplierDisplayDto;

            if (supplier != null)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to deactivate this supplier?\n\n{supplier.SupplierCode} - {supplier.SupplierName}\n\nThe supplier will be marked as inactive.",
                    "Confirm Deactivation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    if (_supplierService.DeleteSupplier(supplier.SupplierId))
                    {
                        LoadSuppliers();
                        MessageBox.Show("Supplier has been deactivated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Error deactivating supplier.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // ===== MODAL MANAGEMENT =====
        private void OpenAddSupplierModal_Click(object sender, RoutedEventArgs e)
        {
            _isEditMode = false;
            _editingSupplierId = 0;

            // Reset all form fields
            modalTitle.Text = "Add New Supplier";
            btnSaveSupplier.Content = "Save Supplier";

            modalTxtSupplierName.Clear();
            modalTxtContactPerson.Clear();
            modalTxtPhone.Clear();
            modalTxtEmail.Clear();
            modalTxtAddress.Clear();
            modalTxtCity.Clear();
            modalTxtPostalCode.Clear();
            modalTxtTaxID.Clear();
            modalCmbCountry.SelectedIndex = 0;
            modalCmbStatus.SelectedIndex = 0;
            modalTxtNotes.Clear();

            // Show modal
            modalOverlay.Visibility = Visibility.Visible;
            modalTxtSupplierName.Focus();
        }

        private void OpenEditModal(SupplierDisplayDto supplier)
        {
            _isEditMode = true;
            _editingSupplierId = supplier.SupplierId;

            // Set modal title
            modalTitle.Text = "Edit Supplier";
            btnSaveSupplier.Content = "Update Supplier";

            // Fill form with supplier data
            modalTxtSupplierName.Text = supplier.SupplierName;
            modalTxtContactPerson.Text = supplier.ContactPerson;
            modalTxtPhone.Text = supplier.Phone;
            modalTxtEmail.Text = supplier.Email;
            modalTxtAddress.Text = supplier.Address;
            modalTxtCity.Text = supplier.City;
            modalTxtPostalCode.Text = supplier.PostalCode;
            modalTxtTaxID.Text = supplier.TaxNumber;
            modalTxtNotes.Text = supplier.Notes;

            // Set country
            for (int i = 0; i < modalCmbCountry.Items.Count; i++)
            {
                var item = modalCmbCountry.Items[i] as ComboBoxItem;
                if (item?.Content?.ToString() == supplier.Country)
                {
                    modalCmbCountry.SelectedIndex = i;
                    break;
                }
            }

            // Set status
            modalCmbStatus.SelectedIndex = supplier.IsActive ? 0 : 1;

            // Show modal
            modalOverlay.Visibility = Visibility.Visible;
            modalTxtSupplierName.Focus();
        }

        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            modalOverlay.Visibility = Visibility.Collapsed;
        }

        private void CancelModal_Click(object sender, RoutedEventArgs e)
        {
            modalOverlay.Visibility = Visibility.Collapsed;
        }

        private void SaveSupplier_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(modalTxtSupplierName.Text))
            {
                MessageBox.Show("Please enter supplier name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                modalTxtSupplierName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(modalTxtContactPerson.Text))
            {
                MessageBox.Show("Please enter contact person name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                modalTxtContactPerson.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(modalTxtPhone.Text))
            {
                MessageBox.Show("Please enter phone number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                modalTxtPhone.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(modalTxtEmail.Text))
            {
                MessageBox.Show("Please enter email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                modalTxtEmail.Focus();
                return;
            }

            if (!modalTxtEmail.Text.Contains("@") || !modalTxtEmail.Text.Contains("."))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                modalTxtEmail.Focus();
                return;
            }

            // Check for duplicate Tax Number
            if (!string.IsNullOrWhiteSpace(modalTxtTaxID.Text))
            {
                if (_supplierService.TaxNumberExists(modalTxtTaxID.Text, _isEditMode ? _editingSupplierId : null))
                {
                    MessageBox.Show("A supplier with this Tax Number already exists.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    modalTxtTaxID.Focus();
                    return;
                }
            }

            // Get selected values
            string country = (modalCmbCountry.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Poland";
            bool isActive = modalCmbStatus.SelectedIndex == 0; // 0 = Active, 1 = Inactive

            // Create supplier object
            var supplier = new Supplier
            {
                SupplierId = _editingSupplierId,
                SupplierName = modalTxtSupplierName.Text.Trim(),
                ContactPerson = modalTxtContactPerson.Text.Trim(),
                Phone = modalTxtPhone.Text.Trim(),
                Email = modalTxtEmail.Text.Trim(),
                Address = modalTxtAddress.Text.Trim(),
                City = modalTxtCity.Text.Trim(),
                PostalCode = modalTxtPostalCode.Text.Trim(),
                Country = country,
                TaxNumber = modalTxtTaxID.Text.Trim(),
                Notes = modalTxtNotes.Text.Trim(),
                IsActive = isActive
            };

            bool success;
            if (_isEditMode)
            {
                success = _supplierService.UpdateSupplier(supplier);
            }
            else
            {
                success = _supplierService.AddSupplier(supplier);
            }

            if (success)
            {
                modalOverlay.Visibility = Visibility.Collapsed;
                LoadSuppliers();
                LoadFilters();

                string message = _isEditMode
                    ? $"Supplier '{supplier.SupplierName}' has been updated successfully!"
                    : $"Supplier '{supplier.SupplierName}' has been added successfully!";

                MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Error saving supplier. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                // Already on this page
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
    }
}