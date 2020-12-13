#nullable enable

using System.Collections.Generic;

using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Entities;

namespace HB.FullStack.Database.SQL
{
    internal class SQLExpressionVisitorContenxt
    {
        private readonly IList<KeyValuePair<string, object>> _parameters = new List<KeyValuePair<string, object>>();

        public string Seperator { get; set; } = " ";

        public int ParamCounter { get; set; }

        public string ParamPlaceHolderPrefix { get; set; } = "_";

        public bool IsParameterized { get; set; } = true;

        public bool PrefixFieldWithTableName { get; set; } = true;

        public string? OrderByStatementBySQLUtilIn_QuotedColName { get; set; }

        public string[]? OrderByStatementBySQLUtilIn_Ins { get; set; }

        public IDatabaseEntityDefFactory EntityDefFactory { get; set; }

        public DatabaseEngineType EngineType { get; private set; }

        public SQLExpressionVisitorContenxt(IDatabaseEntityDefFactory databaseEntityDefFactory, DatabaseEngineType engineType)
        {
            EntityDefFactory = databaseEntityDefFactory;

            EngineType = engineType;
        }

        public void AddParameter(string key, object value)
        {
            _parameters.Add(new KeyValuePair<string, object>(key, value));
        }

        public IList<KeyValuePair<string, object>> GetParameters()
        {
            return _parameters;
        }

    }
}
