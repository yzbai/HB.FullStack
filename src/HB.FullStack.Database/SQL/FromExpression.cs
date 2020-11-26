#nullable enable

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

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FromExpression<T> where T : Entity, new()
    {
        private readonly StringBuilder _statementBuilder = new StringBuilder();

        private readonly IDatabaseEntityDefFactory _entityDefFactory;

        private readonly SQLExpressionVisitorContenxt _expressionContext;

        public bool WithFromString { get; set; } = true;

        public SqlJoinType? JoinType { get; set; }

        public IList<KeyValuePair<string, object>> GetParameters()
        {
            return _expressionContext.GetParameters();
        }

        public override string ToString()
        {
            StringBuilder resultBuilder = WithFromString ? new StringBuilder(" FROM ") : new StringBuilder(" ");

            resultBuilder.Append(_entityDefFactory.GetDef<T>().DbTableReservedName);
            resultBuilder.Append(_statementBuilder);

            return resultBuilder.ToString();
        }

        internal FromExpression(IDatabaseEngine databaseEngine, IDatabaseEntityDefFactory entityDefFactory)
        {
            _entityDefFactory = entityDefFactory;

            _expressionContext = new SQLExpressionVisitorContenxt(databaseEngine, entityDefFactory)
            {
                ParamPlaceHolderPrefix = databaseEngine.ParameterizedChar + "f__"
            };
        }

        /// <exception cref="System.ArgumentException"></exception>
        public FromExpression<T> InnerJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : Entity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.INNER)
            {
                throw new ArgumentException(Resources.SqlJoinTypeMixedErrorMessage);
            }

            JoinType = SqlJoinType.INNER;

            return InternalJoin<TTarget>("INNER JOIN", joinExpr);
        }

        /// <exception cref="System.ArgumentException"></exception>
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

        /// <exception cref="System.ArgumentException"></exception>
        public FromExpression<T> LeftJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : Entity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.LEFT)
            {
                throw new ArgumentException(Resources.SqlJoinTypeMixedErrorMessage);
            }

            JoinType = SqlJoinType.LEFT;

            return InternalJoin<TTarget>("LEFT JOIN", joinExpr);
        }

        /// <exception cref="System.ArgumentException"></exception>
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

        /// <exception cref="System.ArgumentException"></exception>
        public FromExpression<T> RightJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : Entity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.RIGHT)
            {
                throw new ArgumentException(Resources.SqlJoinTypeMixedErrorMessage);
            }

            JoinType = SqlJoinType.RIGHT;

            return InternalJoin<TTarget>("RIGHT JOIN", joinExpr);
        }

        /// <exception cref="System.ArgumentException"></exception>
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

        /// <exception cref="System.ArgumentException"></exception>
        public FromExpression<T> FullJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : Entity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.FULL)
            {
                throw new ArgumentException(Resources.SqlJoinTypeMixedErrorMessage);
            }

            JoinType = SqlJoinType.FULL;

            return InternalJoin<TTarget>("FULL JOIN", joinExpr);
        }

        /// <exception cref="System.ArgumentException"></exception>
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

        /// <exception cref="System.ArgumentException"></exception>
        public FromExpression<T> CrossJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : Entity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.CROSS)
            {
                throw new ArgumentException(Resources.SqlJoinTypeMixedErrorMessage);
            }

            JoinType = SqlJoinType.CROSS;

            return InternalJoin<TTarget>("CROSS JOIN", joinExpr);
        }

        /// <exception cref="System.ArgumentException"></exception>
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

        private FromExpression<T> InternalJoin<Target>(string joinType, Expression joinExpr)
        {
            DatabaseEntityDef targetDef = _entityDefFactory.GetDef(typeof(Target));

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
