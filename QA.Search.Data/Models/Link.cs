using System;
using System.ComponentModel.DataAnnotations;

namespace QA.Search.Data.Models
{
    public class Link
    {
        [Key]
        public string Hash { get; set; }
        public string Url { get; set; }
        public DateTime NextIndexingUtc { get; set; }
        public bool IsActive { get; set; }
        public long Version { get; set; }
    }
}
