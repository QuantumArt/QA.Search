using System.Collections.Generic;

namespace QA.Search.Generic.Integration.QP.Permissions.Models.DTO
{
    public class ContentInformation
    {
        public decimal ContentItemId { get; set; }
        public string ContentName { get; set; } = string.Empty;
        public string IndexName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public decimal? ParentId { get; set; }
    }
}
