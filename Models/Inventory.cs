using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CycleDesk.Models
{
    [Table("Inventory")]
    public class Inventory
    {
        [Key]
        [Column("InventoryID")]
        public int InventoryId { get; set; }

        [Column("ProductID")]
        public int ProductId { get; set; }

        public int QuantityInStock { get; set; }
        public DateTime LastStockUpdate { get; set; }
        public string LocationInWarehouse { get; set; }
    }
}