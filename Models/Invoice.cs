using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CycleDesk.Models
{
    [Table("Invoices")]
    public class Invoice
    {
        [Key]
        [Column("InvoiceID")]
        public int InvoiceId { get; set; }

        public string InvoiceNumber { get; set; }
        public string InvoiceType { get; set; }

        [Column("SaleID")]
        public int SaleId { get; set; }

        [Column("CustomerID")]
        public int? CustomerId { get; set; }

        public DateTime IssueDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string PaymentMethod { get; set; }
        public decimal SubtotalAmount { get; set; }
        public decimal VATAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }

        [Column("IssuedByUserID")]
        public int IssuedByUserId { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}