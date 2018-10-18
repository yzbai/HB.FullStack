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
    public class From<T> : SQLExpression
        where T : DatabaseEntity, new()
    {
        private StringBuilder _statementBuilder;

        private DatabaseEntityDef _sourceEntityDef;

        private readonly IDatabaseEngine _databaseEngine;

        public bool WithFromString { get; set; }

        public override string ToString()
        {
            StringBuilder resultBuilder = WithFromString ? new StringBuilder(" FROM ") : new StringBuilder(" ");

            resultBuilder.Append(_sourceEntityDef.DbTableReservedName);
            resultBuilder.Append(_statementBuilder);

            return resultBuilder.ToString();
        }

        public From(IDatabaseEngine databaseEngine, IDatabaseEntityDefFactory entityDefFactory) : base(entityDefFactory)
        {
            _entityDefFactory = entityDefFactory;
            _sourceEntityDef = _entityDefFactory.Get<T>();
            _databaseEngine = databaseEngine;

            _sep = " ";
            PrefixFieldWithTableName = true;
            WithFromString = true;

            _statementBuilder = new StringBuilder();
        }

        protected override IDatabaseEngine GetDatabaseEngine()
        {
            return _databaseEngine;
        }

        public From<T> InnerJoin<Target>(Expression<Func<T, Target, bool>> joinExpr) where Target : DatabaseEntity, new()
        {
            return InternalJoin<Target>("INNER JOIN", joinExpr);
        }

        public From<T> LeftJoin<Target>(Expression<Func<T, Target, bool>> joinExpr) where Target : DatabaseEntity, new()
        {
            return InternalJoin<Target>("LEFT JOIN", joinExpr);
        }

        public From<T> RightJoin<Target>(Expression<Func<T, Target, bool>> joinExpr) where Target : DatabaseEntity, new()
        {
            return InternalJoin<Target>("RIGHT JOIN", joinExpr);
        }

        public From<T> FullJoin<Target>(Expression<Func<T, Target, bool>> joinExpr) where Target : DatabaseEntity, new()
        {
            return InternalJoin<Target>("FULL JOIN", joinExpr);
        }

        public From<T> CrossJoin<Target>(Expression<Func<T, Target, bool>> joinExpr) where Target : DatabaseEntity, new()
        {
            return InternalJoin<Target>("CROSS JOIN", joinExpr);
        }

        private From<T> InternalJoin<Target>(string joinType, Expression joinExpr)
        {
            DatabaseEntityDef targetDef = _entityDefFactory.Get(typeof(Target));

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
