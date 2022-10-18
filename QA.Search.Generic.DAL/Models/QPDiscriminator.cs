using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace QA.Search.Generic.DAL.Models
{
    public class QPDiscriminator : GenericItem
    {
        public QPDiscriminator()
        {
            AbstractItems = new HashSet<QPAbstractItem>();
        }

        [Column("title")]
        public string Title { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("description")]
        public string Description { get; set; }
        [Column("preferredcontentid")]
        public int? PreferredContentID { get; set; }
        [Column("ispage")]
        public bool IsPage { get; set; }

        public ICollection<QPAbstractItem> AbstractItems { get; set; }
    }
}
