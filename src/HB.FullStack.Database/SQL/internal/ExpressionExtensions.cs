using HB.FullStack.Database.SQL;

namespace System.Linq.Expressions
{
    internal static class ExpressionExtensions
    {
        /// <summary>
        /// ToStatement
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="System.DatabaseException"></exception>
        public static string ToStatement(this Expression expression, SQLExpressionVisitorContenxt context)
        {
            return SQLExpressionVisitor.Visit(expression, context).ToString()!;
        }
    }
}