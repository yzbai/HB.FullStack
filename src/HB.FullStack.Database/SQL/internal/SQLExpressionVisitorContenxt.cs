

using System.Collections.Generic;

using HB.FullStack.Database.Engine;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database.SQL
{
    internal class SQLExpressionVisitorContenxt
    {
        private readonly IList<KeyValuePair<string, object>> _parameters = new List<KeyValuePair<string, object>>();

        public string Seperator { get; set; } = " ";

        public int ParamCounter { get; private set; }

        public string ParamPlaceHolderPrefix { get; set; } = "_";

        public bool PrefixFieldWithTableName { get; set; } = true;

        public string? OrderByStatementBySQLUtilIn_QuotedColName { get; set; }

        public string[]? OrderByStatementBySQLUtilIn_Ins { get; set; }

        public EngineType EngineType { get; private set; }
        
        public SQLExpressionVisitorContenxt(EngineType engineType)
        {
            EngineType = engineType;
        }

        public string GetNextParamPlaceholder()
        {
            return ParamPlaceHolderPrefix + ParamCounter++;
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