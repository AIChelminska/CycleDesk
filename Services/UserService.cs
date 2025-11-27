using CycleDesk.Data;
using CycleDesk.Models;

namespace CycleDesk.Services
{
    /// <summary>
    /// Serwis do obsługi użytkowników używający Entity Framework Core
    /// Dostosowany do modelu User: UserId, Username, FullName, PasswordHash, Role, IsActive, CreatedAt, LastLoginAt
    /// </summary>
    public class UserService
    {
        // ===== CREATE =====
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
                existingUser.Role = user.Role;
                existingUser.IsActive = user.IsActive;
                existingUser.ModifiedDate = DateTime.Now;

                context.SaveChanges();
                return true;
            }
        }

        public bool UpdatePassword(int userId, string newPasswordHash)
        {
            using (var context = new CycleDeskDbContext())
            {
                var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null) return false;

                user.PasswordHash = newPasswordHash;
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
                        u.Role.ToLower().Contains(term))
                    .OrderByDescending(u => u.CreatedDate)
                    .ToList();
            }
        }

        // ===== HELPERS =====
        public string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public string HashPassword(string password)
        {
            // UWAGA: W produkcji użyj BCrypt.Net-Next!
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }
    }
}