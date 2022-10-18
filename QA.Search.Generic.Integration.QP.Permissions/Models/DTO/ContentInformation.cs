using System.Collections.Generic;

namespace QA.Search.Generic.Integration.QP.Permissions.Models.DTO
{
    public class ContentInformation
    {
        public decimal ContentItemId { get; set; }
        public string ContentName { get; set; }
        public string IndexName { get; set; }
        public List<string> Roles { get; set; }
        public decimal? ParentId { get; set; }
    }
}
