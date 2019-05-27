using System;

namespace HB.Framework.Database.Entity
{
    public interface IDatabaseEntityDefFactory
    {
        DatabaseEntityDef GetDef(Type entityType);
        DatabaseEntityDef GetDef<T>();
        int GetVarcharDefaultLength();
    }
}