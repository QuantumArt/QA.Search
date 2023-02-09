using System;

namespace QA.Search.Generic.Integration.QP.Permissions.Configuration
{
    public class PermissionsConfiguration
    {
        public string DefaultRoleAlias { get; set; } = string.Empty;
        public string PermissionIndexName { get; set; } = string.Empty;
        public string[] QpAbstractItems { get; set; } = Array.Empty<string>();
    }
}
