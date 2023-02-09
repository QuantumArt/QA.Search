using System;
using System.ComponentModel.DataAnnotations;

namespace QA.Search.Data.Models
{
    public class Domain
    {
        [Key]
        [MaxLength(450)]
        public string Origin { get; set; }

        public int DomainGroupId { get; set; }
        [Required]
        public DomainGroup DomainGroup { get; set; }

        
        public DateTime? LastFastCrawlingUtc { get; set; }
        public DateTime? LastDeepCrawlingUtc { get; set; }
    }
}
