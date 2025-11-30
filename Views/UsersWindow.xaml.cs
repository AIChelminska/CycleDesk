using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CycleDesk.Models;
using CycleDesk.Services;

namespace CycleDesk
{
    public partial class UsersWindow : Window
    {
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;
        private int _currentUserId;

        private UserService _userService;
        private List<User> _allUsers;
        private List<User> _filteredUsers;

        private User _selectedUser;
        private Border _currentlySelectedRow;
        private bool _isEditMode;

        public UsersWindow(string username, string password, string fullName, string role)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _fullName = fullName;
            _role = role;

            _userService = new UserService();

            // Sprawdź uprawnienia
            if (!_userService.CanManageUsers(role))
            {
                MessageBox.Show("You don't have permission to access User Management.",
                    "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }

            // Pobierz ID aktualnie zalogowanego użytkownika
            var currentUser = _userService.GetUserByUsername(username);
            _currentUserId = currentUser?.UserId ?? 0;

            // Inicjalizuj SideMenuControl
            sideMenu.Initialize(fullName, role);
            sideMenu.SetActiveMenu("Administration", "Users");

            ConnectMenuEvents();
            LoadUsers();
        }

        // ===== DATA LOADING =====
        private void LoadUsers()
        {
            try
            {
                _allUsers = _userService.GetAllUsers();
                ApplyFilters();
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatistics()
        {
            try
            {
                var stats = _userService.GetUserStatistics();
                txtTotalUsers.Text = stats["Total"].ToString();
                txtActiveUsers.Text = stats["Active"].ToString();
                txtInactiveUsers.Text = stats["Inactive"].ToString();
                txtManagerCount.Text = stats["Manager"].ToString();
                txtUserCount.Text = $"({stats["Total"]} users)";
            }
            catch { }
        }

        // ===== FILTERING =====
        private void ApplyFilters()
        {
            string searchText = txtSearch?.Text?.ToLower() ?? "";

            _filteredUsers = _allUsers
                .Where(u =>
                    string.IsNullOrEmpty(searchText) ||
                    u.Username.ToLower().Contains(searchText) ||
                    u.FirstName.ToLower().Contains(searchText) ||
                    u.LastName.ToLower().Contains(searchText) ||
                    u.Role.ToLower().Contains(searchText) ||
                    (u.Email != null && u.Email.ToLower().Contains(searchText)))
                .OrderByDescending(u => u.CreatedDate)
                .ToList();

            DisplayUsers();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        // ===== DISPLAY =====
        private void DisplayUsers()
        {
            usersStackPanel.Children.Clear();

            foreach (var user in _filteredUsers)
            {
                usersStackPanel.Children.Add(CreateUserRow(user));
            }
        }

        private Border CreateUserRow(User user)
        {
            var border = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDEE2E6")),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Background = Brushes.White,
                Cursor = Cursors.Hand,
                Tag = user
            };

            border.MouseLeftButtonDown += UserRow_Click;
            border.MouseLeave += UserRow_MouseLeave;

            var grid = new Grid { Height = 60 };

            // Define columns
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });

            // Column 0: Avatar
            grid.Children.Add(CreateCell(0, CreateAvatar(user)));

            // Column 1: Full Name
            grid.Children.Add(CreateCell(1, CreateTextBlock(user.FullName, true)));

            // Column 2: Username
            grid.Children.Add(CreateCell(2, CreateTextBlock(user.Username)));

            // Column 3: Role Badge
            grid.Children.Add(CreateCell(3, CreateRoleBadge(user.Role)));

            // Column 4: Email
            grid.Children.Add(CreateCell(4, CreateTextBlock(user.Email ?? "-")));

            // Column 5: Status Badge
            grid.Children.Add(CreateCell(5, CreateStatusBadge(user.IsActive)));

            // Column 6: Last Login
            grid.Children.Add(CreateCell(6, CreateTextBlock(user.LastLoginDate?.ToString("dd.MM.yyyy HH:mm") ?? "-", false, true)));

            // Column 7: Created (no right border)
            grid.Children.Add(CreateCell(7, CreateTextBlock(user.CreatedDate.ToString("dd.MM.yyyy"), false, true), false));

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

        private UIElement CreateAvatar(User user)
        {
            string initials = GetInitials(user.FirstName, user.LastName);
            var avatarBorder = new Border
            {
                Width = 40,
                Height = 40,
                CornerRadius = new CornerRadius(20),
                Background = GetRoleColor(user.Role),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            avatarBorder.Child = new TextBlock
            {
                Text = initials,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            return avatarBorder;
        }

        private string GetInitials(string firstName, string lastName)
        {
            string first = !string.IsNullOrEmpty(firstName) ? firstName[0].ToString().ToUpper() : "";
            string last = !string.IsNullOrEmpty(lastName) ? lastName[0].ToString().ToUpper() : "";
            return first + last;
        }

        private SolidColorBrush GetRoleColor(string role)
        {
            return role switch
            {
                "Manager" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDC3545")),
                "Supervisor" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF17A2B8")),
                "Operator" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF28A745")),
                "Cashier" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFC107")),
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D"))
            };
        }

        private TextBlock CreateTextBlock(string text, bool isBold = false, bool isCenter = false)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = 13,
                FontWeight = isBold ? FontWeights.SemiBold : FontWeights.Normal,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isBold ? "#FF2D3E50" : "#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = isCenter ? TextAlignment.Center : TextAlignment.Left,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
        }

        private UIElement CreateRoleBadge(string role)
        {
            string bgColor = role switch
            {
                "Manager" => "#FFDC3545",
                "Supervisor" => "#FF17A2B8",
                "Operator" => "#FF28A745",
                "Cashier" => "#FFFFC107",
                _ => "#FF6C757D"
            };

            var badge = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgColor)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 4, 10, 4),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            badge.Child = new TextBlock
            {
                Text = role,
                FontSize = 11,
                Foreground = role == "Cashier" ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")) : Brushes.White,
                FontWeight = FontWeights.SemiBold
            };

            return badge;
        }

        private UIElement CreateStatusBadge(bool isActive)
        {
            var badge = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isActive ? "#FFD4EDDA" : "#FFF8D7DA")),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 4, 10, 4),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            badge.Child = new TextBlock
            {
                Text = isActive ? "Active" : "Inactive",
                FontSize = 11,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isActive ? "#FF28A745" : "#FFDC3545")),
                FontWeight = FontWeights.SemiBold
            };

            return badge;
        }

        // ===== CLICKABLE ROW LOGIC =====
        private void UserRow_Click(object sender, MouseButtonEventArgs e)
        {
            Border clickedRow = sender as Border;
            if (clickedRow == null) return;

            _selectedUser = clickedRow.Tag as User;

            // Reset previous highlight
            if (_currentlySelectedRow != null && _currentlySelectedRow != clickedRow)
            {
                _currentlySelectedRow.Background = Brushes.White;
            }

            // Highlight new row
            clickedRow.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8F9FA"));
            _currentlySelectedRow = clickedRow;

            // Build action buttons dynamically
            BuildActionButtons();

            // Show popup
            actionButtonsPopup.PlacementTarget = clickedRow;
            actionButtonsPopup.Placement = PlacementMode.MousePoint;
            actionButtonsPopup.HorizontalOffset = 10;
            actionButtonsPopup.VerticalOffset = 5;
            actionButtonsPopup.IsOpen = true;
        }

        private void BuildActionButtons()
        {
            actionButtonsPanel.Children.Clear();

            if (_selectedUser == null) return;

            // Edit button
            var btnEdit = new Button
            {
                Content = "Edit",
                Width = 60,
                Margin = new Thickness(0, 0, 8, 0),
                Style = (Style)FindResource("EditButtonStyle")
            };
            btnEdit.Click += EditUser_Click;
            actionButtonsPanel.Children.Add(btnEdit);

            // Reset Password button
            var btnResetPassword = new Button
            {
                Content = "Password",
                Width = 80,
                Margin = new Thickness(0, 0, 8, 0),
                Style = (Style)FindResource("ResetPasswordButtonStyle")
            };
            btnResetPassword.Click += ResetPassword_Click;
            actionButtonsPanel.Children.Add(btnResetPassword);

            // Activate/Deactivate button (not for self)
            if (_selectedUser.UserId != _currentUserId)
            {
                if (_selectedUser.IsActive)
                {
                    var btnDeactivate = new Button
                    {
                        Content = "Deactivate",
                        Width = 85,
                        Margin = new Thickness(0, 0, 8, 0),
                        Style = (Style)FindResource("DeactivateButtonStyle")
                    };
                    btnDeactivate.Click += DeactivateUser_Click;
                    actionButtonsPanel.Children.Add(btnDeactivate);
                }
                else
                {
                    var btnActivate = new Button
                    {
                        Content = "Activate",
                        Width = 75,
                        Margin = new Thickness(0, 0, 8, 0),
                        Style = (Style)FindResource("ActivateButtonStyle")
                    };
                    btnActivate.Click += ActivateUser_Click;
                    actionButtonsPanel.Children.Add(btnActivate);
                }

                // Delete button
                var btnDelete = new Button
                {
                    Content = "Delete",
                    Width = 70,
                    Style = (Style)FindResource("DeleteButtonStyle")
                };
                btnDelete.Click += DeleteUser_Click;
                actionButtonsPanel.Children.Add(btnDelete);
            }
        }

        private void UserRow_MouseLeave(object sender, MouseEventArgs e)
        {
            if (actionButtonsPopup.IsOpen && !IsMouseOverPopup())
            {
                actionButtonsPopup.IsOpen = false;
                if (_currentlySelectedRow != null)
                {
                    _currentlySelectedRow.Background = Brushes.White;
                    _currentlySelectedRow = null;
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
                _currentlySelectedRow.Background = Brushes.White;
                _currentlySelectedRow = null;
            }
        }

        // ===== ADD/EDIT USER =====
        private void AddNewUser_Click(object sender, RoutedEventArgs e)
        {
            _isEditMode = false;
            _selectedUser = null;
            ClearModalFields();
            txtModalTitle.Text = "Add New User";
            btnSaveUser.Content = "Create User";
            modalOverlay.Visibility = Visibility.Visible;
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null) return;

            actionButtonsPopup.IsOpen = false;
            _isEditMode = true;

            txtModalTitle.Text = "Edit User";
            btnSaveUser.Content = "Save Changes";

            // Fill form with user data
            txtModalFirstName.Text = _selectedUser.FirstName;
            txtModalLastName.Text = _selectedUser.LastName;
            txtModalUsername.Text = _selectedUser.Username;
            txtModalEmail.Text = _selectedUser.Email ?? "";
            txtModalPhone.Text = _selectedUser.Phone ?? "";

            // Select role
            foreach (ComboBoxItem item in cmbModalRole.Items)
            {
                if (item.Content.ToString() == _selectedUser.Role)
                {
                    cmbModalRole.SelectedItem = item;
                    break;
                }
            }

            modalOverlay.Visibility = Visibility.Visible;
        }

        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            modalOverlay.Visibility = Visibility.Collapsed;
            ClearModalFields();
        }

        private void ClearModalFields()
        {
            txtModalFirstName.Text = "";
            txtModalLastName.Text = "";
            txtModalUsername.Text = "";
            txtModalEmail.Text = "";
            txtModalPhone.Text = "";
            cmbModalRole.SelectedIndex = 0;
        }

        private void SaveUser_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtModalFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtModalLastName.Text) ||
                string.IsNullOrWhiteSpace(txtModalUsername.Text))
            {
                MessageBox.Show("Please fill in all required fields (First Name, Last Name, Username).",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_userService.IsUsernameAvailable(txtModalUsername.Text, _isEditMode ? _selectedUser?.UserId : null))
            {
                MessageBox.Show("This username is already taken.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string selectedRole = (cmbModalRole.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Operator";

                if (_isEditMode && _selectedUser != null)
                {
                    _selectedUser.FirstName = txtModalFirstName.Text.Trim();
                    _selectedUser.LastName = txtModalLastName.Text.Trim();
                    _selectedUser.Username = txtModalUsername.Text.Trim();
                    _selectedUser.Email = string.IsNullOrWhiteSpace(txtModalEmail.Text) ? null : txtModalEmail.Text.Trim();
                    _selectedUser.Phone = string.IsNullOrWhiteSpace(txtModalPhone.Text) ? null : txtModalPhone.Text.Trim();
                    _selectedUser.Role = selectedRole;

                    _userService.UpdateUser(_selectedUser);
                    MessageBox.Show("User updated successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Generuj Access Code zamiast tworzyć użytkownika z hasłem
                    string accessCode = _userService.CreateUserWithAccessCode(
                        txtModalFirstName.Text.Trim(),
                        txtModalLastName.Text.Trim(),
                        txtModalUsername.Text.Trim(),
                        string.IsNullOrWhiteSpace(txtModalEmail.Text) ? null : txtModalEmail.Text.Trim(),
                        string.IsNullOrWhiteSpace(txtModalPhone.Text) ? null : txtModalPhone.Text.Trim(),
                        selectedRole,
                        _currentUserId
                    );

                    // Pokaż kod dostępu managerowi
                    ShowAccessCodeDialog(accessCode, txtModalFirstName.Text.Trim(), txtModalLastName.Text.Trim(), txtModalUsername.Text.Trim());
                }

                modalOverlay.Visibility = Visibility.Collapsed;
                ClearModalFields();
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving user: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowAccessCodeDialog(string accessCode, string firstName, string lastName, string username)
        {
            string message = $"Access Code Generated Successfully!\n\n" +
                           $"Employee: {firstName} {lastName}\n" +
                           $"Username: {username}\n\n" +
                           $"═══════════════════════════\n" +
                           $"ACCESS CODE: {accessCode}\n" +
                           $"═══════════════════════════\n\n" +
                           $"Please give this code to the employee.\n" +
                           $"They will use it to set their password and activate their account.\n\n" +
                           $"Click OK to copy the code to clipboard.";

            MessageBox.Show(message, "Access Code Created", MessageBoxButton.OK, MessageBoxImage.Information);

            // Kopiuj kod do schowka
            Clipboard.SetText(accessCode);
        }

        // ===== RESET PASSWORD =====
        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null) return;

            actionButtonsPopup.IsOpen = false;

            var result = MessageBox.Show(
                $"Are you sure you want to reset password for user:\n\n" +
                $"Name: {_selectedUser.FullName}\n" +
                $"Username: {_selectedUser.Username}\n" +
                $"Role: {_selectedUser.Role}\n\n" +
                $"A password reset code will be generated that the user can use to set a new password.",
                "Confirm Password Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    string resetCode = _userService.CreatePasswordResetCode(_selectedUser.UserId, _currentUserId);

                    if (string.IsNullOrEmpty(resetCode))
                    {
                        MessageBox.Show("Error generating reset code.", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string message = $"PASSWORD RESET CODE CREATED\n\n" +
                                   $"For user: {_selectedUser.FullName}\n" +
                                   $"Username: {_selectedUser.Username}\n\n" +
                                   $"═══════════════════════════\n" +
                                   $"RESET CODE: {resetCode}\n" +
                                   $"═══════════════════════════\n\n" +
                                   $"Please give this code to the employee.\n" +
                                   $"They will use it on the login screen to set a new password.\n\n" +
                                   $"Click OK to copy the code to clipboard.";

                    MessageBox.Show(message, "Reset Code Created", MessageBoxButton.OK, MessageBoxImage.Information);
                    Clipboard.SetText(resetCode);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generating reset code: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseResetPasswordModal_Click(object sender, RoutedEventArgs e)
        {
            resetPasswordOverlay.Visibility = Visibility.Collapsed;
        }

        private void ConfirmResetPassword_Click(object sender, RoutedEventArgs e)
        {
            resetPasswordOverlay.Visibility = Visibility.Collapsed;
        }

        // ===== ACTIVATE/DEACTIVATE/DELETE =====
        private void DeactivateUser_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null || _selectedUser.UserId == _currentUserId) return;

            actionButtonsPopup.IsOpen = false;

            var result = MessageBox.Show($"Deactivate user {_selectedUser.FullName}?\n\nThis user will no longer be able to log in.",
                "Confirm Deactivation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _userService.DeactivateUser(_selectedUser.UserId);
                    LoadUsers();
                    MessageBox.Show($"User {_selectedUser.FullName} has been deactivated.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ActivateUser_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null) return;

            actionButtonsPopup.IsOpen = false;

            var result = MessageBox.Show($"Activate user {_selectedUser.FullName}?\n\nThis user will be able to log in again.",
                "Confirm Activation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _userService.ActivateUser(_selectedUser.UserId);
                    LoadUsers();
                    MessageBox.Show($"User {_selectedUser.FullName} has been activated.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null || _selectedUser.UserId == _currentUserId) return;

            actionButtonsPopup.IsOpen = false;

            if (!_userService.CanDeleteUser(_selectedUser.UserId, out string reason))
            {
                MessageBox.Show(reason, "Cannot Delete", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Delete user {_selectedUser.FullName}?\n\nUsername: {_selectedUser.Username}\nRole: {_selectedUser.Role}\n\nThis action cannot be undone!",
                "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _userService.DeleteUser(_selectedUser.UserId);
                    LoadUsers();
                    MessageBox.Show($"User {_selectedUser.FullName} has been deleted.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ===== MENU EVENTS =====
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

            sideMenu.UsersClicked += (s, e) => LoadUsers();

            sideMenu.SettingsClicked += (s, e) =>
            {
                new SettingsWindow(_username, _password, _fullName, _role).Show();
                Close();
            };

            sideMenu.LogoutClicked += (s, e) =>
            {
                var result = MessageBox.Show("Are you sure you want to logout?",
                    "Confirm Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    new MainWindow().Show();
                    Close();
                }
            };
        }
    }
}