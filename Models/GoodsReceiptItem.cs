/*Do czego: Linie w dokumencie PZ
Co robi:
* Jedna pozycja w dokumencie (np. 50 opon po 30 PLN)
* Połączenie między PZ i produktem
* Zapisuje: jaki produkt, ile sztuk, jaką cenę*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleDesk.Models
{
    public class GoodsReceiptItem
    {
        public int GoodsReceiptItemId { get; set; }
        public int GoodsReceiptId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public GoodsReceipt GoodsReceipt { get; set; }
        public Product Product { get; set; }
    }
}
