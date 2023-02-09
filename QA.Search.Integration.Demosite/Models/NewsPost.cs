using QA.Search.Generic.DAL.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QA.Search.Integration.Demosite.Models
{
    public class NewsPost : GenericItem
    {
        [Column("category")]
        public int? Category { get; set; }

        [Column("title")]
        public string? Title { get; set; }

        [Column("brief")]
        public string? Description { get; set; }

        [Column("text")]
        public string? Text { get; set; }

        [Column("postdate")]
        public DateTime PostDate { get; set; }
    }
}
