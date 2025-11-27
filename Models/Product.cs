using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CycleDesk.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        [Column("ProductID")]
        public int ProductId { get; set; }

        public string SKU { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }

        [Column("CategoryID")]
        public int CategoryId { get; set; }

        [Column("SupplierID")]
        public int? SupplierId { get; set; }

        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal VAT { get; set; }
        public string Unit { get; set; }
        public string Barcode { get; set; }
        public int MinimumStock { get; set; }
        public int ReorderLevel { get; set; }
        public string ImagePath { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}