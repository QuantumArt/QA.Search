using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using QA.Search.Generic.DAL.Services.Configuration;
using QA.Search.Generic.DAL.Extensions;
using System;
using System.Linq;

namespace QA.Search.Generic.DAL.Services.Extensions
{
    public static class DbContextExtension
    {
        public static string TryGetColumnName(this DbContext dbContext, Type type, string propertyName)
        {
            IEntityType entityType = dbContext.Model.FindEntityType(type);

            IProperty property = entityType
                .GetProperties()
                .SingleOrDefault(p => p.Name == propertyName);

            return property?.GetPropertyColumnName();
        }

        public static string GetFormatedTableName(this DbContext dbContext, Type type, ContextConfiguration contextConfiguration)
        {       
            IEntityType entityType = dbContext.Model.FindEntityType(type);
            string tableName = entityType.GetTableName();

            //if (tableName.EndsWith("_new"))
            //    tableName = tableName.Substring(0, tableName.Length - 4);

            return string.Format(contextConfiguration.FormatTableName, entityType.GetSchema() ?? contextConfiguration.DefaultSchemeName, tableName);            
        }
    }
}
