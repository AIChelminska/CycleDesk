using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CycleDesk.Models
{
    [Table("ApplicationSettings")]
    public class ApplicationSettings
    {
        [Key]
        [Column("SettingID")]
        public int SettingId { get; set; }
        
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; }
        public DateTime? ModifiedDate { get; set; }
        
        [Column("ModifiedByUserID")]
        public int? ModifiedByUserId { get; set; }
    }
}
