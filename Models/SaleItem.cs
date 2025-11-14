/*Do czego: Linie w sprzedaży
Co robi:
* Jeden produkt w koszyku (np. 2x opony po 89 PLN)
* Ilość, cena jednostkowa, rabat
* Oblicza całość: cena × ilość - rabat*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleDesk.Models
{
    public class SaleItem
    {
        public int SaleItemId { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal Total { get; set; }

        // Navigation
        public Sale Sale { get; set; }
        public Product Product { get; set; }
    }
}
