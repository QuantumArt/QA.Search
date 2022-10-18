using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;
using QA.Search.Generic.DAL.Extensions;
using QA.Search.Generic.DAL.Services.Configuration;
using QA.Search.Generic.DAL.Services.Extensions;
using QA.Search.Generic.DAL.Services.Interfaces;
using QA.Search.Generic.Integration.QP.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace QA.Search.Generic.Integration.QP.Infrastructure
{
    /// <summary>
    /// Транслятор для SQL-запроса, который загружает партию JSON-документов.
    /// Преобразует дерево выражений DSL методов-расширений в синтаксис
    /// `FOR JSON PATH` для SQL Server 2016+.
    /// </summary>
    internal class LoadQueryVisitor : QueryVisitor
    {
        private readonly Stack<string> _propertyNameStack = new Stack<string>();
        private readonly StringBuilder _selectSql = new StringBuilder();
        private readonly StringBuilder _fromSql = new StringBuilder();
        private readonly StringBuilder _joinSql = new StringBuilder();
        private readonly StringBuilder _whereSql = new StringBuilder();
        private readonly StringBuilder _orderBySql = new StringBuilder();
        private readonly List<PropertyInfo> _pickProps = new List<PropertyInfo>();
        private readonly List<PropertyInfo> _omitProps = new List<PropertyInfo>();

        public LoadQueryVisitor(DbContext dbContext, IOptions<ContextConfiguration> contextConfigurationOption)
            : base(dbContext, contextConfigurationOption)
        {
        }

        /// <summary>
        /// Stateful Visitor для преобразования .JoinMany() в SQL-подзапрос `FOR JSON PATH`.
        /// </summary>
        private LoadQueryVisitor(QueryVisitor parent)
            : base(parent)
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
            string primaryColumn = PrimaryColumnName(dbSet.ElementType);
            string modifiedColumn = ModifiedColumnName(dbSet.ElementType);
            string tableAlias = $"{dbSet.ElementType.Name}_{_tableAliasCounter.Count++}";

            _tableAliasStack.Push(tableAlias);

            if (_contextConfiguration.SqlServerType == SqlServerType.PostgreSQL)
            {
                _selectSql.AppendLine("SELECT array_to_json(array_agg(row_to_json(subresult)))");
                _selectSql.AppendLine("FROM (");
                _selectSql.AppendLine("SELECT");
            }

            if (_contextConfiguration.SqlServerType == SqlServerType.MSSQL)
                _selectSql.AppendLine("SELECT TOP (@batchSize)");

            foreach (IProperty property in ScalarProperties(dbSet.ElementType))
            {
                string columnName = property.GetPropertyColumnName();
                Type propertyType = property.PropertyInfo.PropertyType;
                if ((propertyType == typeof(bool) || propertyType == typeof(bool?)) && _contextConfiguration.SqlServerType == SqlServerType.MSSQL)
                {
                    _selectSql.AppendLine($"  CAST({tableAlias}.[{columnName}] AS bit) AS [{property.Name}],");
                }
                else
                {
                    _selectSql.AppendLine($"  {tableAlias}.[{columnName}] AS [{property.Name}],");
                }
            }

            if (_contextConfiguration.SqlServerType == SqlServerType.MSSQL)
                _fromSql.AppendLine($"FROM {tableName} AS {tableAlias} WITH(NOLOCK)");
            else
                _fromSql.AppendLine($"FROM {tableName} AS {tableAlias}");

            _whereSql.AppendLine($"WHERE {tableAlias}.[{primaryColumn}] > @fromId");
            _whereSql.AppendLine($"  AND {tableAlias}.[{modifiedColumn}] > @fromDate");

            _orderBySql.AppendLine($"ORDER BY {tableAlias}.[{primaryColumn}] ASC");

            VisitStatements(statements, _whereSql);

            _selectSql.RemoveLastSeparator(',');

            _tableAliasStack.Pop();

            var sb = new StringBuilder();
            sb.Append(_selectSql);
            sb.Append(_fromSql);
            sb.Append(_joinSql);
            sb.Append(_whereSql);
            sb.Append(_orderBySql);

            if (_contextConfiguration.SqlServerType == SqlServerType.MSSQL)
                sb.Append("FOR JSON PATH");

            if (_contextConfiguration.SqlServerType == SqlServerType.PostgreSQL)
            {
                sb.AppendLine("LIMIT (@batchSize)");
                sb.AppendLine(") AS subresult");

                sb.Replace("[", "");
                sb.Replace("]", "");
            }

            return sb.ToString();
        }

        private void VisitStatements(List<MethodCallExpression> statements, StringBuilder filterSql)
        {
            foreach (var statement in statements)
            {
                if (statement.Method.Name == nameof(QueryableExtensions.Filter))
                {
                    VisitFilter(statement, filterSql);
                }
                else if (statement.Method.Name == nameof(QueryableExtensions.JoinOne))
                {
                    VisitJoinOne(statement);
                }
                else if (statement.Method.Name == nameof(QueryableExtensions.JoinMany))
                {
                    if (statement.Arguments.Count == 2)
                    {
                        _selectSql.Append(_tabulation);
                        _selectSql.Append("  (");
                        _selectSql.Append(new LoadQueryVisitor(this).VisitJoinMany(statement, out string property));
                        string path = String.Join(".", _propertyNameStack.Reverse().Concat(new[] { property }));
                        _selectSql.AppendLine($") AS [{path}],");
                    }
                    else if (statement.Arguments.Count == 3)
                    {
                        _selectSql.Append(_tabulation);
                        _selectSql.Append("  (");
                        _selectSql.Append(new LoadQueryVisitor(this).VisitJoinManyToMany(statement, out string property));
                        string path = String.Join(".", _propertyNameStack.Reverse().Concat(new[] { property }));
                        _selectSql.AppendLine($") AS [{path}],");
                    }
                }
            }
        }

        private void VisitPick(MethodCallExpression pick)
        {
            var selector = pick.Arguments[1].ExpandQuote() as LambdaExpression;

            if (selector.Body is NewExpression constructor &&
                constructor.Members.Count == constructor.Arguments.Count &&
                constructor.Arguments.All(argument => argument is MemberExpression))
            {
                for (int i = 0; i < constructor.Members.Count; i++)
                {
                    MemberInfo member = constructor.Members[i];
                    var argument = (MemberExpression)constructor.Arguments[i];
                    var propertyInfo = (PropertyInfo)argument.Member;

                    if (member.Name != propertyInfo.Name)
                    {
                        throw new NotSupportedException("Properties can't be renamed");
                    }

                    _pickProps.Add(propertyInfo);
                }
            }
            else if (selector.Body is MemberExpression memberExpr &&
                memberExpr.Member is PropertyInfo propertyInfo)
            {
                _pickProps.Add(propertyInfo);
            }
            else
            {
                throw new NotSupportedException("Suppuorts only property access and anonymous objects");
            }
        }

        private void VisitOmit(MethodCallExpression pick)
        {
            var selector = pick.Arguments[1].ExpandQuote() as LambdaExpression;

            if (selector.Body is NewExpression constructor &&
                constructor.Members.Count == constructor.Arguments.Count &&
                constructor.Arguments.All(argument => argument is MemberExpression))
            {
                for (int i = 0; i < constructor.Members.Count; i++)
                {
                    MemberInfo member = constructor.Members[i];
                    var argument = (MemberExpression)constructor.Arguments[i];
                    var propertyInfo = (PropertyInfo)argument.Member;

                    if (member.Name != propertyInfo.Name)
                    {
                        throw new NotSupportedException("Properties can't be renamed");
                    }

                    _omitProps.Add(propertyInfo);
                }
            }
            else if (selector.Body is MemberExpression memberExpr &&
                memberExpr.Member is PropertyInfo propertyInfo)
            {
                _omitProps.Add(propertyInfo);
            }
            else
            {
                throw new NotSupportedException("Suppuorts only property access and anonymous objects");
            }
        }

        private void VisitJoinOne(MethodCallExpression joinOne)
        {
            var selector = joinOne.Arguments[1].ExpandQuote() as LambdaExpression;

            List<MethodCallExpression> statements = UnrollStatements(selector.Body);

            var propertyExpr = statements.FirstOrDefault()?.Arguments.First() ?? selector.Body;
            var propertyInfo = (PropertyInfo)((MemberExpression)propertyExpr).Member;
            string tableName = _dbContext.GetFormatedTableName(propertyInfo.PropertyType, _contextConfiguration);
            string tableAlias = $"{propertyInfo.PropertyType.Name}_{_tableAliasCounter.Count++}";

            string outerTableAlias = _tableAliasStack.Peek();

            _tableAliasStack.Push(tableAlias);
            _propertyNameStack.Push(propertyInfo.Name);

            string path = String.Join(".", _propertyNameStack.Reverse());

            foreach (IProperty property in ScalarProperties(propertyInfo.PropertyType))
            {
                string columnName = property.GetPropertyColumnName();
                Type propertyType = property.PropertyInfo.PropertyType;
                if (propertyType == typeof(bool) || propertyType == typeof(bool?))
                {
                    _selectSql.AppendLine($"  CAST({tableAlias}.[{columnName}] AS bit) AS [{path}.{property.Name}],");
                }
                else
                {
                    _selectSql.AppendLine($"  {tableAlias}.[{columnName}] AS [{path}.{property.Name}],");
                }
            }

            var entityType = _dbContext.Model.FindEntityType(propertyInfo.ReflectedType);
            var navigation = entityType.GetNavigations().Single(nav => nav.PropertyInfo == propertyInfo);
            var outerKey = navigation.ForeignKey.Properties.Single().GetPropertyColumnName();
            var innerKey = navigation.ForeignKey.PrincipalKey.Properties.Single().GetPropertyColumnName();

            _joinSql.AppendLine($"LEFT JOIN {tableName} AS {tableAlias} WITH (NOLOCK)");
            _joinSql.AppendLine($"  ON {outerTableAlias}.[{outerKey}] = {tableAlias}.[{innerKey}]");

            VisitStatements(statements, _joinSql);

            _propertyNameStack.Pop();
            _tableAliasStack.Pop();
        }

        /// <param name="propertyName">
        /// Имя Navigation Property куда будет сохранен массив JSON-объектов,
        /// являющийся результатом SQL-подзапроса `FOR JSON PATH`
        /// </param>
        private string VisitJoinMany(MethodCallExpression joinMany, out string propertyName)
        {
            var selector = joinMany.Arguments[1].ExpandQuote() as LambdaExpression;

            List<MethodCallExpression> statements = UnrollStatements(selector.Body);

            var propertyExpr = statements.FirstOrDefault()?.Arguments.First() ?? selector.Body;
            var propertyInfo = (PropertyInfo)((MemberExpression)propertyExpr).Member;
            Type itemType = propertyInfo.PropertyType.GetGenericArguments()[0];
            string itemTableName = _dbContext.GetFormatedTableName(itemType, _contextConfiguration);
            string itemTableAlias = $"{itemType.Name}_{_tableAliasCounter.Count++}";

            string outerTableAlias = _tableAliasStack.Peek();

            _tableAliasStack.Push(itemTableAlias);

            _selectSql.AppendLine("SELECT");

            foreach (IProperty property in ScalarProperties(itemType))
            {
                string columnName = property.GetPropertyColumnName();
                Type propertyType = property.PropertyInfo.PropertyType;
                if (propertyType == typeof(bool) || propertyType == typeof(bool?))
                {
                    _selectSql.AppendLine($"  CAST({itemTableAlias}.[{columnName}] AS bit) AS [{property.Name}],");
                }
                else
                {
                    _selectSql.AppendLine($"  {itemTableAlias}.[{columnName}] AS [{property.Name}],");
                }
            }

            var entityType = _dbContext.Model.FindEntityType(propertyInfo.ReflectedType);
            var navigation = entityType.GetNavigations().Single(nav => nav.PropertyInfo == propertyInfo);
            var outerKey = navigation.ForeignKey.PrincipalKey.Properties.Single().GetPropertyColumnName();
            var itemKey = navigation.ForeignKey.Properties.Single().GetPropertyColumnName();

            _fromSql.AppendLine($"FROM {itemTableName} AS {itemTableAlias} WITH (NOLOCK)");
            _whereSql.AppendLine($"WHERE {itemTableAlias}.[{itemKey}] = {outerTableAlias}.[{outerKey}]");

            VisitStatements(statements, _whereSql);

            _selectSql.RemoveLastSeparator(',');

            _tableAliasStack.Pop();

            propertyName = propertyInfo.Name;
            var sb = new StringBuilder();
            sb.Append(_selectSql);
            sb.Append(_fromSql);
            sb.Append(_joinSql);
            sb.Append(_whereSql);
            sb.Append("FOR JSON PATH");
            return sb.ToString().Replace(Environment.NewLine, Environment.NewLine + _tabulation);
        }

        /// <param name="propertyName">
        /// Имя Navigation Property куда будет сохранен массив JSON-объектов,
        /// являющийся результатом SQL-подзапроса `FOR JSON PATH`
        /// </param>
        private string VisitJoinManyToMany(MethodCallExpression joinMany, out string propertyName)
        {
            var linkSelector = joinMany.Arguments[1].ExpandQuote() as LambdaExpression;
            var linkPropertyInfo = (PropertyInfo)((MemberExpression)linkSelector.Body).Member;
            Type linkType = linkPropertyInfo.PropertyType.GetGenericArguments()[0];
            string linkTableName = _dbContext.GetFormatedTableName(linkType, _contextConfiguration);
            string linkTableAlias = $"{linkType.Name}_{_tableAliasCounter.Count++}";

            var itemSelector = joinMany.Arguments[2].ExpandQuote() as LambdaExpression;

            List<MethodCallExpression> statements = UnrollStatements(itemSelector.Body);

            var itemPropertyExpr = statements.FirstOrDefault()?.Arguments.First() ?? itemSelector.Body;
            var itemPropertyInfo = (PropertyInfo)((MemberExpression)itemPropertyExpr).Member;
            Type itemType = itemPropertyInfo.PropertyType;
            string itemTableName = _dbContext.GetFormatedTableName(itemType, _contextConfiguration);
            string itemTableAlias = $"{itemType.Name}_{_tableAliasCounter.Count++}";

            string outerTableAlias = _tableAliasStack.Peek();

            _tableAliasStack.Push(linkTableAlias);
            _tableAliasStack.Push(itemTableAlias);

            _selectSql.AppendLine("SELECT");

            foreach (IProperty property in ScalarProperties(itemType))
            {
                string columnName = property.GetPropertyColumnName();
                Type propertyType = property.PropertyInfo.PropertyType;
                if (propertyType == typeof(bool) || propertyType == typeof(bool?))
                {
                    _selectSql.AppendLine($"  CAST({itemTableAlias}.[{columnName}] AS bit) AS [{property.Name}],");
                }
                else
                {
                    _selectSql.AppendLine($"  {itemTableAlias}.[{columnName}] AS [{property.Name}],");
                }
            }

            var linkEntityType = _dbContext.Model.FindEntityType(linkPropertyInfo.ReflectedType);
            var linkNavigation = linkEntityType.GetNavigations().Single(nav => nav.PropertyInfo == linkPropertyInfo);
            var outerTableKey = linkNavigation.ForeignKey.PrincipalKey.Properties.Single().GetPropertyColumnName();
            var linkLeftKey = linkNavigation.ForeignKey.Properties.Single().GetPropertyColumnName();

            _fromSql.AppendLine($"FROM {linkTableName} AS {linkTableAlias} WITH (NOLOCK)");
            _whereSql.AppendLine($"WHERE {linkTableAlias}.[{linkLeftKey}] = {outerTableAlias}.[{outerTableKey}]");

            var itemEntityType = _dbContext.Model.FindEntityType(itemPropertyInfo.ReflectedType);
            var itemNavigation = itemEntityType.GetNavigations().Single(nav => nav.PropertyInfo == itemPropertyInfo);
            var linkRightKey = itemNavigation.ForeignKey.Properties.Single().GetPropertyColumnName();
            var itemTableKey = itemNavigation.ForeignKey.PrincipalKey.Properties.Single().GetPropertyColumnName();

            _joinSql.AppendLine($"JOIN {itemTableName} AS {itemTableAlias} WITH (NOLOCK)");
            _joinSql.AppendLine($"  ON {linkTableAlias}.[{linkRightKey}] = {itemTableAlias}.[{itemTableKey}]");

            VisitStatements(statements, _whereSql);

            _selectSql.RemoveLastSeparator(',');

            _tableAliasStack.Pop();
            _tableAliasStack.Pop();

            propertyName = linkPropertyInfo.Name;
            var sb = new StringBuilder();
            sb.Append(_selectSql);
            sb.Append(_fromSql);
            sb.Append(_joinSql);
            sb.Append(_whereSql);
            sb.Append("FOR JSON PATH");
            return sb.ToString().Replace(Environment.NewLine, Environment.NewLine + _tabulation);
        }

        #region Utils

        /// <summary>
        /// Получить цепочку вызовов методов в виде списка, от верхнего метода к нижнему.
        /// Обработать вызовы .Peek() и .Omit()
        /// </summary>
        protected override List<MethodCallExpression> UnrollStatements(Expression expression)
        {
            var statements = base.UnrollStatements(expression);

            foreach (var statement in statements)
            {
                if (statement.Method.Name == nameof(QueryableExtensions.Pick))
                {
                    VisitPick(statement);
                }
                else if (statement.Method.Name == nameof(QueryableExtensions.Omit))
                {
                    VisitOmit(statement);
                }
            }

            return statements;
        }

        /// <summary>
        /// Имена общих служебных свойств каждого контента QP: Archived, Modified, etc.
        /// </summary>
        protected static readonly string[] ArticlePropertyNames = typeof(IGenericItem)
            .GetProperties()
            .Select(p => p.Name)
            .Except(new[] { nameof(IGenericItem.ContentItemID) })
            .ToArray();

        /// <summary>
        /// Получить набор простых полей, выбранных для индексации.
        /// По-умолчанию выбраны все поля, кроме служебных полей статьи QP: Archived, Modified, etc.
        /// Если присутствует вызов .Peek() — будут выбраны только указанные поля.
        /// Если присутствует вызов .Omit() — будут выбраны все поля, кроме указанных.
        /// и кроме служебных полей статьи QP.
        /// </summary>
        protected IProperty[] ScalarProperties(Type type)
        {
            var entityType = _dbContext.Model.FindEntityType(type);

            string[] pickPropNames = _pickProps
                .Where(prop => prop.ReflectedType == type)
                .Select(prop => prop.Name)
                .ToArray();

            string[] omitPropNames = _omitProps
                .Where(prop => prop.ReflectedType == type)
                .Select(prop => prop.Name)
                .ToArray();

            var scalarProps = entityType
                .GetProperties()
                .Except(entityType.GetForeignKeys().SelectMany(fk => fk.Properties));

            if (pickPropNames.Length > 0)
            {
                return scalarProps
                    .Where(prop => pickPropNames.Contains(prop.Name) && !omitPropNames.Contains(prop.Name))
                    .ToArray();
            }

            omitPropNames = omitPropNames.Concat(ArticlePropertyNames).ToArray();

            return scalarProps
                .Where(p => !omitPropNames.Contains(p.Name))
                .ToArray();
        }

        #endregion
    }
}