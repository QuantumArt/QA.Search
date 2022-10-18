using System.ComponentModel.DataAnnotations.Schema;

namespace QA.Search.Generic.DAL.Models
{
    [Table("status_type_new")]
    public class StatusType
    {
        [Column("id")]
        public int ID { get; set; }
        [Column("name")]
        public string StatusTypeName { get; set; }
        [Column("weight")]
        public int Weight { get; set; }
        [Column("site_id")]
        public int SiteID { get; set; }
    }
}
