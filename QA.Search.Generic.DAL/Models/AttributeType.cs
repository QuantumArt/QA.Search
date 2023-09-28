using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QA.Search.Generic.DAL.Models
{
    [Table("attribute_type")]
    public class AttributeType
    {
        [Key]
        [Column("attribute_type_id")]
        public decimal AttributeTypeId { get; set; }
        [Column("type_name")]
        public string TypeName { get; set; }
    }
}
