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
        public IDatabase Database { get; private set; }

        private IDatabaseSettings _databaseSettings;

        private IDatabaseEngine _databaseEngine;

        public DatabaseBuilder()
        {

        }

        public DatabaseBuilder SetDatabaseSettings(IDatabaseSettings databaseSettings)
        {
            _databaseSettings = databaseSettings;

            return this;
        }

        public DatabaseBuilder SetDatabaseEngine(IDatabaseEngine databaseEngine)
        {
            _databaseEngine = databaseEngine;

            return this;
        }

        public IDatabase Build()
        {
            if (_databaseEngine == null)
            {
                throw new ArgumentNullException("databaseEngine");
            }

            if (_databaseSettings == null)
            {
                throw new ArgumentNullException("databaseSettings");
            }

            IDatabaseTypeConverterFactory databaseTypeConverterFactory = new DatabaseTypeConverterFactory();
            IDatabaseEntityDefFactory databaseEntityDefFactory = new DefaultDatabaseEntityDefFactory(_databaseSettings, _databaseEngine, databaseTypeConverterFactory);
            IDatabaseEntityMapper databaseEntityMapper = new DefaultDatabaseEntityMapper(databaseEntityDefFactory);
            ISQLBuilder sQLBuilder = new SQLBuilder(_databaseEngine, databaseEntityDefFactory);

            return new DefaultDatabase(_databaseSettings, _databaseEngine, databaseEntityDefFactory, databaseEntityMapper, sQLBuilder);
        }

    }
}
