using HB.Framework.Database;
using HB.Framework.Database.Engine;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.MySQL
{
    public class MySQLBuilder
    {
        public IDatabaseEngine DatabaseEngine { get; private set; }
        public IDatabaseSettings DatabaseSettings { get; private set; }

        private MySQLOptions _mysqlOptions;

        public MySQLBuilder() { }

        public MySQLBuilder SetMySqlOptions(MySQLOptions mySQLOptions)
        {
            _mysqlOptions = mySQLOptions;

            return this;
        }

        public MySQLBuilder Build()
        {
            if (_mysqlOptions == null)
            {
                throw new ArgumentNullException("mySQLOptions");
            }

            DatabaseSettings = _mysqlOptions.DatabaseSettings;

            DatabaseEngine = new MySQLEngine(_mysqlOptions);

            return this;
        }
    }
}
