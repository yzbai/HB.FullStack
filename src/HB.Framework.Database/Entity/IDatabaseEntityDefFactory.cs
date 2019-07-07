using System;
using System.Collections.Generic;

namespace HB.Framework.Database.Entity
{
    internal interface IDatabaseEntityDefFactory
    {
        DatabaseEntityDef GetDef(Type entityType);
        DatabaseEntityDef GetDef<T>();
        int GetVarcharDefaultLength();

        IEnumerable<DatabaseEntityDef> GetAllDefsByDatabase(string databaseName);
    }
}