using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CycleDesk
{
    public partial class InvoicesWindow : Window
    {
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;
        private Border _currentlyHoveredRow = null;

        public InvoicesWindow(string username, string password, string fullName, string role)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _fullName = fullName;
            _role = role;

            // Inicjalizuj SideMenuControl
            sideMenu.Initialize(fullName, role);
            sideMenu.SetActiveMenu("Sales", "Invoices");

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
            var result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Confirm Logout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                new MainWindow().Show();
                Close();
            }
        }

        // ===== ROW HOVER EFFECT =====
        private void InvoiceRow_MouseEnter(object sender, MouseEventArgs e)
        {
            Border row = sender as Border;
            if (row == null) return;

            _currentlyHoveredRow = row;
            row.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8F9FA"));
        }

        private void InvoiceRow_MouseLeave(object sender, MouseEventArgs e)
        {
            Border row = sender as Border;
            if (row == null) return;

            row.Background = new SolidColorBrush(Colors.White);
            _currentlyHoveredRow = null;
        }

        private void ActionButtonsPopup_Closed(object sender, EventArgs e)
        {
            if (_currentlyHoveredRow != null)
            {
                _currentlyHoveredRow.Background = new SolidColorBrush(Colors.White);
                _currentlyHoveredRow = null;
            }
        }

        // ===== INVOICE ACTIONS =====
        private void GenerateInvoice_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Generate Invoice functionality will allow you to create a new invoice from a sale.\n\nThis feature is coming soon!",
                "Generate Invoice",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void ViewInvoice_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "View Invoice Details\n\nThis will open a detailed view of the selected invoice showing:\n" +
                "• Invoice information\n" +
                "• Customer details\n" +
                "• Itemized list of products\n" +
                "• Payment information\n" +
                "• Total amount and tax breakdown",
                "View Invoice",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            actionButtonsPopup.IsOpen = false;
        }

        private void DownloadInvoice_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Do you want to download this invoice as a PDF file?",
                "Download Invoice",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show(
                    "Invoice downloaded successfully!\n\nThe PDF file has been saved to your Downloads folder.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }

            actionButtonsPopup.IsOpen = false;
        }

        private void DeleteInvoice_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete this invoice?\n\n" +
                "This action cannot be undone and will permanently remove the invoice from the system.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show(
                    "Invoice deleted successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }

            actionButtonsPopup.IsOpen = false;
        }
    }
}