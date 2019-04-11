using HB.Framework.Database.Engine;
using HB.Framework.Database.Entity;
using System;
using System.Linq.Expressions;
using System.Text;

namespace HB.Framework.Database.SQL
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FromExpression<T> : SQLExpression
        where T : DatabaseEntity, new()
    {
        private readonly StringBuilder _statementBuilder;

        private readonly DatabaseEntityDef _sourceEntityDef;

        private readonly IDatabaseEngine _databaseEngine;

        public bool WithFromString { get; set; }

        public override string ToString()
        {
            StringBuilder resultBuilder = WithFromString ? new StringBuilder(" FROM ") : new StringBuilder(" ");

            resultBuilder.Append(_sourceEntityDef.DbTableReservedName);
            resultBuilder.Append(_statementBuilder);

            return resultBuilder.ToString();
        }

        public FromExpression(IDatabaseEngine databaseEngine, IDatabaseEntityDefFactory entityDefFactory) : base(entityDefFactory)
        {
            EntityDefFactory = entityDefFactory;
            _sourceEntityDef = EntityDefFactory.GetDef<T>();
            _databaseEngine = databaseEngine;

            Seperator = " ";
            PrefixFieldWithTableName = true;
            WithFromString = true;

            _statementBuilder = new StringBuilder();
        }

        protected override IDatabaseEngine GetDatabaseEngine()
        {
            return _databaseEngine;
        }

        public FromExpression<T> InnerJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : DatabaseEntity, new()
        {
            return InternalJoin<TTarget>("INNER JOIN", joinExpr);
        }

        public FromExpression<T> LeftJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : DatabaseEntity, new()
        {
            return InternalJoin<TTarget>("LEFT JOIN", joinExpr);
        }

        public FromExpression<T> RightJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : DatabaseEntity, new()
        {
            return InternalJoin<TTarget>("RIGHT JOIN", joinExpr);
        }

        public FromExpression<T> FullJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : DatabaseEntity, new()
        {
            return InternalJoin<TTarget>("FULL JOIN", joinExpr);
        }

        public FromExpression<T> CrossJoin<TTarget>(Expression<Func<T, TTarget, bool>> joinExpr) where TTarget : DatabaseEntity, new()
        {
            return InternalJoin<TTarget>("CROSS JOIN", joinExpr);
        }

        private FromExpression<T> InternalJoin<Target>(string joinType, Expression joinExpr)
        {
            DatabaseEntityDef targetDef = EntityDefFactory.GetDef(typeof(Target));

            _statementBuilder.Append(" ");
            _statementBuilder.Append(joinType);
            _statementBuilder.Append(" ");
            _statementBuilder.Append(targetDef.DbTableReservedName);
            _statementBuilder.Append(" ON ");
            _statementBuilder.Append(Visit(joinExpr));
            _statementBuilder.Append(" ");

            return this;
        }


    }
}
