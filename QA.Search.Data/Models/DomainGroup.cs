using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QA.Search.Data.Models
{
    public class DomainGroup
    {
        public int Id { get; set; }
        [MaxLength(450)]
        public string Name { get; set; }
        public string IndexingConfig { get; set; } = "null";

        public ICollection<Domain> Domains { get; set; } = Array.Empty<Domain>();
        public ICollection<Route> Routes { get; set; } = Array.Empty<Route>();
    }
}
