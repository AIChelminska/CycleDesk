/* Do czego: Przechowuje dane klientów sklepu (osoby fizyczne i firmy)
Co robi:
* Zapisuje kompletne dane kontaktowe klienta (imię, nazwisko, email, telefon, adres)
* Obsługuje firmy (CompanyName + NIP) i osoby prywatne
* Używane przy płatnościach przelewem - potrzebne do wystawienia faktury
* Opcjonalne przy sprzedaży - walk-in customers (gotówka/karta) nie potrzebują Customer
* Pozwala na budowanie bazy stałych klientów (CRM)
* Jedno połączenie Customer → wiele Sale (klient może robić wiele zakupów)
* Jedno połączenie Customer → wiele Invoice (klient może mieć wiele faktur)*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleDesk.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string NIP { get; set; } // Numer identyfikacji podatkowej (dla firm)
        public string CompanyName { get; set; } // Jeśli to klient biznesowy
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
