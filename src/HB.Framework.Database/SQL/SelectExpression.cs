using HB.Framework.Database.Engine;
using HB.Framework.Database.Entity;
using System;
using System.Linq.Expressions;
using System.Text;

namespace HB.Framework.Database.SQL
{
    public class SelectExpression<T> where T : DatabaseEntity, new()
    {
        private StringBuilder _statementBuilder = new StringBuilder();

        private bool _firstAssign = true;

        private readonly IDatabaseEngine _databaseEngine;
        private SQLExpressionVisitorContenxt expressionContext = null;

        public bool WithSelectString { get; set; } = true;

        public override string ToString()
        {
            StringBuilder resultBuilder = WithSelectString ? new StringBuilder(" SELECT ") : new StringBuilder(" ");

            resultBuilder.Append(_statementBuilder);
            resultBuilder.Append(" ");

            return resultBuilder.ToString();
        }

        internal SelectExpression(IDatabaseEngine databaseEngine, IDatabaseEntityDefFactory entityDefFactory)
        {
            _databaseEngine = databaseEngine;
            expressionContext = new SQLExpressionVisitorContenxt(databaseEngine, entityDefFactory);
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

            _statementBuilder.Append(expr.ToStatement(expressionContext));

            return this;
        }
    }
}
