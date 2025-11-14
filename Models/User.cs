/*Do czego: Przechowuje dane użytkowników aplikacji
Co robi:
* Rejestruje użytkowników (login, hasło)
* Przydziela role: Manager (zatwierdzanie), Operator (magazyn), Cashier (kasa)
* Śledzi ostatnie logowanie
* Aktywne/nieaktywne konta*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleDesk.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // Manager, Operator, Cashier
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}
