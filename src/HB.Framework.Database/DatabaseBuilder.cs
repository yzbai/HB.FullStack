using HB.Framework.Database.Engine;
using HB.Framework.Database.Entity;
using HB.Framework.Database.SQL;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.Database
{
    public class DatabaseBuilder
    {
        private IDatabaseEngine _databaseEngine;

        public DatabaseBuilder(IDatabaseEngine databaseEngine)
        {
            _databaseEngine = databaseEngine;
        }

        public IDatabase Build()
        {
            if (_databaseEngine == null)
            {
                throw new ArgumentNullException("databaseEngine");
            }

            IDatabaseTypeConverterFactory databaseTypeConverterFactory = new DatabaseTypeConverterFactory();
            IDatabaseEntityDefFactory databaseEntityDefFactory = new DefaultDatabaseEntityDefFactory(_databaseEngine.DatabaseSettings, _databaseEngine, databaseTypeConverterFactory);
            IDatabaseEntityMapper databaseEntityMapper = new DefaultDatabaseEntityMapper(databaseEntityDefFactory);
            ISQLBuilder sQLBuilder = new SQLBuilder(_databaseEngine, databaseEntityDefFactory);

            return new DefaultDatabase(_databaseEngine.DatabaseSettings, _databaseEngine, databaseEntityDefFactory, databaseEntityMapper, sQLBuilder);
        }

    }
}
