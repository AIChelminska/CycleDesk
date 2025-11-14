/*Do czego: Dokument przyjęcia towaru (PZ)
Co robi:
* Tworzy dokument gdy przychodzi dostawa
* Status: Draft (niedokończony) → Pending (czeka na zatwierdzenie) → Approved (zatwierdzone)
* Tylko Manager może zatwierdzić
* Jak zatwierdzimy → automatycznie zwiększa się stan magazynowy*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleDesk.Models
{
    public class GoodsReceipt
    {
        public int GoodsReceiptId { get; set; }
        public string DocumentNumber { get; set; }
        public int SupplierId { get; set; }
        public DateTime DocumentDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string Status { get; set; } // Draft, Pending, Approved, Rejected
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        // Navigation
        public Supplier Supplier { get; set; }
        public User CreatedByUser { get; set; }
        public User ApprovedByUser { get; set; }
        public ICollection<GoodsReceiptItem> Items { get; set; } = new List<GoodsReceiptItem>();
    }
}
