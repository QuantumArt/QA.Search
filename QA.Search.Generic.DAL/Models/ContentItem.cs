using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QA.Search.Generic.DAL.Models
{
    [Table("content_item")]
    public class ContentItem
    {
        [Key]
        [Column("content_item_id")]
        public decimal ContentItemID { get; set; }
        [ForeignKey("status_type_id")]
        public StatusType StatusType { get; set; }
        [ForeignKey("content_id")]
        public Content Content { get; set; }
        [Column("splitted")]
        public bool Splitted { get; set; }
    }
}
