using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QA.Search.Data.Models
{
    public class Route
    {
        public int Id { get; set; }

        public int DomainGroupId { get; set; }
        [Required]
        public DomainGroup DomainGroup { get; set; }

        [Column("route")]
        public string RouteText { get; set; }
        public int ScanPeriodMsec { get; set; }
        public string IndexingConfig { get; set; } = "null";
    }
}
