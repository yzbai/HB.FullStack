

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Versioning;
using System.Text;
using HB.FullStack.Database.Entities;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.SQL
{
    public class FromExpression<T> where T : DatabaseEntity, new()
    {
        private readonly StringBuilder _statementBuilder = new StringBuilder();

        private readonly SQLExpressionVisitorContenxt _expressionContext;
        private readonly IEntityDefFactory _entityDefFactory;
        private readonly ISQLExpressionVisitor _expressionVisitor;

        public SqlJoinType? JoinType { get; set; }

        public IList<KeyValuePair<string, object>> GetParameters()
        {
            return _expressionContext.GetParameters();
        }

        public string ToStatement()
        {
            return $" FROM {_entityDefFactory.GetDef<T>()!.DbTableReservedName} {_statementBuilder}";
        }

        internal FromExpression(EngineType engineType, IEntityDefFactory entityDefFactory, ISQLExpressionVisitor expressionVisitor)
        {
            _expressionContext = new SQLExpressionVisitorContenxt(engineType)
            {
                ParamPlaceHolderPrefix = SqlHelper.PARAMETERIZED_CHAR + "f__"
            };

            _entityDefFactory = entityDefFactory;
            _expressionVisitor = expressionVisitor;
        }

        public FromExpression<T> InnerJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.INNER)
            {
                throw DatabaseExceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.INNER;

            return InternalJoin<TTarget>("INNER JOIN", joinExpr);
        }

        public FromExpression<T> InnerJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinExpr)
            where TLeft : DatabaseEntity, new()
            where TRight : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.INNER)
            {
                throw DatabaseExceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.INNER;

            return InternalJoin<TRight>("INNER JOIN", joinExpr);
        }

        public FromExpression<T> LeftJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.LEFT)
            {
                throw DatabaseExceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.LEFT;

            return InternalJoin<TTarget>("LEFT JOIN", joinExpr);
        }

        public FromExpression<T> LeftJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinExpr)
            where TLeft : DatabaseEntity, new()
            where TRight : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.LEFT)
            {
                throw DatabaseExceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.LEFT;

            return InternalJoin<TRight>("LEFT JOIN", joinExpr);
        }

        public FromExpression<T> RightJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.RIGHT)
            {
                throw DatabaseExceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.RIGHT;

            return InternalJoin<TTarget>("RIGHT JOIN", joinExpr);
        }

        public FromExpression<T> RightJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinExpr)
            where TLeft : DatabaseEntity, new()
            where TRight : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.RIGHT)
            {
                throw DatabaseExceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.RIGHT;

            return InternalJoin<TRight>("RIGHT JOIN", joinExpr);
        }

        public FromExpression<T> FullJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.FULL)
            {
                throw DatabaseExceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.FULL;

            return InternalJoin<TTarget>("FULL JOIN", joinExpr);
        }

        public FromExpression<T> FullJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinExpr)
            where TLeft : DatabaseEntity, new()
            where TRight : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.FULL)
            {
                throw DatabaseExceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.FULL;

            return InternalJoin<TRight>("FULL JOIN", joinExpr);
        }

        public FromExpression<T> CrossJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.CROSS)
            {
                throw DatabaseExceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.CROSS;

            return InternalJoin<TTarget>("CROSS JOIN", joinExpr);
        }

        public FromExpression<T> CrossJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinExpr)
            where TLeft : DatabaseEntity, new()
            where TRight : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.CROSS)
            {
                throw DatabaseExceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.CROSS;

            return InternalJoin<TRight>("CROSS JOIN", joinExpr);
        }

        private FromExpression<T> InternalJoin<Target>(string joinType, Expression joinExpr) where Target : DatabaseEntity
        {
            EntityDef targetDef = _entityDefFactory.GetDef<Target>()!;

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