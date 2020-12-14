using HB.FullStack.Database.SQL;

namespace System.Linq.Expressions
{
    internal static class ExpressionExtensions
    {
        public static string ToStatement(this Expression expression, SQLExpressionVisitorContenxt context)
        {
            return SQLExpressionVisitor.Visit(expression, context).ToString();
        }
    }
}