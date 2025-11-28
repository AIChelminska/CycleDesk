using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CycleDesk
{
    public partial class UsersWindow : Window
    {
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;

        private ObservableCollection<User> _allUsers;
        private ObservableCollection<User> _filteredUsers;

        public UsersWindow(string username, string password, string fullName, string role)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _fullName = fullName;
            _role = role;

            // Inicjalizuj SideMenuControl
            sideMenu.Initialize(fullName, role);
            sideMenu.SetActiveMenu("Administration", "Users");

            // Podłącz eventy menu
            ConnectMenuEvents();

            // Initialize data
            LoadMockUsers();
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
                new ProductsToOrderWindow(_username, _password, _fullName, _role).Show();
                Close();
            };

            sideMenu.UsersClicked += (s, e) =>
            {
                // Już jesteśmy na tej stronie - nic nie rób
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
        public class User
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FullName => $"{FirstName} {LastName}";
            public string Initials => $"{FirstName[0]}{LastName[0]}";
            public string Username { get; set; }
            public string Role { get; set; } // Administrator, Manager, Cashier
            public string AccessCode { get; set; }
            public string Status { get; set; } // Active, Pending, Inactive
            public DateTime CreatedDate { get; set; }
            public string CreatedDateFormatted => CreatedDate.ToString("dd.MM.yyyy");
            public DateTime? LastLoginDate { get; set; }
            public string LastLoginFormatted => LastLoginDate?.ToString("dd.MM.yyyy HH:mm") ?? "-";

            public SolidColorBrush RoleColor
            {
                get
                {
                    return Role switch
                    {
                        "Administrator" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDC3545")),
                        "Manager" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF17A2B8")),
                        "Cashier" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF28A745")),
                        _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D"))
                    };
                }
            }

            public SolidColorBrush StatusColor
            {
                get
                {
                    return Status switch
                    {
                        "Active" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF28A745")),
                        "Pending" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFC107")),
                        "Inactive" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDC3545")),
                        _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D"))
                    };
                }
            }

            public string DisplayAccessCode => Status == "Pending" ? AccessCode : "•••••••••••";
        }

        // ===== MOCK DATA =====
        private void LoadMockUsers()
        {
            _allUsers = new ObservableCollection<User>
            {
                new User
                {
                    Id = 1,
                    FirstName = "Admin",
                    LastName = "User",
                    Username = "admin1",
                    Role = "Administrator",
                    AccessCode = "12345678901",
                    Status = "Active",
                    CreatedDate = new DateTime(2025, 1, 1),
                    LastLoginDate = DateTime.Now.AddHours(-2)
                },
                new User
                {
                    Id = 2,
                    FirstName = "Anna",
                    LastName = "Kowalska",
                    Username = "akowalska",
                    Role = "Manager",
                    AccessCode = "23456789012",
                    Status = "Active",
                    CreatedDate = new DateTime(2025, 10, 5),
                    LastLoginDate = DateTime.Now.AddDays(-1)
                },
                new User
                {
                    Id = 3,
                    FirstName = "Jan",
                    LastName = "Nowak",
                    Username = "jnowak",
                    Role = "Cashier",
                    AccessCode = "34567890123",
                    Status = "Active",
                    CreatedDate = new DateTime(2025, 10, 10),
                    LastLoginDate = DateTime.Now.AddHours(-5)
                },
                new User
                {
                    Id = 4,
                    FirstName = "Sarah",
                    LastName = "Johnson",
                    Username = "sjohnson",
                    Role = "Cashier",
                    AccessCode = "45678901234",
                    Status = "Active",
                    CreatedDate = new DateTime(2025, 10, 15),
                    LastLoginDate = DateTime.Now.AddDays(-2)
                },
                new User
                {
                    Id = 5,
                    FirstName = "Tom",
                    LastName = "Brown",
                    Username = "tbrown",
                    Role = "Manager",
                    AccessCode = "56789012345",
                    Status = "Active",
                    CreatedDate = new DateTime(2025, 10, 20),
                    LastLoginDate = DateTime.Now.AddDays(-1)
                },
                new User
                {
                    Id = 6,
                    FirstName = "Piotr",
                    LastName = "Kowal",
                    Username = "pkowal",
                    Role = "Cashier",
                    AccessCode = "67890123456",
                    Status = "Pending",
                    CreatedDate = new DateTime(2025, 11, 17),
                    LastLoginDate = null
                },
                new User
                {
                    Id = 7,
                    FirstName = "Maria",
                    LastName = "Wiśniewska",
                    Username = "mwisniewski",
                    Role = "Manager",
                    AccessCode = "78901234567",
                    Status = "Pending",
                    CreatedDate = new DateTime(2025, 11, 18),
                    LastLoginDate = null
                },
                new User
                {
                    Id = 8,
                    FirstName = "John",
                    LastName = "Smith",
                    Username = "jsmith",
                    Role = "Manager",
                    AccessCode = "89012345678",
                    Status = "Inactive",
                    CreatedDate = new DateTime(2025, 8, 1),
                    LastLoginDate = new DateTime(2025, 11, 10, 12, 0, 0)
                }
            };

            _filteredUsers = new ObservableCollection<User>(_allUsers);
        }

        // ===== FILTERING =====
        private void ApplyFilters()
        {
            string searchText = txtSearch.Text.ToLower();

            var filtered = _allUsers.AsEnumerable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filtered = filtered.Where(u =>
                    u.FullName.ToLower().Contains(searchText) ||
                    u.Username.ToLower().Contains(searchText) ||
                    u.Role.ToLower().Contains(searchText));
            }

            _filteredUsers = new ObservableCollection<User>(filtered);

            // Update UI
            UpdateKPIs();
            GenerateUserTable();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded)
            {
                ApplyFilters();
            }
        }

        private void UpdateKPIs()
        {
            // Total Users
            txtTotalUsers.Text = _allUsers.Count.ToString();

            // Active Users
            int activeCount = _allUsers.Count(u => u.Status == "Active");
            txtActiveUsers.Text = activeCount.ToString();

            // Pending Users
            int pendingCount = _allUsers.Count(u => u.Status == "Pending");
            txtPendingUsers.Text = pendingCount.ToString();

            // Inactive Users
            int inactiveCount = _allUsers.Count(u => u.Status == "Inactive");
            txtInactiveUsers.Text = inactiveCount.ToString();
        }

        private void GenerateUserTable()
        {
            // Clear existing rows (keep header)
            while (usersPanel.Children.Count > 1)
            {
                usersPanel.Children.RemoveAt(1);
            }

            // Add user rows
            foreach (var user in _filteredUsers)
            {
                AddUserRow(user);
            }
        }

        private void AddUserRow(User user)
        {
            var border = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDEE2E6")),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Background = Brushes.White,
                Padding = new Thickness(0, 15, 0, 15)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });

            // Avatar
            var avatarBorder = new Border
            {
                Width = 40,
                Height = 40,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8F9FA")),
                CornerRadius = new CornerRadius(20),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            var txtAvatar = new TextBlock
            {
                Text = user.Initials,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            avatarBorder.Child = txtAvatar;
            Grid.SetColumn(avatarBorder, 0);
            grid.Children.Add(avatarBorder);

            // Full Name
            var txtName = new TextBlock
            {
                Text = user.FullName,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D3E50")),
                FontWeight = FontWeights.Medium,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(txtName, 1);
            grid.Children.Add(txtName);

            // Username
            var txtUsername = new TextBlock
            {
                Text = user.Username,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(txtUsername, 2);
            grid.Children.Add(txtUsername);

            // Role Badge
            var roleBorder = new Border
            {
                Background = user.RoleColor,
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 4, 10, 4),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var txtRole = new TextBlock
            {
                Text = user.Role,
                FontSize = 11,
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold
            };
            roleBorder.Child = txtRole;
            Grid.SetColumn(roleBorder, 3);
            grid.Children.Add(roleBorder);

            // Access Code
            var txtCode = new TextBlock
            {
                Text = user.DisplayAccessCode,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                FontFamily = new FontFamily("Consolas")
            };
            Grid.SetColumn(txtCode, 4);
            grid.Children.Add(txtCode);

            // Status Badge
            var statusBorder = new Border
            {
                Background = user.Status == "Active"
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD4EDDA"))
                    : user.Status == "Pending"
                        ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFEF3CD"))
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8D7DA")),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 4, 10, 4),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var txtStatus = new TextBlock
            {
                Text = user.Status,
                FontSize = 11,
                Foreground = user.StatusColor,
                FontWeight = FontWeights.SemiBold
            };
            statusBorder.Child = txtStatus;
            Grid.SetColumn(statusBorder, 5);
            grid.Children.Add(statusBorder);

            // Last Login
            var txtLastLogin = new TextBlock
            {
                Text = user.LastLoginFormatted,
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtLastLogin, 6);
            grid.Children.Add(txtLastLogin);

            // Created Date
            var txtCreated = new TextBlock
            {
                Text = user.CreatedDateFormatted,
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtCreated, 7);
            grid.Children.Add(txtCreated);

            // Actions (buttons depending on status)
            var actionsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (user.Status == "Active")
            {
                // Edit button
                var btnEdit = CreateActionButton("✏️", "#FF17A2B8");
                btnEdit.Click += (s, e) => EditUser_Click(user);
                actionsPanel.Children.Add(btnEdit);

                // Deactivate button
                var btnDeactivate = CreateActionButton("🔒", "#FFFFC107");
                btnDeactivate.Click += (s, e) => DeactivateUser_Click(user);
                actionsPanel.Children.Add(btnDeactivate);

                // Delete button
                var btnDelete = CreateActionButton("🗑️", "#FFDC3545");
                btnDelete.Click += (s, e) => DeleteUser_Click(user);
                actionsPanel.Children.Add(btnDelete);
            }
            else if (user.Status == "Pending")
            {
                // Edit button
                var btnEdit = CreateActionButton("✏️", "#FF17A2B8");
                btnEdit.Click += (s, e) => EditUser_Click(user);
                actionsPanel.Children.Add(btnEdit);

                // Reset Code button
                var btnReset = CreateActionButton("🔄", "#FFFFC107");
                btnReset.Click += (s, e) => ResetCode_Click(user);
                actionsPanel.Children.Add(btnReset);

                // Copy Code button
                var btnCopy = CreateActionButton("📋", "#FF28A745");
                btnCopy.Click += (s, e) => CopyCode_Click(user);
                actionsPanel.Children.Add(btnCopy);

                // Delete button
                var btnDelete = CreateActionButton("🗑️", "#FFDC3545");
                btnDelete.Click += (s, e) => DeleteUser_Click(user);
                actionsPanel.Children.Add(btnDelete);
            }
            else if (user.Status == "Inactive")
            {
                // Edit button
                var btnEdit = CreateActionButton("✏️", "#FF17A2B8");
                btnEdit.Click += (s, e) => EditUser_Click(user);
                actionsPanel.Children.Add(btnEdit);

                // Activate button
                var btnActivate = CreateActionButton("🔓", "#FF28A745");
                btnActivate.Click += (s, e) => ActivateUser_Click(user);
                actionsPanel.Children.Add(btnActivate);

                // Delete button
                var btnDelete = CreateActionButton("🗑️", "#FFDC3545");
                btnDelete.Click += (s, e) => DeleteUser_Click(user);
                actionsPanel.Children.Add(btnDelete);
            }

            Grid.SetColumn(actionsPanel, 8);
            grid.Children.Add(actionsPanel);

            border.Child = grid;
            usersPanel.Children.Add(border);
        }

        private Button CreateActionButton(string content, string colorHex)
        {
            var button = new Button
            {
                Content = content,
                Width = 32,
                Height = 32,
                Margin = new Thickness(2),
                Background = Brushes.Transparent,
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex)),
                BorderThickness = new Thickness(1),
                Cursor = System.Windows.Input.Cursors.Hand,
                FontSize = 13
            };

            var style = new Style(typeof(Button));
            var template = new ControlTemplate(typeof(Button));

            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            borderFactory.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Button.BorderBrushProperty));
            borderFactory.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Button.BorderThicknessProperty));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));

            var contentFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            borderFactory.AppendChild(contentFactory);

            template.VisualTree = borderFactory;

            var trigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            trigger.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8F9FA"))));
            template.Triggers.Add(trigger);

            style.Setters.Add(new Setter(Button.TemplateProperty, template));
            button.Style = style;

            return button;
        }

        // ===== ACTION HANDLERS =====
        private void AddNewUser_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Add New User modal - to be implemented\n\n" +
                          "Will open a form with:\n" +
                          "- First Name\n" +
                          "- Last Name\n" +
                          "- Username\n" +
                          "- Role (ComboBox)\n" +
                          "- Auto-generated Access Code\n\n" +
                          "User will be created with status 'Pending'",
                          "Add New User", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void EditUser_Click(User user)
        {
            MessageBox.Show($"Edit User: {user.FullName}\n\n" +
                          $"Current Status: {user.Status}\n" +
                          $"Username: {user.Username}\n" +
                          $"Role: {user.Role}\n\n" +
                          "Edit modal - to be implemented",
                          "Edit User", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeactivateUser_Click(User user)
        {
            var result = MessageBox.Show($"Deactivate user {user.FullName}?\n\n" +
                                       $"This user will no longer be able to log in.",
                                       "Confirm Deactivation",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                user.Status = "Inactive";
                ApplyFilters();
                MessageBox.Show($"User {user.FullName} has been deactivated.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ActivateUser_Click(User user)
        {
            var result = MessageBox.Show($"Activate user {user.FullName}?\n\n" +
                                       $"This user will be able to log in again.",
                                       "Confirm Activation",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                user.Status = "Active";
                ApplyFilters();
                MessageBox.Show($"User {user.FullName} has been activated.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ResetCode_Click(User user)
        {
            var result = MessageBox.Show($"Generate new Access Code for {user.FullName}?\n\n" +
                                       $"Current code: {user.AccessCode}\n" +
                                       $"The old code will stop working.",
                                       "Reset Access Code",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Generate new code
                Random random = new Random();
                string newCode = string.Empty;
                for (int i = 0; i < 11; i++)
                {
                    newCode += random.Next(0, 10).ToString();
                }

                user.AccessCode = newCode;
                ApplyFilters();

                MessageBox.Show($"New Access Code generated for {user.FullName}:\n\n" +
                              $"{newCode}\n\n" +
                              $"Please share this code with the employee.",
                              "New Code Generated",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
        }

        private void CopyCode_Click(User user)
        {
            try
            {
                Clipboard.SetText(user.AccessCode);
                MessageBox.Show($"Access Code copied to clipboard:\n\n{user.AccessCode}",
                              "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy to clipboard: {ex.Message}",
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteUser_Click(User user)
        {
            // Check if trying to delete self
            if (user.Username == _username)
            {
                MessageBox.Show("You cannot delete your own account!",
                              "Cannot Delete", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if last admin
            if (user.Role == "Administrator")
            {
                int adminCount = _allUsers.Count(u => u.Role == "Administrator");
                if (adminCount <= 1)
                {
                    MessageBox.Show("Cannot delete the last Administrator!\n\n" +
                                  "There must be at least one Administrator in the system.",
                                  "Cannot Delete", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            var result = MessageBox.Show($"Delete user {user.FullName}?\n\n" +
                                       $"Username: {user.Username}\n" +
                                       $"Role: {user.Role}\n\n" +
                                       $"This action cannot be undone!",
                                       "Confirm Deletion",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _allUsers.Remove(user);
                ApplyFilters();
                MessageBox.Show($"User {user.FullName} has been deleted.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}