using System;

namespace HB.Framework.Database.Entity
{
    internal interface IDatabaseEntityDefFactory
    {
        DatabaseEntityDef GetDef(Type entityType);
        DatabaseEntityDef GetDef<T>();
        int GetVarcharDefaultLength();
    }
}