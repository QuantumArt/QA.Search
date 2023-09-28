using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QA.Search.Generic.DAL.Models
{
    public class AppSetting
    {
        [Key]
        [Column("key")]
        public string Key { get; set; }
        [Column("value")]
        public string Value { get; set; }
    }
}
