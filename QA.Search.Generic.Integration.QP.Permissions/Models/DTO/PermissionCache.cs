using System.Collections.Generic;

namespace QA.Search.Generic.Integration.QP.Permissions.Models.DTO
{
    public class PermissionCache
    {
        public List<string> Roles { get; set; } = new List<string>();
        public decimal ContentId { get; set; }
    }
}
