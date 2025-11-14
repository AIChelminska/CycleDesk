/*Do czego: Dziennik zmian magazynu
Co robi:
* Zapisuje KAŻDĄ zmianę stanu:
* Sprzedaż (ile zmniejszyło)
* Przyjęcie (ile zwiększyło)
* Manualny nowy limit
* Kto zmienił i kiedy
* Audyt - widzisz całą historię*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleDesk.Models
{
    public class StockHistory
    {
        public int StockHistoryId { get; set; }
        public int ProductId { get; set; }
        public int OldQuantity { get; set; }
        public int NewQuantity { get; set; }
        public string Reason { get; set; } // Sale, GoodsReceipt, Adjustment
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Product Product { get; set; }
        public User User { get; set; }
    }
}
