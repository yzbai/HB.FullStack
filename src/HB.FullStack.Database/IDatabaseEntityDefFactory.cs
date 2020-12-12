#nullable enable

using System;
using System.Collections.Generic;

using HB.FullStack.Common.Entities;

namespace HB.FullStack.Database.Entities
{
    internal interface IDatabaseEntityDefFactory
    {
        DatabaseEntityDef GetDef<T>() where T : Entity;
        int GetVarcharDefaultLength();

        IEnumerable<DatabaseEntityDef> GetAllDefsByDatabase(string databaseName);
        DatabaseEntityDef GetDef(Type entityType);
    }
}