#nullable enable

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Versioning;
using System.Text;
using HB.FullStack.Database.Def;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.SQL
{
    public enum SqlJoinType
    {
        None = 0,
        INNER = 1,
        LEFT = 2,
        RIGHT = 3,
        FULL = 4,
        CROSS = 5
    }

    public class FromExpression<T> where T : DatabaseEntity, new()
    {
        private readonly StringBuilder _statementBuilder = new StringBuilder();

        private readonly SQLExpressionVisitorContenxt _expressionContext;

        public SqlJoinType? JoinType { get; set; }

        public IList<KeyValuePair<string, object>> GetParameters()
        {
            return _expressionContext.GetParameters();
        }

        public string ToStatement()
        {
            return $" FROM {EntityDefFactory.GetDef<T>()!.DbTableReservedName} {_statementBuilder}";
        }

        internal FromExpression(EngineType engineType)
        {
            _expressionContext = new SQLExpressionVisitorContenxt(engineType)
            {
                ParamPlaceHolderPrefix = SqlHelper.ParameterizedChar + "f__"
            };
        }

        /// <summary>
        /// InnerJoin
        /// </summary>
        /// <param name="joinExpr"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public FromExpression<T> InnerJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.INNER)
            {
                throw Exceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.INNER;

            return InternalJoin<TTarget>("INNER JOIN", joinExpr);
        }

        /// <summary>
        /// InnerJoin
        /// </summary>
        /// <param name="joinExpr"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public FromExpression<T> InnerJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinExpr)
            where TLeft : DatabaseEntity, new()
            where TRight : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.INNER)
            {
                throw Exceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.INNER;

            return InternalJoin<TRight>("INNER JOIN", joinExpr);
        }

        /// <summary>
        /// LeftJoin
        /// </summary>
        /// <param name="joinExpr"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public FromExpression<T> LeftJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.LEFT)
            {
                throw Exceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.LEFT;

            return InternalJoin<TTarget>("LEFT JOIN", joinExpr);
        }

        /// <summary>
        /// LeftJoin
        /// </summary>
        /// <param name="joinExpr"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public FromExpression<T> LeftJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinExpr)
            where TLeft : DatabaseEntity, new()
            where TRight : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.LEFT)
            {
                throw Exceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.LEFT;

            return InternalJoin<TRight>("LEFT JOIN", joinExpr);
        }

        /// <summary>
        /// RightJoin
        /// </summary>
        /// <param name="joinExpr"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public FromExpression<T> RightJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.RIGHT)
            {
                throw Exceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.RIGHT;

            return InternalJoin<TTarget>("RIGHT JOIN", joinExpr);
        }

        /// <summary>
        /// RightJoin
        /// </summary>
        /// <param name="joinExpr"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public FromExpression<T> RightJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinExpr)
            where TLeft : DatabaseEntity, new()
            where TRight : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.RIGHT)
            {
                throw Exceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.RIGHT;

            return InternalJoin<TRight>("RIGHT JOIN", joinExpr);
        }

        /// <summary>
        /// FullJoin
        /// </summary>
        /// <param name="joinExpr"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public FromExpression<T> FullJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.FULL)
            {
                throw Exceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.FULL;

            return InternalJoin<TTarget>("FULL JOIN", joinExpr);
        }

        /// <summary>
        /// FullJoin
        /// </summary>
        /// <param name="joinExpr"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public FromExpression<T> FullJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinExpr)
            where TLeft : DatabaseEntity, new()
            where TRight : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.FULL)
            {
                throw Exceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.FULL;

            return InternalJoin<TRight>("FULL JOIN", joinExpr);
        }

        /// <summary>
        /// CrossJoin
        /// </summary>
        /// <param name="joinExpr"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public FromExpression<T> CrossJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.CROSS)
            {
                throw Exceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.CROSS;

            return InternalJoin<TTarget>("CROSS JOIN", joinExpr);
        }

        /// <summary>
        /// CrossJoin
        /// </summary>
        /// <param name="joinExpr"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException">Ignore.</exception>
        public FromExpression<T> CrossJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinExpr)
            where TLeft : DatabaseEntity, new()
            where TRight : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.CROSS)
            {
                throw Exceptions.SqlJoinTypeMixedError();
            }

            JoinType = SqlJoinType.CROSS;

            return InternalJoin<TRight>("CROSS JOIN", joinExpr);
        }

        /// <summary>
        /// InternalJoin
        /// </summary>
        /// <param name="joinType"></param>
        /// <param name="joinExpr"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private FromExpression<T> InternalJoin<Target>(string joinType, Expression joinExpr) where Target : DatabaseEntity
        {
            EntityDef targetDef = EntityDefFactory.GetDef<Target>()!;

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