using BCrypt.Net;
using CycleDesk.Data;
using CycleDesk.Models;

namespace CycleDesk.Services
{
    /// <summary>
    /// Serwis do obsługi użytkowników używający Entity Framework Core
    /// Hashowanie hasła za pomocą BCrypt
    /// </summary>
    public class UserService
    {
        // ===== CREATE =====
        
        /// <summary>
        /// Tworzy nowego użytkownika z hashowaniem hasła BCrypt
        /// </summary>
        /// <param name="user">Obiekt użytkownika</param>
        /// <param name="plainPassword">Hasło w formie jawnej (zostanie zahashowane)</param>
        /// <returns>ID utworzonego użytkownika</returns>
        public int CreateUser(User user, string plainPassword)
        {
            using (var context = new CycleDeskDbContext())
            {
                // Hashuj hasło BCrypt
                user.PasswordHash = HashPassword(plainPassword);
                user.CreatedDate = DateTime.Now;
                user.IsActive = true;
                
                context.Users.Add(user);
                context.SaveChanges();
                return user.UserId;
            }
        }

        /// <summary>
        /// Tworzy nowego użytkownika (bez podawania hasła - używa już zahashowanego)
        /// </summary>
        public int CreateUser(User user)
        {
            using (var context = new CycleDeskDbContext())
            {
                user.CreatedDate = DateTime.Now;
                user.IsActive = true;
                context.Users.Add(user);
                context.SaveChanges();
                return user.UserId;
            }
        }

        // ===== READ =====
        public List<User> GetAllUsers()
        {
            using (var context = new CycleDeskDbContext())
            {
                return context.Users
                    .OrderByDescending(u => u.CreatedDate)
                    .ToList();
            }
        }

        public List<User> GetActiveUsers()
        {
            using (var context = new CycleDeskDbContext())
            {
                return context.Users
                    .Where(u => u.IsActive)
                    .OrderByDescending(u => u.CreatedDate)
                    .ToList();
            }
        }

        public User GetUserById(int userId)
        {
            using (var context = new CycleDeskDbContext())
            {
                return context.Users.FirstOrDefault(u => u.UserId == userId);
            }
        }

        public User GetUserByUsername(string username)
        {
            using (var context = new CycleDeskDbContext())
            {
                return context.Users.FirstOrDefault(u => u.Username == username);
            }
        }

        // ===== UPDATE =====
        public bool UpdateUser(User user)
        {
            using (var context = new CycleDeskDbContext())
            {
                var existingUser = context.Users.FirstOrDefault(u => u.UserId == user.UserId);
                if (existingUser == null) return false;

                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.Username = user.Username;
                existingUser.Email = user.Email;
                existingUser.Phone = user.Phone;
                existingUser.Role = user.Role;
                existingUser.IsActive = user.IsActive;
                existingUser.ModifiedDate = DateTime.Now;

                context.SaveChanges();
                return true;
            }
        }

        /// <summary>
        /// Aktualizuje hasło użytkownika (przyjmuje jawne hasło, hashuje BCrypt)
        /// </summary>
        public bool UpdatePassword(int userId, string newPlainPassword)
        {
            using (var context = new CycleDeskDbContext())
            {
                var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null) return false;

                user.PasswordHash = HashPassword(newPlainPassword);
                user.ModifiedDate = DateTime.Now;
                context.SaveChanges();
                return true;
            }
        }

