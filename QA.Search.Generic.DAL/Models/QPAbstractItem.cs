using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace QA.Search.Generic.DAL.Models
{
    public class QPAbstractItem : GenericItem
    {
        public QPAbstractItem()
        {
            Children = new HashSet<QPAbstractItem>();
        }

        [Column("title")]
        public string Title { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("zonename")]
        public string ZoneName { get; set; }

        [Column("parent")]
        public int? ParentID { get; set; }
        public QPAbstractItem Parent { get; set; }
        public ICollection<QPAbstractItem> Children { get; set; }

        [Column("discriminator")]
        public int? DiscriminatorID { get; set; }
        public QPDiscriminator Discriminator { get; set; }

        [Column("extensionid")]
        public int? ExtensionID { get; set; }
    }
}
