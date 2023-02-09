using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Options;
using QA.Search.Generic.DAL.Extensions;
using QA.Search.Generic.DAL.Services.Configuration;
using QA.Search.Generic.DAL.Services.Interfaces;
using QA.Search.Generic.Integration.QP.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace QA.Search.Generic.Integration.QP.Infrastructure
{
    /// <summary>
    /// Базовый транслятор для SQL-запроса. Преобразует дерево выражений DSL
    /// методов-расширений в синтаксис `FOR JSON PATH` для SQL Server 2016+.
    /// </summary>
    internal abstract class QueryVisitor
    {
        protected class TableAliasCounter
        {
            public int Count = 0;
        }

        protected readonly DbContext _dbContext;
        protected readonly string _tabulation;
        protected readonly TableAliasCounter _tableAliasCounter;
        protected readonly Stack<string> _tableAliasStack;
        protected readonly ContextConfiguration _contextConfiguration;

        protected QueryVisitor(DbContext dbContext, IOptions<ContextConfiguration> contextConfigurationOption)
        {
            _dbContext = dbContext;
            _tabulation = "";
            _tableAliasCounter = new TableAliasCounter();
            _tableAliasStack = new Stack<string>();
            _contextConfiguration = contextConfigurationOption.Value;
        }

        /// <summary>
        /// Stateful Visitor для преобразования .JoinMany() в SQL-подзапрос `FOR JSON PATH`.
        /// </summary>
        protected QueryVisitor(QueryVisitor parent)
        {
            _dbContext = parent._dbContext;
            _tabulation = parent._tabulation + "  ";
            _tableAliasCounter = parent._tableAliasCounter;
            _tableAliasStack = parent._tableAliasStack;
        }

        public abstract string Visit(IQueryable queryable);

        #region Filter

        protected void VisitFilter(MethodCallExpression filter, StringBuilder filterSql)
        {
            var conditionLambda = filter.Arguments[1].ExpandQuote() as LambdaExpression;

            filterSql.Append(filterSql.Length == 0 ? "WHERE " : "  AND ");

            VisitCondition(conditionLambda.Body.ExpandConversion(), filterSql);
        }

        private void VisitCondition(Expression condition, StringBuilder filterSql)
        {
            if (condition is MemberExpression member
                && (member.Type == typeof(bool) || member.Type == typeof(bool?)))
            {
                VisitEquality(Expression.Equal(member, Expression.Constant(true, member.Type)), filterSql);
            }
            else if (condition is UnaryExpression unary && unary.NodeType == ExpressionType.Not)
            {
                if (unary.Operand is MemberExpression notMember
                    && (notMember.Type == typeof(bool) || notMember.Type == typeof(bool?)))
                {
                    VisitEquality(Expression.Equal(notMember, Expression.Constant(false, notMember.Type)), filterSql);
                }
                else
                {
                    filterSql.Append("NOT (");
                    VisitCondition(unary.Operand, filterSql);
                    filterSql.InsertBeforeTrailingLineBreak(")");
                }
            }
            else if (condition is BinaryExpression binary)
            {
                switch (binary.NodeType)
                {
                    case ExpressionType.AndAlso:
                        filterSql.Append('(');
                        VisitCondition(binary.Left, filterSql);
                        filterSql.Append("  AND ");
                        VisitCondition(binary.Right, filterSql);
                        filterSql.InsertBeforeTrailingLineBreak(")");
                        break;

                    case ExpressionType.OrElse:
                        filterSql.Append('(');
                        VisitCondition(binary.Left, filterSql);
                        filterSql.Append("   OR ");
                        VisitCondition(binary.Right, filterSql);
                        filterSql.InsertBeforeTrailingLineBreak(")");
                        break;

                    case ExpressionType.Equal:
                        VisitEquality(binary, filterSql);
                        break;

                    case ExpressionType.NotEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.LessThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThanOrEqual:
                        VisitComparsion(binary, filterSql);
                        break;

                    default:
                        throw new NotSupportedException($"Condition {condition.NodeType} is not supported");
                }
            }
            else if (condition is MethodCallExpression contains &&
                IsLinqMethod(contains.Method) &&
                contains.Method.Name == "Contains")
            {
                VisitContains(contains, filterSql);
            }
            else
            {
                throw new NotSupportedException($"Condition {condition.NodeType} is not supported");
            }
        }

        private void VisitEquality(BinaryExpression equality, StringBuilder filterSql)
        {
            string leftColumn = TryGetTableColumn(equality.Left);
            string rightColumn = TryGetTableColumn(equality.Right);
            if (leftColumn != null)
            {
                if (rightColumn != null)
                {
                    filterSql.AppendLine($"{leftColumn} = {rightColumn}");
                }
                else
                {
                    object value = equality.Right.GetValue();

                    filterSql.AppendLine(value == null
                        ? $"{leftColumn} IS NULL"
                        : $"{leftColumn} = {EscapeValue(value)}");
                }
            }
            else if (rightColumn != null)
            {
                object value = equality.Left.GetValue();

                filterSql.AppendLine(value == null
                    ? $"{rightColumn} IS NULL"
                    : $"{EscapeValue(value)} = {rightColumn}");
            }
            else
            {
                throw new NotSupportedException("Equality without table columns is not supported");
            }
        }

        private void VisitComparsion(BinaryExpression comparsion, StringBuilder filterSql)
        {
            string op;
            switch (comparsion.NodeType)
            {
                case ExpressionType.NotEqual:
                    op = "<>";
                    break;
                case ExpressionType.LessThan:
                    op = "<";
                    break;
                case ExpressionType.GreaterThan:
                    op = ">";
                    break;
                case ExpressionType.LessThanOrEqual:
                    op = "<=";
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    op = ">=";
                    break;
                default:
                    throw new InvalidOperationException();
            }

            string leftColumn = TryGetTableColumn(comparsion.Left);
            string rightColumn = TryGetTableColumn(comparsion.Right);
            if (leftColumn != null)
            {
                if (rightColumn != null)
                {
                    filterSql.AppendLine($"{leftColumn} {op} {rightColumn}");
                }
                else
                {
                    object value = comparsion.Right.GetValue();

                    filterSql.AppendLine(value == null && comparsion.NodeType == ExpressionType.NotEqual
                        ? $"{leftColumn} IS NOT NULL"
                        : $"{leftColumn} {op} {EscapeValue(value)}");
                }
            }
            else if (rightColumn != null)
            {
                object value = comparsion.Left.GetValue();

                filterSql.AppendLine(value == null && comparsion.NodeType == ExpressionType.NotEqual
                    ? $"{rightColumn} IS NOT NULL"
                    : $"{EscapeValue(value)} {op} {rightColumn}");
            }
            else
            {
                throw new NotSupportedException("Comparsion without table columns is not supported");
            }
        }

        private void VisitContains(MethodCallExpression contains, StringBuilder filterSql)
        {
            string tableAliasColumn = TryGetTableColumn(contains.Arguments[1]);

            if (tableAliasColumn == null)
            {
                throw new NotSupportedException(".Contains() argument should be a table column");
            }

            filterSql.Append(tableAliasColumn);
            filterSql.Append(" IN (");
            foreach (object value in (IEnumerable)contains.Arguments[0].GetValue())
            {
                filterSql.Append(EscapeValue(value)); filterSql.Append(",");
            }
            filterSql.RemoveLastSeparator(',');
            filterSql.AppendLine(")");
        }

        #endregion

        #region Utils

        protected static readonly string NotSupportedStatementError = $"Supports only 'JoinOne', 'JoinMany', 'Pick', 'Omit' and 'Filter'";

        /// <summary>
        /// Получить цепочку вызовов методов в виде списка, от верхнего метода к нижнему.
        /// </summary>
        protected virtual List<MethodCallExpression> UnrollStatements(Expression expression)
        {
            var stack = new List<MethodCallExpression>();
            while (expression is MethodCallExpression statement)
            {
                if (!IsValidMethod(statement.Method))
                {
                    throw new NotSupportedException(NotSupportedStatementError);
                }
                stack.Add(statement);
                expression = statement.Arguments[0];
            }
            stack.Reverse();
            return stack;
        }

        protected static bool IsValidMethod(MethodInfo method)
        {
            return method.DeclaringType == typeof(QueryableExtensions) &&
                method.IsDefined(typeof(ExtensionAttribute), true);
        }

        protected static bool IsLinqMethod(MethodInfo method)
        {
            return (method.DeclaringType == typeof(Queryable) ||
                method.DeclaringType == typeof(Enumerable)) &&
                method.IsDefined(typeof(ExtensionAttribute), true);
        }

        /// <summary>
        /// Имя столбца первичного ключа
        /// </summary>
        protected string PrimaryColumnName(Type type)
        {
            var primaryKey = _dbContext.Model.FindEntityType(type).FindPrimaryKey();
            return primaryKey.Properties.Single().GetPropertyColumnName();
        }

        /// <summary>
        /// Имя столбца MODIFIED
        /// </summary>
        protected string ModifiedColumnName(Type type)
        {
            return _dbContext.TryGetColumnName(type, nameof(IGenericItem.Modified));
        }

        /// <summary>
        /// Получить имя столбца в виде tabeAlias.[ColumnName] из левой или правой части
        /// бинарного выражения, если она является обращением к свойству .NET-объекта
        /// </summary>
        protected string TryGetTableColumn(Expression expression)
        {
            expression = expression.ExpandConversion();
            if (expression is MemberExpression column)
            {
                string columnName = TryGetColumnName(column.Member);

                if (columnName == null)
                {
                    return null;
                }

                string tableAlias = _tableAliasStack.Peek();

                return $"{tableAlias}.{columnName}";
            }
            return null;
        }

        /// <summary>
        /// Получить имя слотбца в БД по имени свойства .NET-объекта в виде [ColumnName]
        /// </summary>
        protected string TryGetColumnName(MemberInfo memberInfo)
        {
            var entityType = _dbContext.Model.FindEntityType(memberInfo.DeclaringType);
            if (entityType != null)
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.Name == memberInfo.Name)
                    {
                        return $"[{property.GetPropertyColumnName()}]";
                    }
                };
            }
            return null;
        }

        /// <summary>
        /// Определяет что за тип Expression-а и в зависимости от этого возвращает IQueriable объект для дальнейшей работы с ним.
        /// Не уверен что ConstantExpression вообще будет использоваться как-то, но для внезапной совместимости оставил.
        /// </summary>
        /// <param name="expression">Выражение для определения</param>
        /// <param name="query">Оригинальный запрос откуда выражение было достато</param>
        /// <returns>IQueriable объект для дальнейшего составление ручного запроса на его базе</returns>
        /// <exception cref="NotSupportedException"></exception>
        protected static IQueryable GetQueriableExpression(Expression expression, IQueryable query)
        {
            if (expression is ConstantExpression table && table.Value is IQueryable tableQuery)
            {
                return tableQuery;
            }
            else if (expression is QueryRootExpression)
            {
                return query;
            }
            else
            {
                throw new NotSupportedException(NotSupportedStatementError);
            }
        }

        /// <summary>
        /// Экранировать (сериализовать) скалярное значение в QLS
        /// </summary>
        protected static string EscapeValue(object value)
        {
            if (value == null)
            {
                return "NULL";
            }
            if (value is bool bit)
            {
                return bit ? "1" : "0";
            }
            if (value.IsNumericType())
            {
                return value.ToString();
            }
            if (value is string str)
            {
                return $"N'{str.Replace("'", "''")}'";
            }
            if (value is DateTime date)
            {
                return $"'{date.ToString("yyyy-MM-dd hh:mm:ss.fff")}'";
            }
            throw new NotSupportedException($"Data type {value?.GetType()} is not supported");
        }

        #endregion
    }
}
