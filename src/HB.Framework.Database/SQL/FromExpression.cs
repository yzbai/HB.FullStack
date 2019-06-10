using HB.Framework.Database.Engine;
using HB.Framework.Database.Entity;
using System;
using System.Linq.Expressions;
using System.Text;

namespace HB.Framework.Database.SQL
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
    public class FromExpression<T> where T : DatabaseEntity, new()
    {
        private readonly StringBuilder _statementBuilder = new StringBuilder();

        private readonly IDatabaseEntityDefFactory entityDefFactory;

        private readonly IDatabaseEngine _databaseEngine;

        private SQLExpressionVisitorContenxt expressionContext = null;

        public bool WithFromString { get; set; } = true;

        public SqlJoinType? JoinType { get; set; }

        public override string ToString()
        {
            StringBuilder resultBuilder = WithFromString ? new StringBuilder(" FROM ") : new StringBuilder(" ");

            resultBuilder.Append(entityDefFactory.GetDef<T>().DbTableReservedName);
            resultBuilder.Append(_statementBuilder);

            return resultBuilder.ToString();
        }

        internal FromExpression(IDatabaseEngine databaseEngine, IDatabaseEntityDefFactory entityDefFactory)
        {
            this.entityDefFactory = entityDefFactory;
            _databaseEngine = databaseEngine;
            expressionContext = new SQLExpressionVisitorContenxt(databaseEngine, entityDefFactory);
        }

        public FromExpression<T> InnerJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.INNER)
            {
                throw new ArgumentException("DO NOT MIX JOIN UP");
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
                throw new ArgumentException("DO NOT MIX JOIN UP");
            }

            JoinType = SqlJoinType.INNER;

            return InternalJoin<TRight>("INNER JOIN", joinExpr);
        }

        public FromExpression<T> LeftJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.LEFT)
            {
                throw new ArgumentException("DO NOT MIX JOIN UP");
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
                throw new ArgumentException("DO NOT MIX JOIN UP");
            }

            JoinType = SqlJoinType.LEFT;

            return InternalJoin<TRight>("LEFT JOIN", joinExpr);
        }

        public FromExpression<T> RightJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.RIGHT)
            {
                throw new ArgumentException("DO NOT MIX JOIN UP");
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
                throw new ArgumentException("DO NOT MIX JOIN UP");
            }

            JoinType = SqlJoinType.RIGHT;

            return InternalJoin<TRight>("RIGHT JOIN", joinExpr);
        }

        public FromExpression<T> FullJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.FULL)
            {
                throw new ArgumentException("DO NOT MIX JOIN UP");
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
                throw new ArgumentException("DO NOT MIX JOIN UP");
            }

            JoinType = SqlJoinType.FULL;

            return InternalJoin<TRight>("FULL JOIN", joinExpr);
        }

        public FromExpression<T> CrossJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : DatabaseEntity, new()
        {
            if (JoinType != null && JoinType != SqlJoinType.CROSS)
            {
                throw new ArgumentException("DO NOT MIX JOIN UP");
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
                throw new ArgumentException("DO NOT MIX JOIN UP");
            }

            JoinType = SqlJoinType.CROSS;

            return InternalJoin<TRight>("CROSS JOIN", joinExpr);
        }

        private FromExpression<T> InternalJoin<Target>(string joinType, Expression joinExpr)
        {
            DatabaseEntityDef targetDef = entityDefFactory.GetDef(typeof(Target));

            _statementBuilder.Append(" ");
            _statementBuilder.Append(joinType);
            _statementBuilder.Append(" ");
            _statementBuilder.Append(targetDef.DbTableReservedName);
            _statementBuilder.Append(" ON ");
            _statementBuilder.Append(joinExpr.ToStatement(expressionContext));
            _statementBuilder.Append(" ");

            return this;
        }


    }
}
