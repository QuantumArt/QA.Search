using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QA.Search.Generic.DAL.Models
{
    [Table("content_attribute")]
    public class ContentAttribute
    {
        [Key]
        [Column("attribute_id")]
        public decimal AttributeId { get; set; }
        [Column("content_id")]
        public decimal ContentID { get; set; }
        [Column("attribute_name")]
        public string AttributeName { get; set; }
        [Column("default_value")]
        public string DefaultValue { get; set; }
        [Column("friendly_name")]
        public string FrendlyName { get; set; }
        [ForeignKey("attribute_type_id")]
        public AttributeType AttributeType { get; set; }
        [ForeignKey("link_id")]
        public Content2Content Content2Content { get; set; }
    }
}
