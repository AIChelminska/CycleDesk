/*Do czego: Dostawcy towarów
Co robi:
* Przechowuje kontakt do dostawcy (email, telefon, adres)
* Każdy produkt wie od kogo pochodzi
* Można zamawiać od konkretnego dostawcy*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleDesk.Models
{
    public class Supplier
    {
        public int SupplierId { get; set; }
        public string Name { get; set; }
        public string ContactEmail { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
