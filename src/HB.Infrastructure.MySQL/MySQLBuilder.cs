using HB.Framework.Database;
using HB.Framework.Database.Engine;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.MySQL
{
    public class MySQLBuilder
    {
        private MySQLOptions _mysqlOptions;

        private MySQLBuilder() { }

        public MySQLBuilder (MySQLOptions mySQLOptions)
        {
            _mysqlOptions = mySQLOptions;

        }

        public IDatabaseEngine Build()
        {
            if (_mysqlOptions == null)
            {
                throw new ArgumentNullException("mySQLOptions");
            }

            return  new MySQLEngine(_mysqlOptions);
        }
    }
}
