﻿#nullable enable

using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Entities;
using HB.FullStack.Database.Properties;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace HB.FullStack.Database.SQL
{
    public enum SqlJoinType
    {
        INNER = 1,
        LEFT = 2,
        RIGHT = 3,
        FULL = 4,
        CROSS = 5
    }

    public class FromExpression<T> where T : Entity, new()
    {
        private readonly StringBuilder _statementBuilder = new StringBuilder();

        private readonly SQLExpressionVisitorContenxt _expressionContext;

        private readonly IDatabaseEntityDefFactory _entityDefFactory;

        public SqlJoinType? JoinType { get; set; }

        public IList<KeyValuePair<string, object>> GetParameters()
        {
            return _expressionContext.GetParameters();
        }

        public override string ToString()
        {
            return $" FROM {_entityDefFactory.GetDef<T>().DbTableReservedName} {_statementBuilder}";
        }

        internal FromExpression(IDatabaseEntityDefFactory databaseEntityDefFactory, DatabaseEngineType engineType)
        {
            _entityDefFactory = databaseEntityDefFactory;

            _expressionContext = new SQLExpressionVisitorContenxt(databaseEntityDefFactory, engineType)
            {
                ParamPlaceHolderPrefix = SqlHelper.ParameterizedChar + "f__"
            };
        }

        public FromExpression<T> InnerJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : Entity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.INNER)
            {
                throw new ArgumentException(Resources.SqlJoinTypeMixedErrorMessage);
            }

            JoinType = SqlJoinType.INNER;

            return InternalJoin<TTarget>("INNER JOIN", joinExpr);
        }

        public FromExpression<T> InnerJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinExpr)
            where TLeft : Entity, new()
            where TRight : Entity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.INNER)
            {
                throw new ArgumentException(Resources.SqlJoinTypeMixedErrorMessage);
            }

            JoinType = SqlJoinType.INNER;

            return InternalJoin<TRight>("INNER JOIN", joinExpr);
        }

        public FromExpression<T> LeftJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : Entity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.LEFT)
            {
                throw new ArgumentException(Resources.SqlJoinTypeMixedErrorMessage);
            }

            JoinType = SqlJoinType.LEFT;

            return InternalJoin<TTarget>("LEFT JOIN", joinExpr);
        }

        public FromExpression<T> LeftJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinExpr)
            where TLeft : Entity, new()
            where TRight : Entity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.LEFT)
            {
                throw new ArgumentException(Resources.SqlJoinTypeMixedErrorMessage);
            }

            JoinType = SqlJoinType.LEFT;

            return InternalJoin<TRight>("LEFT JOIN", joinExpr);
        }

        public FromExpression<T> RightJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : Entity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.RIGHT)
            {
                throw new ArgumentException(Resources.SqlJoinTypeMixedErrorMessage);
            }

            JoinType = SqlJoinType.RIGHT;

            return InternalJoin<TTarget>("RIGHT JOIN", joinExpr);
        }

        public FromExpression<T> RightJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinExpr)
            where TLeft : Entity, new()
            where TRight : Entity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.RIGHT)
            {
                throw new ArgumentException(Resources.SqlJoinTypeMixedErrorMessage);
            }

            JoinType = SqlJoinType.RIGHT;

            return InternalJoin<TRight>("RIGHT JOIN", joinExpr);
        }

        public FromExpression<T> FullJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : Entity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.FULL)
            {
                throw new ArgumentException(Resources.SqlJoinTypeMixedErrorMessage);
            }

            JoinType = SqlJoinType.FULL;

            return InternalJoin<TTarget>("FULL JOIN", joinExpr);
        }

        public FromExpression<T> FullJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinExpr)
            where TLeft : Entity, new()
            where TRight : Entity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.FULL)
            {
                throw new ArgumentException(Resources.SqlJoinTypeMixedErrorMessage);
            }

            JoinType = SqlJoinType.FULL;

            return InternalJoin<TRight>("FULL JOIN", joinExpr);
        }

        public FromExpression<T> CrossJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : Entity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.CROSS)
            {
                throw new ArgumentException(Resources.SqlJoinTypeMixedErrorMessage);
            }

            JoinType = SqlJoinType.CROSS;

            return InternalJoin<TTarget>("CROSS JOIN", joinExpr);
        }

        public FromExpression<T> CrossJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinExpr)
            where TLeft : Entity, new()
            where TRight : Entity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.CROSS)
            {
                throw new ArgumentException(Resources.SqlJoinTypeMixedErrorMessage);
            }

            JoinType = SqlJoinType.CROSS;

            return InternalJoin<TRight>("CROSS JOIN", joinExpr);
        }

        private FromExpression<T> InternalJoin<Target>(string joinType, Expression joinExpr) where Target : Entity
        {
            DatabaseEntityDef targetDef = _entityDefFactory.GetDef<Target>();

            _statementBuilder.Append(' ');
            _statementBuilder.Append(joinType);
            _statementBuilder.Append(' ');
            _statementBuilder.Append(targetDef.DbTableReservedName);
            _statementBuilder.Append(" ON ");
            _statementBuilder.Append(joinExpr.ToStatement(_expressionContext));
            _statementBuilder.Append(' ');

            return this;
        }


    }
}
