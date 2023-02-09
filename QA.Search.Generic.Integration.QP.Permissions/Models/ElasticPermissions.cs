using System;

namespace QA.Search.Generic.Integration.QP.Permissions.Models
{
    public class IndexesByRoles
    {
        public string Role { get; set; } = string.Empty;
        public string[] Indexes { get; set; } = Array.Empty<string>();
    }
}
