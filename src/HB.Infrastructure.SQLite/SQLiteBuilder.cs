using System;
using System.Collections.Generic;
using System.Text;
using HB.Framework.Database;
using HB.Framework.Database.Engine;

namespace HB.Infrastructure.SQLite
{
    public class SQLiteBuilder
    {
        private SQLiteOptions _sqliteOptions;

        private SQLiteBuilder() { }

        public SQLiteBuilder(SQLiteOptions sqliteOptions)
        {
            _sqliteOptions = sqliteOptions;
        }

        public IDatabaseEngine Build()
        {
            if (_sqliteOptions == null)
            {
                throw new ArgumentNullException("sqliteOptions");
            }

            return new SQLiteEngine(_sqliteOptions);
        }
    }
}
