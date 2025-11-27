using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CycleDesk.Models
{
    [Table("SaleItems")]
    public class SaleItem
    {
        [Key]
        [Column("SaleItemID")]
        public int SaleItemId { get; set; }

        [Column("SaleID")]
        public int SaleId { get; set; }

        [Column("ProductID")]
        public int ProductId { get; set; }

        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal VATRate { get; set; }
        public decimal VATAmount { get; set; }
        public decimal TotalPrice { get; set; }
    }
}