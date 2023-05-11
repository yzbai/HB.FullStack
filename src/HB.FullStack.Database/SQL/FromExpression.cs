

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database.SQL
{
    public class FromExpression<T> where T : BaseDbModel, new()
    {
        private readonly StringBuilder _statementBuilder = new StringBuilder();
        private readonly DbModelDef _tModelDef;
        private readonly SQLExpressionVisitorContenxt _expressionContext;
        private readonly IDbModelDefFactory _modelDefFactory;
        private readonly ISQLExpressionVisitor _expressionVisitor;

        public SqlJoinType? JoinType { get; set; }

        public IList<KeyValuePair<string, object>> GetParameters()
        {
            return _expressionContext.GetParameters();
        }

        public string ToStatement()
        {
            return $" FROM {_tModelDef.DbTableReservedName} {_statementBuilder}";
        }

        internal FromExpression(IDbModelDefFactory modelDefFactory, ISQLExpressionVisitor expressionVisitor)
        {
            _tModelDef = modelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            _expressionContext = new SQLExpressionVisitorContenxt(_tModelDef.EngineType)
            {
                ParamPlaceHolderPrefix = SqlHelper.PARAMETERIZED_CHAR + "f__"
            };

            _modelDefFactory = modelDefFactory;
            _expressionVisitor = expressionVisitor;
        }

        public FromExpression<T> InnerJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : BaseDbModel, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.INNER)
            {
                throw DbExceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.INNER;

            return InternalJoin<TTarget>("INNER JOIN", joinExpr);
        }

        public FromExpression<T> InnerJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinExpr)
            where TLeft : BaseDbModel, new()
            where TRight : BaseDbModel, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.INNER)
            {
                throw DbExceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.INNER;

            return InternalJoin<TRight>("INNER JOIN", joinExpr);
        }

        public FromExpression<T> LeftJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : BaseDbModel, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.LEFT)
            {
                throw DbExceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.LEFT;

            return InternalJoin<TTarget>("LEFT JOIN", joinExpr);
        }

        public FromExpression<T> LeftJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinExpr)
            where TLeft : BaseDbModel, new()
            where TRight : BaseDbModel, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.LEFT)
            {
                throw DbExceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.LEFT;

            return InternalJoin<TRight>("LEFT JOIN", joinExpr);
        }

        public FromExpression<T> RightJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : BaseDbModel, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.RIGHT)
            {
                throw DbExceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.RIGHT;

            return InternalJoin<TTarget>("RIGHT JOIN", joinExpr);
        }

        public FromExpression<T> RightJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinExpr)
            where TLeft : BaseDbModel, new()
            where TRight : BaseDbModel, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.RIGHT)
            {
                throw DbExceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.RIGHT;

            return InternalJoin<TRight>("RIGHT JOIN", joinExpr);
        }

        public FromExpression<T> FullJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : BaseDbModel, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.FULL)
            {
                throw DbExceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.FULL;

            return InternalJoin<TTarget>("FULL JOIN", joinExpr);
        }

        public FromExpression<T> FullJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinExpr)
            where TLeft : BaseDbModel, new()
            where TRight : BaseDbModel, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.FULL)
            {
                throw DbExceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.FULL;

            return InternalJoin<TRight>("FULL JOIN", joinExpr);
        }

        public FromExpression<T> CrossJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : BaseDbModel, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.CROSS)
            {
                throw DbExceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.CROSS;

            return InternalJoin<TTarget>("CROSS JOIN", joinExpr);
        }

        public FromExpression<T> CrossJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinExpr)
            where TLeft : BaseDbModel, new()
            where TRight : BaseDbModel, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.CROSS)
            {
                throw DbExceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.CROSS;

            return InternalJoin<TRight>("CROSS JOIN", joinExpr);
        }

        private FromExpression<T> InternalJoin<Target>(string joinType, Expression joinExpr) where Target : BaseDbModel
        {
            DbModelDef targetDef = _modelDefFactory.GetDef<Target>()!;

            _statementBuilder.Append(' ');
            _statementBuilder.Append(joinType);
            _statementBuilder.Append(' ');
            _statementBuilder.Append(targetDef.DbTableReservedName);
            _statementBuilder.Append(" ON ");
            _statementBuilder.Append(_expressionVisitor.Visit(joinExpr, _expressionContext));
            _statementBuilder.Append(' ');

            return this;
        }
    }
}