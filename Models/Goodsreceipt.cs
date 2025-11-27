using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CycleDesk.Models
{
    [Table("GoodsReceipts")]
    public class GoodsReceipt
    {
        [Key]
        [Column("GoodsReceiptID")]
        public int GoodsReceiptId { get; set; }

        public string ReceiptNumber { get; set; }

        [Column("SupplierID")]
        public int SupplierId { get; set; }

        public DateTime ReceiptDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public string Notes { get; set; }

        [Column("ReceivedByUserID")]
        public int ReceivedByUserId { get; set; }

        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}