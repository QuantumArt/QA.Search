using System.ComponentModel.DataAnnotations.Schema;

#nullable enable

namespace QA.Search.Generic.DAL.Models
{
    public class StartPageExtension : GenericItem
    {
        [Column("itemid")]
        public int ItemId { get; set; }
        [Column("defaulthost")]
        public string? DefaultHost { get; set; }
    }
}
