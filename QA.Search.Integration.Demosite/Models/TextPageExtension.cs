using QA.Search.Generic.DAL.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QA.Search.Integration.Demosite.Models
{
    public class TextPageExtension : GenericItem
    {
        [Column("itemid")]
        public int? ItemID { get; set; }

        [Column("text")]
        public string? Text { get; set; }
    }
}