        public bool ActivateUser(int userId)
        {
            using (var context = new CycleDeskDbContext())
            {
                var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null) return false;

                user.IsActive = true;
                user.ModifiedDate = DateTime.Now;
                context.SaveChanges();
                return true;
            }
        }

        public bool DeactivateUser(int userId)
        {
            using (var context = new CycleDeskDbContext())
            {
                var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null) return false;

                user.IsActive = false;
                user.ModifiedDate = DateTime.Now;
                context.SaveChanges();
                return true;
            }
        }

        public bool UpdateLastLogin(int userId)
        {
            using (var context = new CycleDeskDbContext())
            {
                var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null) return false;

                user.LastLoginDate = DateTime.Now;
                context.SaveChanges();
                return true;
            }
        }

        // ===== DELETE =====
        public bool DeleteUser(int userId)
        {
            using (var context = new CycleDeskDbContext())
            {
                var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null) return false;

                context.Users.Remove(user);
                context.SaveChanges();
                return true;
            }
        }

        // ===== VALIDATION =====
        public bool IsUsernameAvailable(string username, int? excludeUserId = null)
        {
            using (var context = new CycleDeskDbContext())
            {
                if (excludeUserId.HasValue)
                {
                    return !context.Users.Any(u => u.Username == username && u.UserId != excludeUserId.Value);
                }
                return !context.Users.Any(u => u.Username == username);
            }
        }

        public bool IsEmailAvailable(string email, int? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(email)) return true;
            
            using (var context = new CycleDeskDbContext())
            {
                if (excludeUserId.HasValue)
                {
                    return !context.Users.Any(u => u.Email == email && u.UserId != excludeUserId.Value);
                }
                return !context.Users.Any(u => u.Email == email);
            }
        }

        // ===== STATISTICS =====
        public Dictionary<string, int> GetUserStatistics()
        {
            using (var context = new CycleDeskDbContext())
            {
                var users = context.Users.ToList();
                return new Dictionary<string, int>
                {
                    { "Total", users.Count },
                    { "Active", users.Count(u => u.IsActive) },
                    { "Inactive", users.Count(u => !u.IsActive) },
                    { "Manager", users.Count(u => u.Role == "Manager") },
                    { "Supervisor", users.Count(u => u.Role == "Supervisor") },
                    { "Operator", users.Count(u => u.Role == "Operator") },
                    { "Cashier", users.Count(u => u.Role == "Cashier") }
                };
            }
        }

        // ===== SEARCH =====
        public List<User> SearchUsers(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllUsers();

            using (var context = new CycleDeskDbContext())
            {
                var term = searchTerm.ToLower();
                return context.Users
                    .Where(u =>
                        u.Username.ToLower().Contains(term) ||
                        u.FirstName.ToLower().Contains(term) ||
                        u.LastName.ToLower().Contains(term) ||
                        u.Role.ToLower().Contains(term) ||
                        (u.Email != null && u.Email.ToLower().Contains(term)))
                    .OrderByDescending(u => u.CreatedDate)
                    .ToList();
            }
        }

        // ===== PASSWORD HELPERS (BCrypt) =====
        
        /// <summary>
        /// Hashuje hasło za pomocą BCrypt
        /// </summary>
        public string HashPassword(string plainPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(plainPassword, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        /// <summary>
        /// Weryfikuje hasło za pomocą BCrypt
        /// </summary>
        public bool VerifyPassword(string plainPassword, string hashedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Generuje tymczasowe hasło
        /// </summary>
        public string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$%";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // ===== AUTHENTICATION =====
        
        /// <summary>
        /// Uwierzytelnia użytkownika
        /// </summary>
        public User Authenticate(string username, string plainPassword)
        {
            using (var context = new CycleDeskDbContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Username == username);
                
                if (user == null) return null;
                if (!user.IsActive) return null;
                if (!VerifyPassword(plainPassword, user.PasswordHash)) return null;
                
                // Aktualizuj ostatnie logowanie
                user.LastLoginDate = DateTime.Now;
                context.SaveChanges();
                
                return user;
            }
        }

        // ===== ACCESS CODE =====
        
        /// <summary>
        /// Generuje 11-cyfrowy kod dostępu
        /// </summary>
        public string GenerateAccessCode()
        {
            var random = new Random();
            return string.Join("", Enumerable.Range(0, 11).Select(_ => random.Next(0, 10)));
        }

        /// <summary>
        /// Tworzy nowego użytkownika z kodem dostępu (bez hasła - użytkownik sam je ustawi)
        /// </summary>
        /// <param name="firstName">Imię</param>
        /// <param name="lastName">Nazwisko</param>
        /// <param name="username">Nazwa użytkownika</param>
        /// <param name="email">Email (opcjonalny)</param>
        /// <param name="phone">Telefon (opcjonalny)</param>
        /// <param name="role">Rola</param>
        /// <param name="createdByUserId">ID użytkownika tworzącego</param>
        /// <returns>Wygenerowany kod dostępu</returns>
        public string CreateUserWithAccessCode(string firstName, string lastName, string username, 
            string email, string phone, string role, int createdByUserId)
        {
            using (var context = new CycleDeskDbContext())
            {
                // Generuj unikalny kod dostępu
                string accessCode;
                do
                {
                    accessCode = GenerateAccessCode();
                } while (context.AccessCodes.Any(ac => ac.Code == accessCode && !ac.IsUsed));

                // Utwórz wpis AccessCode
                var newAccessCode = new AccessCode
                {
                    Code = accessCode,
                    FirstName = firstName,
                    LastName = lastName,
                    Username = username,
                    AccountType = role,
                    IsUsed = false,
                    CreatedDate = DateTime.Now,
                    CreatedByUserId = createdByUserId
                };

                context.AccessCodes.Add(newAccessCode);
                context.SaveChanges();

                return accessCode;
            }
        }

        /// <summary>
        /// Weryfikuje kod dostępu i zwraca dane użytkownika
        /// </summary>
        public AccessCode ValidateAccessCode(string code)
        {
            using (var context = new CycleDeskDbContext())
            {
                return context.AccessCodes
                    .FirstOrDefault(ac => ac.Code == code && !ac.IsUsed);
            }
        }

        /// <summary>
        /// Aktywuje konto użytkownika po rejestracji (używa kodu dostępu)
        /// </summary>
        public User ActivateUserAccount(string accessCode, string password)
        {
            using (var context = new CycleDeskDbContext())
            {
                var code = context.AccessCodes.FirstOrDefault(ac => ac.Code == accessCode && !ac.IsUsed);
                if (code == null) return null;

                // Utwórz użytkownika
                var newUser = new User
                {
                    FirstName = code.FirstName,
                    LastName = code.LastName,
                    Username = code.Username,
                    PasswordHash = HashPassword(password),
                    Role = code.AccountType,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                context.Users.Add(newUser);
                context.SaveChanges();

                // Oznacz kod jako użyty
                code.IsUsed = true;
                code.UsedDate = DateTime.Now;
                code.UsedByUserId = newUser.UserId;
                context.SaveChanges();

                return newUser;
            }
        }

        /// <summary>
        /// Pobiera listę niewykorzystanych kodów dostępu
        /// </summary>
        public List<AccessCode> GetPendingAccessCodes()
        {
            using (var context = new CycleDeskDbContext())
            {
                return context.AccessCodes
                    .Where(ac => !ac.IsUsed)
                    .OrderByDescending(ac => ac.CreatedDate)
                    .ToList();
            }
        }

        /// <summary>
        /// Usuwa niewykorzystany kod dostępu
        /// </summary>
        public bool DeleteAccessCode(int accessCodeId)
        {
            using (var context = new CycleDeskDbContext())
            {
                var code = context.AccessCodes.FirstOrDefault(ac => ac.AccessCodeId == accessCodeId && !ac.IsUsed);
                if (code == null) return false;

                context.AccessCodes.Remove(code);
                context.SaveChanges();
                return true;
            }
        }

        /// <summary>
        /// Tworzy kod resetu hasła dla istniejącego użytkownika
        /// </summary>
        public string CreatePasswordResetCode(int userId, int createdByUserId)
        {
            using (var context = new CycleDeskDbContext())
            {
                var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null) return null;

                // Usuń poprzednie nieużyte kody resetu dla tego użytkownika
                var oldCodes = context.AccessCodes
                    .Where(ac => ac.Username == user.Username && 
                                 ac.AccountType.StartsWith("PasswordReset:") && 
                                 !ac.IsUsed)
                    .ToList();
                context.AccessCodes.RemoveRange(oldCodes);

                // Generuj unikalny kod
                string resetCode;
                do
                {
                    resetCode = GenerateAccessCode();
                } while (context.AccessCodes.Any(ac => ac.Code == resetCode && !ac.IsUsed));

                // Utwórz wpis AccessCode z typem PasswordReset
                var newAccessCode = new AccessCode
                {
                    Code = resetCode,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Username = user.Username,
                    AccountType = $"PasswordReset:{userId}",
                    IsUsed = false,
                    CreatedDate = DateTime.Now,
                    CreatedByUserId = createdByUserId
                };

                context.AccessCodes.Add(newAccessCode);
                context.SaveChanges();

                return resetCode;
            }
        }

        /// <summary>
        /// Weryfikuje kod resetu hasła
        /// </summary>
        public AccessCode ValidatePasswordResetCode(string code)
        {
            using (var context = new CycleDeskDbContext())
            {
                return context.AccessCodes
                    .FirstOrDefault(ac => ac.Code == code && 
                                         !ac.IsUsed && 
                                         ac.AccountType.StartsWith("PasswordReset:"));
            }
        }

        /// <summary>
        /// Resetuje hasło użytkownika używając kodu resetu
        /// </summary>
        public bool ResetPasswordWithCode(string resetCode, string newPassword)
        {
            using (var context = new CycleDeskDbContext())
            {
                var code = context.AccessCodes
                    .FirstOrDefault(ac => ac.Code == resetCode && 
                                         !ac.IsUsed && 
                                         ac.AccountType.StartsWith("PasswordReset:"));
                
                if (code == null) return false;

                var parts = code.AccountType.Split(':');
                if (parts.Length != 2 || !int.TryParse(parts[1], out int userId))
                    return false;

                var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null) return false;

                user.PasswordHash = HashPassword(newPassword);
                user.ModifiedDate = DateTime.Now;

                code.IsUsed = true;
                code.UsedDate = DateTime.Now;
                code.UsedByUserId = userId;

                context.SaveChanges();
                return true;
            }
        }

        // ===== ROLE CHECKING =====
        
        /// <summary>
        /// Sprawdza czy użytkownik ma uprawnienia do zarządzania użytkownikami
        /// </summary>
        public bool CanManageUsers(string role)
        {
            return role == "Manager" || role == "Supervisor";
        }

        /// <summary>
        /// Zwraca listę dostępnych ról
        /// </summary>
        public List<string> GetAvailableRoles()
        {
            return new List<string> { "Manager", "Supervisor", "Operator", "Cashier" };
        }

        /// <summary>
        /// Zwraca liczbę użytkowników z daną rolą
        /// </summary>
        public int GetUserCountByRole(string role)
        {
            using (var context = new CycleDeskDbContext())
            {
                return context.Users.Count(u => u.Role == role);
            }
        }

        /// <summary>
        /// Sprawdza czy można usunąć użytkownika (np. nie można usunąć ostatniego Managera)
        /// </summary>
        public bool CanDeleteUser(int userId, out string reason)
        {
            reason = string.Empty;
            
            using (var context = new CycleDeskDbContext())
            {
                var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null)
                {
                    reason = "User not found.";
                    return false;
                }

                // Sprawdź czy to ostatni Manager
                if (user.Role == "Manager")
                {
                    int managerCount = context.Users.Count(u => u.Role == "Manager");
                    if (managerCount <= 1)
                    {
                        reason = "Cannot delete the last Manager. There must be at least one Manager in the system.";
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
