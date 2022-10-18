using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace QA.Search.Generic.DAL.Models
{
    [Table("content")]
    public class Content
    {
        [Column("content_id")]
        public decimal ContentID { get; set; }
        [Column("content_name")]
        public string ContentName { get; set; }
        [Column("description")]
        public string Description { get; set; }
        [Column("site_id")]
        public decimal SiteID { get; set; }
        [Column("created")]
        public DateTime CreateDate { get; set; }
        [Column("modified")]
        public DateTime ModifyDate { get; set; }
        [Column("map_as_class")]
        public bool MapAsClass { get; set; }
        [Column("net_content_name")]
        public string DotNetName { get; set; }
        [Column("net_plural_content_name")]
        public string DotNetPluralName { get; set; }
        [Column("use_default_filtration")]
        public bool UseDefaultFiltration { get; set; }
    }
}
