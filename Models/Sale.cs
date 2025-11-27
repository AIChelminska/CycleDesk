using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CycleDesk.Models
{
    [Table("Sales")]
    public class Sale
    {
        [Key]
        [Column("SaleID")]
        public int SaleId { get; set; }

        public string SaleNumber { get; set; }
        public DateTime SaleDate { get; set; }

        [Column("CustomerID")]
        public int? CustomerId { get; set; }

        public string CustomerName { get; set; }
        public string PaymentMethod { get; set; }
        public decimal SubtotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal VATAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal ChangeAmount { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }

        [Column("SoldByUserID")]
        public int SoldByUserId { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}