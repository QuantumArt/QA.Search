using System.ComponentModel.DataAnnotations.Schema;

namespace QA.Search.Generic.DAL.Models
{
    [Table("item_to_item")]
    public class Item2Item
    {
        [Column("link_id")]
        public decimal LinkId { get; set; }
        [Column("l_item_id")]
        public decimal LeftItemId { get; set; }
        [Column("r_item_id")]
        public decimal RightItemId { get; set; }
        [Column("is_rev")]
        public bool IsReverse { get; set; }
        [Column("is_self")]
        public bool IsSelf { get; set; }
    }
}
