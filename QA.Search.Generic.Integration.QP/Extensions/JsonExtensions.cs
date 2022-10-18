using Newtonsoft.Json.Linq;
using QA.Search.Generic.DAL.Exceptions;
using QA.Search.Generic.DAL.Services.Configuration;
using QA.Search.Generic.DAL.Services.Interfaces;

namespace QA.Search.Generic.Integration.QP.Extensions
{
    internal static class JsonExtensions
    {
        public static int GetArticleId(this JObject document, ContextConfiguration contextConfiguration)
        {
            string propertyName = nameof(IGenericItem.ContentItemID);

            switch (contextConfiguration.SqlServerType)
            {
                case SqlServerType.MSSQL:
                    return document[propertyName].Value<int>();
                case SqlServerType.PostgreSQL:
                    propertyName = propertyName.ToLower();

                    return (int)document[propertyName].Value<long>();
                default:
                    throw new NotSupportedSqlServerTypeException(contextConfiguration.SqlServerType);
            }
        }
    }
}