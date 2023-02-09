using System.ComponentModel.DataAnnotations.Schema;
using QA.Search.Generic.DAL.Models;

namespace QA.Search.Integration.Demosite.Models
{
    public class NewsCategory : GenericItem
    {
        [Column("itemid")]
        public int ItemId { get; set; }
    }
}
