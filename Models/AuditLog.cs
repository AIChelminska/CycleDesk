using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CycleDesk.Models
{
    [Table("AuditLog")]
    public class AuditLog
    {
        [Key]
        [Column("AuditID")]
        public int AuditId { get; set; }
        
        public string TableName { get; set; }
        public int RecordId { get; set; }
        public string Action { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        
        [Column("UserID")]
        public int? UserId { get; set; }
        
        public DateTime ActionDate { get; set; }
    }
}
