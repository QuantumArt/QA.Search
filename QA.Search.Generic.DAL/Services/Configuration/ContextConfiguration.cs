namespace QA.Search.Generic.DAL.Services.Configuration
{
    public class ContextConfiguration
    {
        public string ConnectionString { get; set; }
        public SqlServerType SqlServerType { get; set; }
        public ContentAccess ContentAccess { get; set; }
        public string DefaultSchemeName { get; set; }
        public string FormatTableName { get; set; }
    }
}
