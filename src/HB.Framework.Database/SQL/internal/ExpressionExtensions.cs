using HB.Framework.Database.SQL;
using System;
using System.Collections.Generic;
using System.Text;

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
