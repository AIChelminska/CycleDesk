using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CycleDesk.Models
{
    [Table("GoodsReceiptItems")]
    public class GoodsReceiptItem
    {
        [Key]
        [Column("GoodsReceiptItemID")]
        public int GoodsReceiptItemId { get; set; }

        [Column("GoodsReceiptID")]
        public int GoodsReceiptId { get; set; }

        [Column("ProductID")]
        public int ProductId { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string BatchNumber { get; set; }
    }
}
