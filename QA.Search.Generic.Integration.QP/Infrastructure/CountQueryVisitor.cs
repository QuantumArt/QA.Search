using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QA.Search.Generic.DAL.Services.Configuration;
using QA.Search.Generic.DAL.Services.Extensions;
using QA.Search.Generic.Integration.QP.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace QA.Search.Generic.Integration.QP.Infrastructure
{
    /// <summary>
    /// Транслятор для SQL-запроса, который подсчитывает количество документов для индексации.
    /// Преобразует дерево выражений DSL методов-расширений запрос в `SELECT COUNT(*)`
    /// </summary>
    internal class CountQueryVisitor : QueryVisitor
    {
        public CountQueryVisitor(DbContext dbContext, IOptions<ContextConfiguration> contextConfigurationOption)
            : base(dbContext, contextConfigurationOption)
        {
        }

        /// <summary>
        /// Собрать SQL-запрос из дерева выражений DSL-методов расширений
        /// </summary>
        public override string Visit(IQueryable queryable)
        {
            Expression expression = queryable.Expression;

            List<MethodCallExpression> statements = UnrollStatements(expression);

            expression = statements.FirstOrDefault()?.Arguments.FirstOrDefault() ?? expression;

            IQueryable dbSet = GetQueriableExpression(expression, queryable);

            string tableName = _dbContext.GetFormatedTableName(dbSet.ElementType, _contextConfiguration);
            string tableAlias = $"{dbSet.ElementType.Name}_{_tableAliasCounter.Count++}";
            string modifiedColumn = ModifiedColumnName(dbSet.ElementType);

            _tableAliasStack.Push(tableAlias);

            var whereSql = new StringBuilder();

            whereSql.AppendLine($"WHERE {tableAlias}.[{modifiedColumn}] > @fromDate");

            foreach (var statement in statements)
            {
                if (statement.Method.Name == nameof(QueryableExtensions.Filter))
                {
                    VisitFilter(statement, whereSql);
                }
            }

            _tableAliasStack.Pop();

            var sb = new StringBuilder();
            sb.AppendLine($"SELECT COUNT (*)");

            if (_contextConfiguration.SqlServerType == SqlServerType.MSSQL)
                sb.AppendLine($"FROM {tableName} AS {tableAlias} WITH (NOLOCK)");

            if (_contextConfiguration.SqlServerType == SqlServerType.PostgreSQL)
                sb.AppendLine($"FROM {tableName} AS {tableAlias}");

            sb.Append(whereSql);

            if (_contextConfiguration.SqlServerType == SqlServerType.PostgreSQL)
            {
                sb.Replace("[", "");
                sb.Replace("]", "");
            }

            return sb.ToString();
        }
    }
}