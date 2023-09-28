using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QA.Search.Generic.DAL.Models
{
    [Table("content_to_content")]
    public class Content2Content
    {
        [Key]
        [Column("link_id")]
        public decimal LinkId { get; set; }
        [ForeignKey("l_content_id")]
        public Content LeftContent { get; set; }
        [ForeignKey("r_content_id")]
        public Content RightContent { get; set; }
    }
}
