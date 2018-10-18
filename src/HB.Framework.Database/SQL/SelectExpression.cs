using HB.Framework.Database.Engine;
using HB.Framework.Database.Entity;
using System;
using System.Linq.Expressions;
using System.Text;

namespace HB.Framework.Database.SQL
{
    public class SelectExpression<T> : SQLExpression
        where T : DatabaseEntity, new()
    {
        private StringBuilder _statementBuilder;

        private bool _firstAssign;

        private readonly DatabaseEntityDef _sourceModelDef;
        private readonly IDatabaseEngine _databaseEngine;

        public bool WithSelectString { get; set; }

        public override string ToString()
        {
            StringBuilder resultBuilder = WithSelectString ? new StringBuilder(" SELECT ") : new StringBuilder(" ");

            resultBuilder.Append(_statementBuilder);
            resultBuilder.Append(" ");

            return resultBuilder.ToString();
        }

        public SelectExpression(IDatabaseEngine databaseEngine, IDatabaseEntityDefFactory modelDefFactory) : base(modelDefFactory)
        {
            EntityDefFactory = modelDefFactory;
            _sourceModelDef = EntityDefFactory.GetDef<T>();
            _databaseEngine = databaseEngine;

            Seperator = " ";
            PrefixFieldWithTableName = true;
            WithSelectString = true;

            _statementBuilder = new StringBuilder();
            _firstAssign = true;

            //DefaultInvolved();

        }

        /*这部分逻辑应该放在DefaultDatabase中去
        private void DefaultInvolved()
        {
            this.select(item=>item.Id)
                .select(item=>item.Deleted)
                .select(item=>item.LastTime)
                .select(item=>item.LastUser)
                .select(item=>item.Version);
        }
        */

        protected override IDatabaseEngine GetDatabaseEngine()
        {
            return _databaseEngine;
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

            _statementBuilder.Append(Visit(expr));

            return this;
        }
    }
}
