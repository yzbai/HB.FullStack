using System.Collections.Generic;
using HB.Framework.Database.Entity;
using HB.Framework.Database.Engine;

namespace HB.Framework.Database.SQL
{
    internal class SQLExpressionVisitorContenxt
    {
        private IList<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();

        public string Seperator { get; set; } = " ";

        public int ParamCounter { get; set; } = 0;

        public string ParamPlaceHolderPrefix { get; set; } = "_";

        public bool IsParameterized { get; set; } = true;

        public bool PrefixFieldWithTableName { get; set; } = true;


        public IDatabaseEngine DatabaesEngine { get; set; }

        public IDatabaseEntityDefFactory EntityDefFactory { get; set; }

        public SQLExpressionVisitorContenxt(IDatabaseEngine databaseEngine, IDatabaseEntityDefFactory databaseEntityDefFactory)
        {
            DatabaesEngine = databaseEngine;
            EntityDefFactory = databaseEntityDefFactory;
        }

        public void AddParameter(string key, object value)
        {
            parameters.Add(new KeyValuePair<string, object>(key, value));
        }

        public IList<KeyValuePair<string, object>> GetParameters()
        {
            return parameters;
        }

    }
}
