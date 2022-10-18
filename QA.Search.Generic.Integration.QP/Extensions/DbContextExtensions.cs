using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using QA.Search.Generic.DAL.Extensions;
using System;
using System.Linq;

namespace QA.Search.Generic.Integration.QP.Extensions
{
    public static class DbContextExtensions
    {
        /// <summary>
        /// Get schema qualified table name (like dbo.Users) by entity type
        /// </summary>
        public static string GetTableFullName(this DbContext dbContext, Type type)
        {
            IEntityType entityType = dbContext.Model.FindEntityType(type);
            string tableName = entityType.GetTableName();

            if (tableName.EndsWith("_new"))
                tableName = tableName[0..^4];

            return $"{entityType.GetSchema() ?? "dbo"}.{tableName}";
        }

        /// <summary>
        /// Get database column name by entity type and class property name
        /// </summary>
        public static string TryGetColumnName(this DbContext dbContext, Type type, string propertyName)
        {
            var entityType = dbContext.Model.FindEntityType(type);

            var property = entityType
                .GetProperties()
                .SingleOrDefault(p => p.Name == propertyName);

            return property?.GetPropertyColumnName();
        }
    }
}