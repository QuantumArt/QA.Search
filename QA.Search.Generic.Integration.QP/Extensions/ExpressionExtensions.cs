using System;
using System.Linq.Expressions;
using System.Reflection;

namespace QA.Search.Generic.Integration.QP.Extensions
{
    internal static class ExpressionExtensions
    {
        public static Expression ExpandConversion(this Expression expression)
        {
            while (expression.NodeType == ExpressionType.Convert)
            {
                expression = ((UnaryExpression)expression).Operand;
            }
            return expression;
        }

        public static Expression ExpandQuote(this Expression expression)
        {
            while (expression.NodeType == ExpressionType.Quote)
            {
                expression = ((UnaryExpression)expression).Operand;
            }
            return expression;
        }

        /// <summary>
        /// Get computed value of Expression if possible or throw <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        public static object GetValue(this Expression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            switch (expression.NodeType)
            {
                case ExpressionType.Constant:
                    return ((ConstantExpression)expression).Value;

                case ExpressionType.MemberAccess:
                    var memberExpr = (MemberExpression)expression;
                    {
                        object instance = memberExpr.Expression.GetValue();
                        switch (memberExpr.Member)
                        {
                            case FieldInfo field:
                                return field.GetValue(instance);

                            case PropertyInfo property:
                                return property.GetValue(instance);
                        }
                    }
                    break;

                case ExpressionType.Convert:
                    var convertExpr = (UnaryExpression)expression;
                    {
                        if (convertExpr.Method == null)
                        {
                            Type type = Nullable.GetUnderlyingType(convertExpr.Type) ?? convertExpr.Type;
                            object value = convertExpr.Operand.GetValue();
                            return Convert.ChangeType(value, type);
                        }
                    }
                    break;

                case ExpressionType.ArrayIndex:
                    var indexExpr = (BinaryExpression)expression;
                    {
                        var array = (Array)indexExpr.Left.GetValue();
                        var index = (int)indexExpr.Right.GetValue();
                        return array.GetValue(index);
                    }

                case ExpressionType.ArrayLength:
                    var lengthExpr = (UnaryExpression)expression;
                    {
                        var array = (Array)lengthExpr.Operand.GetValue();
                        return array.Length;
                    }

                case ExpressionType.Call:
                    var callExpr = (MethodCallExpression)expression;
                    {
                        if (callExpr.Method.Name == "get_Item")
                        {
                            object instance = callExpr.Object.GetValue();
                            object[] arguments = new object[callExpr.Arguments.Count];
                            for (int i = 0; i < arguments.Length; i++)
                            {
                                arguments[i] = callExpr.Arguments[i].GetValue();
                            }
                            return callExpr.Method.Invoke(instance, arguments);
                        }
                    }
                    break;

                case ExpressionType.Quote:
                    var quoteExpression = (UnaryExpression)expression;
                    {
                        return GetValue(quoteExpression.Operand);
                    }

                case ExpressionType.Lambda:
                    return expression;
            }

            // we can't interpret the expression but we can compile and run it
            var objectMember = Expression.Convert(expression, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            return getterLambda.Compile().Invoke();
        }
    }
}