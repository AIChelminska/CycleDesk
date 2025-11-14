/*Do czego: Każdy rower, opona, łańcuch itp. w sklepie
Co robi:
* Przechowuje dane produktu (nazwa, cena, ilość na stanie)
* Połączenie do kategorii (Rowery → Mountain Bike)
* Połączenie do dostawcy (kto dostarcza)
* MinStock → alert gdy mało towaru*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleDesk.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public int CategoryId { get; set; }
        public int SupplierId { get; set; }
        public int Stock { get; set; }
        public int MinStock { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public string Description { get; set; }
        public byte[] ImageData { get; set; }  
        public string ImageFileName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Category Category { get; set; }
        public Supplier Supplier { get; set; }
    }
}
