/*Do czego: Sprzedaż = transakcja w kasie
Co robi:
* Zapisuje całą sprzedaż (nr faktury, data, kwota)
* Metoda płatności: Gotówka / Karta / Przelew
* Rabaty
* Kto sprzedał (Cashier)
* Jak kompletna → automatycznie zmniejsza się stan magazynowy*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleDesk.Models
{
    public class Sale
    {
        public int SaleId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime SaleDate { get; set; }
        public int? CustomerId { get; set; }  // ← OPCJONALNIE (NULL = Walk-in)
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; } // Cash, Card, Transfer
        public string Status { get; set; } // Completed, Refunded, Cancelled
        public int CashierId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Customer Customer { get; set; } 
        public User Cashier { get; set; }
        public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
        public Invoice Invoice { get; set; }  
    }
}
