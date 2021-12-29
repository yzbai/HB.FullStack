#nullable enable

using System.Linq.Expressions;

namespace HB.FullStack.Database.SQL
{
    internal interface ISQLExpressionVisitor
    {
        object Visit(Expression? exp, SQLExpressionVisitorContenxt context);
    }
}