using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CycleDesk
{
    public partial class SuppliersWindow : Window
    {
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;
        private Border _currentlySelectedRow = null;

        public SuppliersWindow(string username, string password, string fullName, string role)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _fullName = fullName;
            _role = role;

            // Inicjalizuj SideMenuControl
            sideMenu.Initialize(fullName, role);
            sideMenu.SetActiveMenu("Inventory", "Suppliers");

            // Podłącz eventy menu
            ConnectMenuEvents();
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
                // Już jesteśmy na tej stronie - nic nie rób
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

        // ===== SEARCH AND FILTER =====
        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearch.Text == "🔍 Search suppliers...")
            {
                txtSearch.Text = "";
                txtSearch.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF212529"));
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "🔍 Search suppliers...";
                txtSearch.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D"));
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Data has been refreshed.",
                          "Information",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);
        }

        private void ExportCSV_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export to CSV will be available soon.",
                          "Information",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);
        }

        // ===== CLICKABLE ROW LOGIC =====
        private void SupplierRow_Click(object sender, MouseButtonEventArgs e)
        {
            Border clickedRow = sender as Border;
            if (clickedRow == null) return;

            // Reset previous highlight
            if (_currentlySelectedRow != null && _currentlySelectedRow != clickedRow)
            {
                _currentlySelectedRow.Background = new SolidColorBrush(Colors.White);
            }

            // Highlight new row
            clickedRow.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8F9FA"));
            _currentlySelectedRow = clickedRow;

            // Show popup with buttons near cursor
            actionButtonsPopup.PlacementTarget = clickedRow;
            actionButtonsPopup.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
            actionButtonsPopup.HorizontalOffset = 10;
            actionButtonsPopup.VerticalOffset = 5;
            actionButtonsPopup.IsOpen = true;
        }

        private void SupplierRow_MouseLeave(object sender, MouseEventArgs e)
        {
            Border row = sender as Border;
            if (row == null) return;

            // If popup is open and mouse is NOT over popup - close
            if (actionButtonsPopup.IsOpen)
            {
                if (!IsMouseOverPopup())
                {
                    actionButtonsPopup.IsOpen = false;

                    // Reset highlight
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

        private void EditSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (_currentlySelectedRow == null) return;

            Grid rowGrid = _currentlySelectedRow.Child as Grid;
            string supplierID = "Unknown";
            string supplierName = "Unknown";

            if (rowGrid != null)
            {
                foreach (var child in rowGrid.Children)
                {
                    if (child is Border border)
                    {
                        int column = Grid.GetColumn(border);

                        // Find TextBlocks inside each Border
                        foreach (var borderChild in LogicalTreeHelper.GetChildren(border))
                        {
                            if (borderChild is TextBlock textBlock)
                            {
                                if (column == 0)
                                {
                                    supplierID = textBlock.Text;
                                }
                                else if (column == 1)
                                {
                                    supplierName = textBlock.Text;
                                }
                            }
                        }
                    }
                }
            }

            MessageBox.Show($"Edit supplier:\n\n{supplierID} - {supplierName}\n\nEdit functionality will be implemented soon.",
                           "Edit Supplier",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);
            actionButtonsPopup.IsOpen = false;
        }

        private void DeleteSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (_currentlySelectedRow == null) return;

            Grid rowGrid = _currentlySelectedRow.Child as Grid;
            string supplierID = "Unknown";
            string supplierName = "Unknown";

            if (rowGrid != null)
            {
                foreach (var child in rowGrid.Children)
                {
                    if (child is Border border)
                    {
                        int column = Grid.GetColumn(border);

                        // Find TextBlocks inside each Border
                        foreach (var borderChild in LogicalTreeHelper.GetChildren(border))
                        {
                            if (borderChild is TextBlock textBlock)
                            {
                                if (column == 0)
                                {
                                    supplierID = textBlock.Text;
                                }
                                else if (column == 1)
                                {
                                    supplierName = textBlock.Text;
                                }
                            }
                        }
                    }
                }
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete this supplier?\n\n{supplierID} - {supplierName}\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show($"Supplier {supplierID} deleted successfully!",
                               "Success",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);
                actionButtonsPopup.IsOpen = false;
            }
        }

        // ===== MODAL MANAGEMENT =====
        private void OpenAddSupplierModal_Click(object sender, RoutedEventArgs e)
        {
            // Reset all form fields
            modalTxtSupplierName.Clear();
            modalTxtContactPerson.Clear();
            modalTxtPhone.Clear();
            modalTxtEmail.Clear();
            modalTxtAddress.Clear();
            modalTxtCity.Clear();
            modalTxtPostalCode.Clear();
            modalTxtTaxID.Clear();
            modalCmbCountry.SelectedIndex = 0; // Poland
            modalCmbStatus.SelectedIndex = 0; // Active
            modalTxtNotes.Clear();

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
            // Validate required fields
            if (string.IsNullOrWhiteSpace(modalTxtSupplierName.Text))
            {
                MessageBox.Show("Please enter supplier name.",
                               "Validation Error",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                modalTxtSupplierName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(modalTxtContactPerson.Text))
            {
                MessageBox.Show("Please enter contact person name.",
                               "Validation Error",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                modalTxtContactPerson.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(modalTxtPhone.Text))
            {
                MessageBox.Show("Please enter phone number.",
                               "Validation Error",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                modalTxtPhone.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(modalTxtEmail.Text))
            {
                MessageBox.Show("Please enter email address.",
                               "Validation Error",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                modalTxtEmail.Focus();
                return;
            }

            // Basic email validation
            if (!modalTxtEmail.Text.Contains("@"))
            {
                MessageBox.Show("Please enter a valid email address.",
                               "Validation Error",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                modalTxtEmail.Focus();
                return;
            }

            // Get selected values
            ComboBoxItem selectedStatus = modalCmbStatus.SelectedItem as ComboBoxItem;
            string status = selectedStatus?.Content?.ToString() ?? "Active";

            ComboBoxItem selectedCountry = modalCmbCountry.SelectedItem as ComboBoxItem;
            string country = selectedCountry?.Content?.ToString() ?? "Poland";

            // TODO: Save to database
            MessageBox.Show($"Supplier '{modalTxtSupplierName.Text}' has been added successfully!\n\n" +
                           $"Contact Person: {modalTxtContactPerson.Text}\n" +
                           $"Phone: {modalTxtPhone.Text}\n" +
                           $"Email: {modalTxtEmail.Text}\n" +
                           $"Country: {country}\n" +
                           $"Status: {status}",
                           "Success",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);

            // Close modal
            modalOverlay.Visibility = Visibility.Collapsed;
        }
    }
}