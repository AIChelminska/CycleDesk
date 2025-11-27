using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CycleDesk.Models
{
    [Table("InventoryAdjustments")]
    public class StockHistory
    {
        [Key]
        [Column("AdjustmentID")]
        public int AdjustmentId { get; set; }

        [Column("ProductID")]
        public int ProductId { get; set; }

        public string AdjustmentType { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; }
        public DateTime AdjustmentDate { get; set; }

        [Column("AdjustedByUserID")]
        public int AdjustedByUserId { get; set; }
    }
}
