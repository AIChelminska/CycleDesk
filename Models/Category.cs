/*Do czego: Organizacja produktów
Co robi:
* Grupuje produkty (Rowery, Opony, Akcesoria, itp.)
* Każda kategoria ma ikonę (emoji)
* Jeden produkt = jedna kategoria*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleDesk.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
