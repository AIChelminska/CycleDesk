using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CycleDesk.Models
{
    [Table("AccessCodes")]
    public class AccessCode
    {
        [Key]
        [Column("AccessCodeID")]
        public int AccessCodeId { get; set; }

        public string Code { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string AccountType { get; set; }
        public bool IsUsed { get; set; }

        [Column("UsedByUserID")]
        public int? UsedByUserId { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UsedDate { get; set; }

        [Column("CreatedByUserID")]
        public int? CreatedByUserId { get; set; }
    }
}
