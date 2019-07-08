using System;
using System.Collections.Generic;
using System.Text;
using HB.Framework.Database;
using HB.Framework.Database.Engine;

namespace HB.Infrastructure.SQLite
{
    public class SQLiteBuilder
    {
        public IDatabaseEngine DatabaseEngine { get; private set; }
        public IDatabaseSettings DatabaseSettings { get; private set; }

        private SQLiteOptions _sqliteOptions;

        public SQLiteBuilder() { }

        public SQLiteBuilder SetSQLiteOptions(SQLiteOptions sqliteOptions)
        {
            _sqliteOptions = sqliteOptions;

            return this;
        }

        public SQLiteBuilder Build()
        {
            if (_sqliteOptions == null)
            {
                throw new ArgumentNullException("sqliteOptions");
            }

            DatabaseSettings = _sqliteOptions.DatabaseSettings;

            DatabaseEngine = new SQLiteEngine(_sqliteOptions);

            return this;
        }
    }
}
