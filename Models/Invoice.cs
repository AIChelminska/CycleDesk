/*Do czego: Faktury
Co robi:
* Generuje fakturę do druku z każdej sprzedaży
* Nr faktury (INV-001234)
* Data wystawienia i termin płatności
* Status: Opłacona / Nieopłacona / Zaległa*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleDesk.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public string DocumentNumber { get; set; } 
        public string DocumentType { get; set; }  // Receipt, Invoice
        public int SaleId { get; set; }
        public int? CustomerId { get; set; }
        public DateTime DocumentDate { get; set; }
        public DateTime? DueDate { get; set; }  // NULL dla receiptu (już zapłacone)
        public decimal Amount { get; set; }
        public string Status { get; set; } // Paid, Unpaid, Overdue
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Sale Sale { get; set; }
        public Customer Customer { get; set; }
    }
}
