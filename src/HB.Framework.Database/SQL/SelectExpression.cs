#nullable enable

using HB.Framework.Common.Entities;
using HB.Framework.Database.Engine;
using HB.Framework.Database.Entities;
using System;
using System.Linq.Expressions;
using System.Text;

namespace HB.Framework.Database.SQL
{
    public class SelectExpression<T> where T : Entity, new()
    {
        private readonly StringBuilder _statementBuilder = new StringBuilder();

        private bool _firstAssign = true;

        private readonly SQLExpressionVisitorContenxt _expressionContext;

        public bool WithSelectString { get; set; } = true;

        public override string ToString()
        {
            StringBuilder resultBuilder = WithSelectString ? new StringBuilder(" SELECT ") : new StringBuilder(" ");

            resultBuilder.Append(_statementBuilder);
            resultBuilder.Append(' ');

            return resultBuilder.ToString();
        }

        internal SelectExpression(IDatabaseEngine databaseEngine, IDatabaseEntityDefFactory entityDefFactory)
        {
            _expressionContext = new SQLExpressionVisitorContenxt(databaseEngine, entityDefFactory);
        }

        public SelectExpression<T> Select<TTarget>(Expression<Func<T, TTarget>> expr)
        {
            if (!_firstAssign)
            {
                _statementBuilder.Append(", ");
            }
            else
            {
                _firstAssign = false;
            }

            _statementBuilder.Append(expr.ToStatement(_expressionContext));

            return this;
        }
    }
}
